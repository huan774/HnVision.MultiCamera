using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MvCamCtrl.NET;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    /// <summary>
    /// 单台相机的会话状态：句柄、采集模式、取流状态、图源、同步锁与采集缓冲。
    /// 【并发约定】每台相机一个实例，锁粒度到会话级，避免多相机之间相互阻塞；
    /// 所有对 SDK 句柄与启停流状态的操作都应在 <see cref="SyncRoot"/> 内进行。
    /// 【线程约定】<see cref="IsGrabbing"/> 供取流线程无锁轮询，故为 volatile。
    /// </summary>
    internal sealed class HikCameraSession : IDisposable
    {
        /// <summary>相机句柄（-1 表示未连接；实际 SDK 托管对象见 <see cref="CameraObj"/>）。</summary>
        public int Handle { get; set; } = -1;

        /// <summary>相机序列号（本会话的唯一标识）。</summary>
        public string SerialNumber { get; }

        /// <summary>当前采集模式。</summary>
        public AcquisitionMode Mode { get; set; } = AcquisitionMode.Continuous;

        /// <summary>是否正在取流（对外语义状态，与 SDK 侧 StartGrabbing 对应）。</summary>
        public bool IsStreaming { get; set; }

        /// <summary>本相机的图源（多消费者订阅入口）。</summary>
        public HikCameraFrameSource Source { get; }

        /// <summary>会话级同步锁（保护 Handle 与取停流状态迁移）。</summary>
        public readonly object SyncRoot = new object();

        /// <summary>SDK 相机对象（对应 <see cref="Handle"/>）。</summary>
        public MyCamera CameraObj;

        /// <summary>帧回调（应用服务订阅；图源发布时同步回调）。</summary>
        public Action<CameraFrame> FrameCallback;

        /// <summary>取流线程。</summary>
        public Thread GrabThread;

        /// <summary>取流线程运行标志（无锁轮询）。</summary>
        public volatile bool IsGrabbing;

        /// <summary>像素格式转换输出缓冲（非托管）。</summary>
        public IntPtr ConvertBuffer = IntPtr.Zero;

        /// <summary>像素格式转换输出缓冲容量（字节）。</summary>
        public uint ConvertBufferSize;

        /// <summary>
        /// 构造会话
        /// </summary>
        /// <param name="serialNumber">相机序列号。</param>
        public HikCameraSession(string serialNumber)
        {
            SerialNumber = serialNumber ?? string.Empty;
            Source = new HikCameraFrameSource(SerialNumber);
        }

        /// <summary>释放会话持有的非托管缓冲（幂等，可重复调用）。</summary>
        public void Dispose()
        {
            if (ConvertBuffer == IntPtr.Zero) return;

            Marshal.FreeHGlobal(ConvertBuffer);
            ConvertBuffer = IntPtr.Zero;
            ConvertBufferSize = 0;
        }
    }
}
