using MultiSerVIsion.Solution.Domain.Models;
using System;
using System.Collections.Generic;

namespace MultiSerVIsion.Solution.Domain.Sources
{
    /// <summary>
    /// 统一图源契约：把「某一路数据（相机 / 文件 / 仿真）」抽象为可被多消费者订阅的帧输出。
    /// 算法配方页、视觉预览页等一律通过本接口取帧，不再直接依赖相机驱动，
    /// 从而使「取流来源」可替换、可并存多消费者。
    ///
    /// 【拷贝语义 · 强制约定】
    /// 1. 每个订阅者在回调中收到的 <see cref="CameraFrame"/> 必须是**该消费者独占的深拷贝**，
    ///    含 <see cref="CameraFrame.Data"/> 像素缓冲；
    /// 2. 严禁跨消费者共享同一像素缓冲，禁止把驱动采集回调内部缓冲的引用直接交出；
    /// 3. 消费者获得帧后拥有其所有权，修改像素内容不得影响其它消费者与图源实现；
    /// 4. 消费者不得缓存采集线程内部缓冲的引用，也无需自行释放帧缓冲（由缓冲池回收复用）。
    ///
    /// 【线程安全】
    /// 1. <see cref="Subscribe"/> / <see cref="Unsubscribe"/> 必须线程安全，可与帧回调并发调用；
    /// 2. 帧回调在采集线程触发，消费者若需更新 UI 必须自行投递到 UI 线程；
    /// 3. 回调内禁止执行长耗时操作（如算法推理），否则会阻塞采集线程并导致丢帧。
    ///
    /// 【生命周期】
    /// 消费者必须在销毁前取消订阅（Dispose 订阅句柄或调用 <see cref="Unsubscribe"/>）；
    /// 存在未取消订阅时，实现不得释放内部缓冲，以避免二次释放。
    /// </summary>
    public interface IFrameSource
    {
        /// <summary>关联的设备 ID（组态设备标识）。</summary>
        string DeviceId { get; }

        /// <summary>当前图源状态快照（只读，可安全跨线程读取）。</summary>
        FrameSourceState State { get; }

        /// <summary>当前已订阅的消费者标识集合（只读快照）。</summary>
        IReadOnlyCollection<string> ConsumerKeys { get; }

        /// <summary>
        /// 订阅帧输出。
        /// </summary>
        /// <param name="consumerKey">消费者唯一标识（如 "Preview" / "Algorithm.Recipe01"），用于隔离多消费者。</param>
        /// <param name="onFrame">帧回调；收到的帧为本消费者独占的深拷贝。</param>
        /// <returns>订阅句柄，Dispose 等价于 <see cref="Unsubscribe"/>，务必释放。</returns>
        /// <exception cref="ArgumentNullException">consumerKey 或 onFrame 为 null。</exception>
        /// <exception cref="InvalidOperationException">同一 consumerKey 重复订阅。</exception>
        IDisposable Subscribe(string consumerKey, Action<CameraFrame> onFrame);

        /// <summary>
        /// 取消订阅。
        /// </summary>
        /// <param name="consumerKey">订阅时使用的消费者唯一标识。</param>
        /// <returns>实际移除订阅返回 true；该消费者不存在返回 false。</returns>
        bool Unsubscribe(string consumerKey);
    }
}
