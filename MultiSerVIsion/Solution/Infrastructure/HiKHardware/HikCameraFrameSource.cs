using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Sources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    /// <summary>
    /// 海康相机图源：把驱动采集到的帧转换为「多消费者各自独立深拷贝」的输出。
    /// 【拷贝语义】每次发布都为每个消费者单独复制像素缓冲，绝不共享驱动回调内部缓冲；
    /// 消费者拿到即拥有该帧，可安全用于渲染或算法处理，无需（也无法）释放。
    /// 【线程安全】订阅表为并发字典；发布可能来自采集线程，订阅/退订可与发布并发执行。
    /// </summary>
    public sealed class HikCameraFrameSource : IFrameSource
    {
        /// <summary>消费者槽位：保存回调与其订阅句柄</summary>
        private sealed class ConsumerSlot
        {
            /// <summary>帧回调</summary>
            public Action<CameraFrame> OnFrame;

            /// <summary>订阅句柄（供释放时退订）</summary>
            public IDisposable Handle;
        }

        /// <summary>订阅句柄：释放即退订，且幂等</summary>
        private sealed class SubscriptionHandle : IDisposable
        {
            private readonly HikCameraFrameSource _owner;
            private readonly string _consumerKey;
            private int _disposed;

            public SubscriptionHandle(HikCameraFrameSource owner, string consumerKey)
            {
                _owner = owner;
                _consumerKey = consumerKey;
            }

            public void Dispose()
            {
                // 幂等：重复释放不得退订其他消费者的订阅
                if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
                _owner.Unsubscribe(_consumerKey);
            }
        }

        /// <summary>消费者订阅表（key = 消费者标识）</summary>
        private readonly ConcurrentDictionary<string, ConsumerSlot> _consumers
            = new ConcurrentDictionary<string, ConsumerSlot>(StringComparer.Ordinal);

        /// <summary>保护状态字段的锁</summary>
        private readonly object _stateLock = new object();

        /// <summary>累计帧数</summary>
        private long _frameCount;

        /// <summary>最近一帧时间（UTC）</summary>
        private DateTime _lastFrameUtc = DateTime.MinValue;

        /// <summary>是否正在出帧</summary>
        private bool _isStreaming;

        /// <summary>当前采集模式</summary>
        private AcquisitionMode _mode = AcquisitionMode.Continuous;

        /// <summary>关联的设备 ID</summary>
        public string DeviceId { get; }

        /// <summary>当前图源状态快照（只读，可安全跨线程读取）</summary>
        public FrameSourceState State
        {
            get
            {
                lock (_stateLock)
                {
                    return new FrameSourceState(DeviceId, _isStreaming, _mode,
                        _frameCount, _lastFrameUtc, _consumers.Count);
                }
            }
        }

        /// <summary>当前已订阅的消费者标识集合（只读快照）</summary>
        public IReadOnlyCollection<string> ConsumerKeys => _consumers.Keys.ToArray();

        /// <summary>
        /// 构造图源
        /// </summary>
        /// <param name="deviceId">关联的设备 ID。</param>
        public HikCameraFrameSource(string deviceId)
        {
            DeviceId = deviceId ?? string.Empty;
        }

        /// <summary>
        /// 订阅帧输出。
        /// </summary>
        /// <param name="consumerKey">消费者唯一标识（如 "Preview" / "Algorithm.Recipe01"）。</param>
        /// <param name="onFrame">帧回调；收到的帧为本消费者独占的深拷贝。</param>
        /// <returns>订阅句柄，Dispose 等价于退订。</returns>
        /// <exception cref="ArgumentNullException">consumerKey 为空或 onFrame 为 null。</exception>
        /// <exception cref="InvalidOperationException">同一 consumerKey 重复订阅。</exception>
        public IDisposable Subscribe(string consumerKey, Action<CameraFrame> onFrame)
        {
            if (string.IsNullOrWhiteSpace(consumerKey))
                throw new ArgumentNullException(nameof(consumerKey), "消费者标识不能为空");
            if (onFrame == null)
                throw new ArgumentNullException(nameof(onFrame), "帧回调不能为空");

            var handle = new SubscriptionHandle(this, consumerKey);
            var slot = new ConsumerSlot { OnFrame = onFrame, Handle = handle };

            if (!_consumers.TryAdd(consumerKey, slot))
                throw new InvalidOperationException($"消费者 {consumerKey} 已订阅，禁止重复订阅");

            return handle;
        }

        /// <summary>
        /// 取消订阅。
        /// </summary>
        /// <param name="consumerKey">订阅时使用的消费者唯一标识。</param>
        /// <returns>实际移除返回 true；不存在返回 false。</returns>
        public bool Unsubscribe(string consumerKey)
        {
            if (string.IsNullOrWhiteSpace(consumerKey)) return false;
            return _consumers.TryRemove(consumerKey, out _);
        }

        /// <summary>
        /// 更新取流状态与采集模式（由驱动在启停流、切换采集模式时调用）。
        /// </summary>
        /// <param name="isStreaming">是否正在出帧。</param>
        /// <param name="mode">当前采集模式。</param>
        internal void UpdateState(bool isStreaming, AcquisitionMode mode)
        {
            lock (_stateLock)
            {
                _isStreaming = isStreaming;
                _mode = mode;
            }
        }

        /// <summary>
        /// 发布一帧：为每个消费者独立深拷贝后回调。
        /// 【线程】由采集线程调用，回调内不得执行长耗时操作，否则会阻塞采集。
        /// </summary>
        /// <param name="frame">驱动采集到的帧；其缓冲可能被驱动复用，本方法会立即复制。</param>
        internal void Publish(CameraFrame frame)
        {
            if (frame == null) return;

            lock (_stateLock)
            {
                _frameCount++;
                _lastFrameUtc = DateTime.UtcNow;
            }

            if (_consumers.IsEmpty) return;

            // 快照遍历：避免回调中退订引发枚举异常
            foreach (var pair in _consumers.ToArray())
            {
                var action = pair.Value?.OnFrame;
                if (action == null) continue;

                try
                {
                    action(CloneFrame(frame));
                }
                catch (Exception)
                {
                    // 单个消费者异常不得影响其他消费者与采集线程
                }
            }
        }

        /// <summary>
        /// 帧深拷贝：像素缓冲独立分配，消费者获得完全所有权。
        /// </summary>
        /// <param name="source">源帧。</param>
        /// <returns>独立副本。</returns>
        private static CameraFrame CloneFrame(CameraFrame source)
        {
            int length = source.Data?.Length ?? 0;
            var data = new byte[length];

            if (length > 0)
                Buffer.BlockCopy(source.Data, 0, data, 0, length);

            return new CameraFrame
            {
                Width = source.Width,
                Height = source.Height,
                PixelFormat = source.PixelFormat,
                Data = data,
                Timestamp = source.Timestamp,
                FrameId = source.FrameId
            };
        }
    }
}
