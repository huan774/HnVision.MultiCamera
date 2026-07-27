using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Presentation.Views;
using MvCameraControl;
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
    public partial class CameraDateilUC : BaseViewUc,IDeviceDatailView
    {
        public event Action<string, Dictionary<string, string>> OnDeviceConfigSave;
        private string _currentDeviceId;

        public event Action SaveConfigRequest;
        public event Action ClearRequest;
        public CameraDateilUC()
        {
            InitializeComponent();
            cbx_CamTrigger.Items.AddRange(new[] { "Soft", "hard" });
            cbx_CamTrigger.SelectedIndex = 0;
            cbx_CamType.Items.AddRange(new[] { "USB", "GigE", "Camera Link", "CoaXPress" });
            cbx_CamType.SelectedIndex = 1;
          /*  btn_SaveConfig.Click += Btn_SaveConfig_Click;*/
        }

        private void btn_SaveConfig_Click(object sender, EventArgs e)
        {
            SaveConfigRequest?.Invoke();
        }
        public void LoadDeviceData(DeviceEntity device)
        {
            this.SuspendLayout();
            try
            {
                lbl_deviceID.Text = device.DeviceId;
                lbl_DeviceName.Text = device.DeviceName;
                lbl_deviceType.Text = device.DeviceType;
                lbl_deviceStuate.Text = "未连接";

                
                var cfg = new CameraParamConfig();
              /*  num_CamExposureUs.Value = (int)cfg.ExposureTime;*/
                num_CamGiain.Value = (int)cfg.Gain;
                cbx_CamTrigger.Text = cfg.TriggerSource;
               
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }
        public void ClearPanel()
        {
            this.SuspendLayout();
            try
            {
                lbl_deviceID.Text = "未加载设备";
                lbl_DeviceName.Text = string.Empty;
                lbl_deviceType.Text = string.Empty;
                lbl_deviceStuate.Text = string.Empty;

                num_CamPort.Value = 3956;
                num_CamExposureUs.Value = 5000;
                num_CamGiain.Value = 8;
                cbx_CamTrigger.SelectedIndex = 0;
                cbx_CamType.SelectedIndex = 1;
                nud_CamChannel.Value = 1;
                chk_AutoExposureDefault.Checked = false;

            }
            finally
            {
                this.ResumeLayout(false);
            }
            SetEditDisable();
        }
        public void SetEditDisable()
        {
          
        }
        public void SetEditEnable()
        {
            
        }
        public DeviceEntity GetEditInput()
        {
            return new CameraEntity();
            
        }
        public void ShowMessage(string message) { MessageBox.Show(this,message); }
        public bool ShowConfirmDialog(string  message) { return MessageBox.Show(this,message,"提示",MessageBoxButtons.YesNo)==DialogResult.Yes; }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearRequest?.Invoke();
        }
       
    }
}
