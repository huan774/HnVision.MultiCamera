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
    public partial class UCVisionView : BaseViewUc
    {
        public event EventHandler ExposureValueChanged;
        public event EventHandler GainValueChanged;
        public event EventHandler TriggerModeChanged;
        public event EventHandler LightSliderChanged;

        public event EventHandler BtnSingleGrabClick;
        public event EventHandler BtnContinuousGrabClick;
        public event EventHandler BtnStopGrabClick;
        public event EventHandler BtnSaveImageClick;
        public event EventHandler BtnClearImgClick;
        public UCVisionView()
        {
            InitializeComponent();
           /* nudExposure.ValueChanged += (s, e) => ExposureValueChanged?.Invoke(this, EventArgs.Empty);
            nudGain.ValueChanged += (s, e) => GainValueChanged?.Invoke(this, EventArgs.Empty);
            cbxTriggerMode.SelectedIndexChanged += (s, e) => TriggerModeChanged?.Invoke(this, EventArgs.Empty);
            trackLight.ValueChanged += (s, e) => LightSliderChanged?.Invoke(this, EventArgs.Empty);

            btnSingleGrab.Click += (s, e) => BtnSingleGrabClick?.Invoke(this, EventArgs.Empty);
            btnContinuousGrab.Click += (s, e) => BtnContinuousGrabClick?.Invoke(this, EventArgs.Empty);
            btnStopGrab.Click += (s, e) => BtnStopGrabClick?.Invoke(this, EventArgs.Empty);
            btnSaveImage.Click += (s, e) => BtnSaveImageClick?.Invoke(this, EventArgs.Empty);
            btnClearImg.Click += (s, e) => BtnClearImgClick?.Invoke(this, EventArgs.Empty);*/
        }
        public override void OnViewShow()
        {
            base.OnViewShow();
           /* lblImgStatus.Text = "未连接相机";*/

        }
        public override void SetUIPlaceholder()
        {
           /* nudExposure.Value = 1000; ;
            nudExposure.Value = 1;
            trackLight.Value = 50;
            cbxTriggerMode.SelectedIndex = 0;
            picVisionImg.Image = null;
            lblImgStatus.Text = "无图像";*/

        }
        public Bitmap ShowImage
        {
            set
            {
               /* picVisionImg.Image?.Dispose();
                picVisionImg.Image = value;*/
            }
        }
/*        public string ImageStatusText
        {
            set => lblImgStatus.Text = value;
        }*/

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        /* public decimal Exposure
         {
             get => nudExposure.Value;
             set => nudExposure.Value = value;
         }

         public decimal Gain
         {
             get => nudGain.Value;
             set => nudGain.Value = value;
         }

         public int LightValue
         {
             get => trackLight.Value;
             set => trackLight.Value = value;
         }

         public string TriggerMode
         {
             get => cbxTriggerMode.Text;
             set => cbxTriggerMode.Text = value;
         }*/
    }
}
