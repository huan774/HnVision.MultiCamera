using System;

namespace MultiSerVIsion.Solution.Domain.Camera
{
    /// <summary>
    /// 帧缓冲池：为「每消费者深拷贝」提供可复用的像素缓冲，降低高帧率下的 GC 压力。
    /// 【所有权】<see cref="Rent"/> 返回的缓冲归调用方独占使用，用完必须 <see cref="Return"/>；
    /// 归还后不得再访问，实现可将其复用给其他消费者。
    /// 【线程安全】Rent / Return / Clear 需可并发调用。
    /// </summary>
    public interface IFrameBufferPool
    {
        /// <summary>当前池中可复用的缓冲数量。</summary>
        int PooledBufferCount { get; }

        /// <summary>累计租出次数（用于观测复用效率）。</summary>
        long TotalRentCount { get; }

        /// <summary>
        /// 租用一块至少指定长度的缓冲。
        /// </summary>
        /// <param name="minimumLength">所需最小字节数。</param>
        /// <returns>可写缓冲，长度不小于 minimumLength。</returns>
        /// <exception cref="ArgumentOutOfRangeException">minimumLength 小于等于 0。</exception>
        byte[] Rent(int minimumLength);

        /// <summary>
        /// 归还缓冲，使其可被复用。
        /// </summary>
        /// <param name="buffer">待归还缓冲；为 null 时忽略。</param>
        void Return(byte[] buffer);

        /// <summary>清空池中所有缓冲并释放引用（通常在停止取流或退出时调用）。</summary>
        void Clear();
    }
}
