using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    public partial class UCVisionView : BaseViewUc,IVisionView
    {
        public event Action StartGrabRequested;
        public event Action StopGrabRequested;

        public UCVisionView()
        {
            InitializeComponent();
            SetGrabButtonsEnabled(canStart: true, canStop: false);
        }
        
        public override void OnViewShow()
        {
            base.OnViewShow();
        }
        public override void SetUIPlaceholder()
        {
          

        }
        // 最新待渲染帧与排队标志：同一时刻只保留 1 帧排队，实现「丢帧保实时」
        private Bitmap _pendingFrame;
        private int _renderQueued;

        public void UpdateFrame(Bitmap frame)
        {
            if (frame == null) return;

            // 控件句柄未创建时直接丢弃帧，避免 Invoke/BeginInvoke 抛异常（控件尚未显示时会出现）
            if (!PitCamera1.IsHandleCreated)
            {
                frame.Dispose();
                return;
            }

            // 用最新帧替换尚未渲染的旧帧，避免 UI 线程排队堆积造成显示延迟与频率抖动
            var stale = System.Threading.Interlocked.Exchange(ref _pendingFrame, frame);
            stale?.Dispose();

            // 已有渲染任务排队时直接返回，UI 线程会取走最新帧
            if (System.Threading.Interlocked.CompareExchange(ref _renderQueued, 1, 0) != 0)
                return;

            PitCamera1.BeginInvoke(new Action(RenderPendingFrame));
        }

        /// <summary>在 UI 线程渲染最新待显示帧（由 UpdateFrame 排队触发）</summary>
        private void RenderPendingFrame()
        {
            // 先放开排队标志，保证后续帧能继续投递
            System.Threading.Interlocked.Exchange(ref _renderQueued, 0);

            var frame = System.Threading.Interlocked.Exchange(ref _pendingFrame, null);
            if (frame == null) return;

            var oldImg = PitCamera1.Image;
            PitCamera1.Image = frame;
            oldImg?.Dispose();
        }
        public void UpdateRunInfo(string info)
        {
            if (!IsHandleCreated) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateRunInfo(info)));
                return;
            }
            // 帧率/分辨率等信息显示在底部状态条首个标签
            toolStripLabel1.Text = info;
        }
        public void SetGrabButtonsEnabled(bool canStart, bool canStop)
        {
            btn_StartGrab.Enabled = canStart;
            btn_StopGrab.Enabled = canStop;
        }
        public void ShowMessage(string message, bool isError = false)
        {
            MessageBox.Show(message, isError ? "错误" : "提示",
                MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }
        public void ClearDisplay()
        {
            // 句柄未创建时无需清空，直接返回避免异常
            if (!PitCamera1.IsHandleCreated) return;

            if (PitCamera1.InvokeRequired)
            {
                PitCamera1.BeginInvoke(new Action(ClearDisplay));
                return;
            }

            // 丢弃尚未渲染的帧，避免清空后又被旧帧覆盖
            System.Threading.Interlocked.Exchange(ref _pendingFrame, null)?.Dispose();

            PitCamera1.Image?.Dispose();
            PitCamera1.Image = null;
           /* lbl_RunInfo.Text = "未采集";*/
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            StopGrabRequested?.Invoke();
            base.OnHandleDestroyed(e);
        }

        private void btn_StopGrab_Click(object sender, EventArgs e)
        {
            StopGrabRequested?.Invoke();
        }

        private void btn_StartGrab_Click(object sender, EventArgs e)
        {
            StartGrabRequested?.Invoke();
        }
    }
}
