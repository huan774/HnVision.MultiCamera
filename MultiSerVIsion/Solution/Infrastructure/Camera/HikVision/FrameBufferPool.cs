using MultiSerVIsion.Solution.Domain.Camera;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MultiSerVIsion.Solution.Infrastructure.Camera.HikVision
{
    /// <summary>
    /// 帧缓冲池实现：复用像素缓冲，降低高帧率下的分配次数与 GC 压力。
    /// 【线程安全】空闲队列为 <see cref="ConcurrentBag{T}"/>，租用/归还均可并发调用。
    /// 【所有权】租出的缓冲归调用方独占，用完必须归还；归还后不得再访问（可能被复用）。
    /// 【防错】重复归还或归还非本池缓冲视为编码错误，直接抛异常以便尽早暴露。
    /// </summary>
    public sealed class FrameBufferPool : IFrameBufferPool
    {
        /// <summary>空闲缓冲队列</summary>
        private readonly ConcurrentBag<byte[]> _idle = new ConcurrentBag<byte[]>();

        /// <summary>已租出缓冲集合，用于校验归还归属</summary>
        private readonly System.Collections.Generic.HashSet<byte[]> _rented
            = new System.Collections.Generic.HashSet<byte[]>();

        /// <summary>保护 _rented 的锁（ConcurrentBag 无法做归属校验）</summary>
        private readonly object _rentedLock = new object();

        /// <summary>累计租出次数</summary>
        private long _totalRentCount;

        /// <summary>当前池中可复用的缓冲数量</summary>
        public int PooledBufferCount => _idle.Count;

        /// <summary>累计租出次数（用于观测复用效率）</summary>
        public long TotalRentCount => Interlocked.Read(ref _totalRentCount);

        /// <summary>
        /// 租用一块至少指定长度的缓冲。
        /// </summary>
        /// <param name="minimumLength">所需最小字节数，必须大于 0。</param>
        /// <returns>可写缓冲，长度不小于 minimumLength。</returns>
        /// <exception cref="ArgumentOutOfRangeException">minimumLength 小于等于 0。</exception>
        public byte[] Rent(int minimumLength)
        {
            if (minimumLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength), "缓冲长度必须大于 0");

            // 复用容量足够的缓冲；容量不足的直接丢弃，避免小请求占住大缓冲后反复扩容
            byte[] buffer = null;
            while (_idle.TryTake(out var candidate))
            {
                if (candidate.Length >= minimumLength)
                {
                    buffer = candidate;
                    break;
                }
            }

            if (buffer == null)
                buffer = new byte[minimumLength];

            lock (_rentedLock)
            {
                _rented.Add(buffer);
            }

            Interlocked.Increment(ref _totalRentCount);
            return buffer;
        }

        /// <summary>
        /// 归还缓冲，使其可被复用。
        /// </summary>
        /// <param name="buffer">待归还缓冲；为 null 时忽略。</param>
        /// <exception cref="InvalidOperationException">缓冲不属于本池或已被归还。</exception>
        public void Return(byte[] buffer)
        {
            if (buffer == null) return;

            lock (_rentedLock)
            {
                if (!_rented.Remove(buffer))
                    throw new InvalidOperationException("缓冲归还失败：该缓冲不属于本池或已被归还");
            }

            _idle.Add(buffer);
        }

        /// <summary>清空池中所有缓冲并释放引用（通常在停止取流或退出时调用）。</summary>
        public void Clear()
        {
            lock (_rentedLock)
            {
                _rented.Clear();
            }

            while (_idle.TryTake(out _))
            {
                // 仅丢弃引用，交由 GC 回收
            }
        }
    }
}
