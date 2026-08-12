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
        public void UpdateFrame(Bitmap frame)
        {
            if (PitCamera1.InvokeRequired)
            {
                PitCamera1.BeginInvoke(new Action(() => UpdateFrame(frame)));
                return;
            }

            // 释放旧图像，避免内存泄漏
            var oldImg = PitCamera1.Image;
            PitCamera1.Image = frame;
            oldImg?.Dispose();
        }
        public void UpdateRunInfo(string info)
        {
        
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
            if (PitCamera1.InvokeRequired)
            {
                PitCamera1.BeginInvoke(new Action(ClearDisplay));
                return;
            }
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
