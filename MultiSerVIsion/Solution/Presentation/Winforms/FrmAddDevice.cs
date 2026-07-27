using MultiSerVIsion.Solution.Application.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Presentation.Winforms
{
 
    public partial class FrmAddDevice : Form
    {
        public string TargetGroupId {  get; set; }
        public FrmAddDevice()
        {
            InitializeComponent();
            cbxDevType.Items.AddRange(new object[] { "相机设备",/* "PLC设备",*/ "运动控制" });
            cbxDevType.SelectedIndex = 0;
        }

        private void btnDevOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDevNama.Text))
            {
                MessageBox.Show("请输入设备名称");
                txtDevNama.Focus();
                return;
            }if (string.IsNullOrWhiteSpace(txtDevIp.Text))
            {
                MessageBox.Show("请填写IP地址");
                txtDevIp.Focus(); return;
            }
            DialogResult= DialogResult.OK;
            Close();
        }

        private void btnDevCancel_Click(object sender, EventArgs e)
        {
            DialogResult= DialogResult.Cancel;
            Close();
        }
        public DeviceCreateInput GetInput()
        {
            string typeKey = cbxDevType.Text;
            switch (typeKey)
            {
                case "相机设备":
                    typeKey = "Camera";
                    break;
               /* case "PLC设备":
                    typeKey = "Plc";
                    break;*/
                case "运动控制":
                    typeKey = "Motion";
                    break;
            }
            return new DeviceCreateInput
            {
                DeviceId = txtDevIp.ToString(),
                GroupTag = TargetGroupId,
                DeviceName = txtDevNama.Text.Trim(),
                IpAddress = txtDevIp.Text.Trim(),
                DeviceType = typeKey,
            };
               
        }
    }
}
