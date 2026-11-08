using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using MultiSerVIsion.Solution.Domain.Enums;
using System;

namespace MultiSerVIsion.Solution.Domain.Camera
{
    /// <summary>
    /// 图源状态快照（构造后不可变），供消费者查询当前流状态与帧计数。
    /// 【线程安全】所有属性均为只读且在构造时确定，可安全跨线程读取与传递。
    /// </summary>
    public sealed class FrameSourceState
    {
        /// <summary>关联的设备 ID（组态设备标识）。</summary>
        public string DeviceId { get; }

        /// <summary>是否正在出帧。</summary>
        public bool IsStreaming { get; }

        /// <summary>当前采集模式。</summary>
        public AcquisitionMode Mode { get; }

        /// <summary>自该图源创建起累计产生的帧数（图源视角，非单消费者视角）。</summary>
        public long FrameCount { get; }

        /// <summary>最近一次帧到达的时间（UTC）；尚未出帧时为 <see cref="DateTime.MinValue"/>。</summary>
        public DateTime LastFrameUtc { get; }

        /// <summary>当前订阅的消费者数量。</summary>
        public int ConsumerCount { get; }

        /// <summary>
        /// 构造图源状态快照
        /// </summary>
        /// <param name="deviceId">关联的设备 ID</param>
        /// <param name="isStreaming">是否正在出帧</param>
        /// <param name="mode">当前采集模式</param>
        /// <param name="frameCount">自图源创建起累计帧数</param>
        /// <param name="lastFrameUtc">最近一次帧到达时间（UTC）</param>
        /// <param name="consumerCount">当前订阅者数量</param>
        public FrameSourceState(string deviceId, bool isStreaming, AcquisitionMode mode,
            long frameCount, DateTime lastFrameUtc, int consumerCount)
        {
            DeviceId = deviceId;
            IsStreaming = isStreaming;
            Mode = mode;
            FrameCount = frameCount;
            LastFrameUtc = lastFrameUtc;
            ConsumerCount = consumerCount;
        }
    }
}
