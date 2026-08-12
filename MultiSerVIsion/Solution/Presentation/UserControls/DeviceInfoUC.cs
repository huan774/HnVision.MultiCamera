using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
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
    public partial class DeviceInfoUC : BaseViewUc,IDeviceInfoParamView
    {
        public event Action OnConnectClicked;
        public event Action OnDisconnectClicked;
        public DeviceInfoUC()
        {
            InitializeComponent();
            this.Dock= DockStyle.Fill;

            btn_Connect.Click += (s, e) => OnConnectClicked?.Invoke();
            btn_Disconnect.Click += (s, e) => OnDisconnectClicked?.Invoke();
                
        }

        public void ShowOnlineCameraInfo(CameraDeviceDto dto)
        {
            lbl_DeviceType.Text = "相机（在线未添加）";
           /* lbl_DeviceName.Text = dto.;*/
            lbl_Serial.Text = dto.SerialNumber;
            lbl_Interface.Text = dto.InterfaceType.ToString();
            lbl_ip.Text = dto.IpAddress;
            lbl_HardwareModel.Text = dto.Model;
        }

        public void ShowConfigCameraInfo(CameraEntity entity)
        {
            lbl_DeviceType.Text = entity.DeviceType;
            lbl_DeviceName.Text = entity.DeviceName;
            lbl_HardwareModel.Text = entity.CameraAllConfig.ConnectConfig.Model;
            lbl_Serial.Text = entity.CameraAllConfig.ConnectConfig.SerialNumber;
        /*    lbl_ip.Text = entity.CameraAllConfig.ConnectConfig.IpAddress.ToString();*/
            lbl_HardwareModel.Text = entity.CameraAllConfig.ConnectConfig.InterfaceType;
        }

        public void ClearDeviceInfo()
        {
            lbl_DeviceType.Text = string.Empty;
            lbl_DeviceName.Text = string.Empty;

            lbl_Serial.Text = string.Empty;
            lbl_ip.Text = string.Empty;
            lbl_Interface.Text = string.Empty;
            lbl_HardwareModel.Text = string.Empty;
        }
        public void UpdateConnectStatus(CameraStatus status)
        {
            switch (status)
            {
                case CameraStatus.Disconnected:
                   /* lbl_DeviceLight.BackColor = Color.Red;*/
                    btn_Connect.Visible = true;
                    btn_Disconnect.Visible = false;
                    break;
                case CameraStatus.Connected:
                case CameraStatus.Streaming:
               /*     lbl_DeviceLight.BackColor = Color.Green;*/
                    btn_Connect.Visible = false;
                    btn_Disconnect.Visible = true;
                    break;
                default:
                   /* lbl_DeviceLight.BackColor = Color.Yellow;*/
                    break; 
            }
        }
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {

        }
    }
}
