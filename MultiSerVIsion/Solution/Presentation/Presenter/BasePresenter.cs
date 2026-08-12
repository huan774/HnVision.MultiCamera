using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Presenter
{
    /// <summary>
    /// 所有Presenter的抽象基类，统一生命周期与公共行为
    /// </summary>
    public abstract class BasePresenter
    {
        /// <summary>
        /// 当前关联的设备ID（可选，全局切换设备时用）
        /// </summary>
        public string CurrentDeviceId { get; protected set; }

        /// <summary>
        /// 是否已释放资源
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 初始化Presenter（只执行一次，订阅视图事件）
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// 加载指定设备（切换设备时调用，可多次执行）
        /// </summary>
        /// <param name="deviceId">设备唯一标识</param>
        public virtual void LoadDevice(string deviceId)
        {
            CurrentDeviceId = deviceId;
        }

        /// <summary>
        /// 卸载当前设备（切换设备前、关闭页面前调用）
        /// </summary>
        public virtual void UnloadDevice()
        {
            CurrentDeviceId = string.Empty;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                // 子类重写这里：取消事件订阅、停止采集、释放托管资源
                UnloadDevice();
            }

            IsDisposed = true;
        }

        ~BasePresenter()
        {
            Dispose(false);
        }
    }
}
