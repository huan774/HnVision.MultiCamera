using System;
using System.Collections.Generic;

namespace MultiSerVIsion.Solution.Domain.Sources
{
    /// <summary>
    /// 图源提供者：按设备 ID 管理图源实例的创建、复用与销毁。
    /// 【职责】集中管理图源生命周期，使消费者只依赖 <see cref="IFrameSource"/>，
    /// 不感知底层是相机、文件还是仿真数据。
    /// 【线程安全】所有成员需可并发调用。
    /// </summary>
    public interface IFrameSourceProvider
    {
        /// <summary>已注册的图源集合（只读快照）。</summary>
        IReadOnlyCollection<IFrameSource> Sources { get; }

        /// <summary>
        /// 获取指定设备的图源；不存在时返回 null，不创建。
        /// </summary>
        /// <param name="deviceId">组态设备 ID。</param>
        /// <returns>已存在的图源；不存在返回 null。</returns>
        IFrameSource GetSource(string deviceId);

        /// <summary>
        /// 获取指定设备的图源；不存在时创建。
        /// </summary>
        /// <param name="deviceId">组态设备 ID。</param>
        /// <returns>该设备对应的图源（新建或已存在）。</returns>
        /// <exception cref="ArgumentException">deviceId 为空。</exception>
        IFrameSource GetOrCreateSource(string deviceId);

        /// <summary>
        /// 移除并释放指定设备的图源（存在未取消订阅时内部应先完成清理）。
        /// </summary>
        /// <param name="deviceId">组态设备 ID。</param>
        /// <returns>实际移除返回 true；不存在返回 false。</returns>
        bool RemoveSource(string deviceId);
    }
}
