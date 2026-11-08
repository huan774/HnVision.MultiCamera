using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Views
{
    public interface IVisionView
    {
        /// <summary>请求开始采图（兼容旧调用；新流程请用 OnStartStreamClicked）</summary>
        event Action StartGrabRequested;
        /// <summary>请求停止采图（兼容旧调用；新流程请用 OnStopStreamClicked）</summary>
        event Action StopGrabRequested;

        /// <summary>取流按钮点击 → 请求连续采集。</summary>
        event Action OnStartStreamClicked;
        /// <summary>停止按钮点击 → 请求停止取流。</summary>
        event Action OnStopStreamClicked;

        /// <summary>更新显示的图像帧</summary>
        void UpdateFrame(Bitmap frame);
        /// <summary>更新运行信息文本（帧率、帧号、分辨率等）</summary>
        void UpdateRunInfo(string info);
        /// <summary>设置采图按钮可用状态</summary>
        void SetGrabButtonsEnabled(bool canStart, bool canStop);
        /// <summary>按参数规格刷新参数配置区（含 Min/Max/Step 限幅提示）。</summary>
        /// <param name="specs">参数规格集合。</param>
        void ShowParameterSpecs(IReadOnlyList<CameraParameterSpec> specs);
        /// <summary>弹出提示消息</summary>
        void ShowMessage(string message, bool isError = false);
        /// <summary>清空显示内容</summary>
        void ClearDisplay();
    }
}
