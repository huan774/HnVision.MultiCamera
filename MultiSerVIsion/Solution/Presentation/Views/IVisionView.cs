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
        /// <summary>请求开始采图</summary>
        event Action StartGrabRequested;
        /// <summary>请求停止采图</summary>
        event Action StopGrabRequested;

        /// <summary>更新显示的图像帧</summary>
        void UpdateFrame(Bitmap frame);
        /// <summary>更新运行信息文本（帧率、帧号、分辨率等）</summary>
        void UpdateRunInfo(string info);
        /// <summary>设置采图按钮可用状态</summary>
        void SetGrabButtonsEnabled(bool canStart, bool canStop);
        /// <summary>弹出提示消息</summary>
        void ShowMessage(string message, bool isError = false);
        /// <summary>清空显示内容</summary>
        void ClearDisplay();
    }
}
