namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    partial class DeviceInfoUC
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceInfoUC));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_DeviceName = new System.Windows.Forms.Label();
            this.lbl_DeviceType = new System.Windows.Forms.Label();
            this.lbl_DeviceLight = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btn_Connect = new System.Windows.Forms.ToolStripButton();
            this.btn_Disconnect = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_Interface = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbl_ip = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbl_HardwareModel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbl_Serial = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1143, 726);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.lbl_DeviceName, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbl_DeviceType, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbl_DeviceLight, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1137, 30);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // lbl_DeviceName
            // 
            this.lbl_DeviceName.AutoSize = true;
            this.lbl_DeviceName.Location = new System.Drawing.Point(570, 0);
            this.lbl_DeviceName.Name = "lbl_DeviceName";
            this.lbl_DeviceName.Size = new System.Drawing.Size(82, 24);
            this.lbl_DeviceName.TabIndex = 2;
            this.lbl_DeviceName.Text = "设备名";
            // 
            // lbl_DeviceType
            // 
            this.lbl_DeviceType.AutoSize = true;
            this.lbl_DeviceType.Location = new System.Drawing.Point(173, 0);
            this.lbl_DeviceType.Name = "lbl_DeviceType";
            this.lbl_DeviceType.Size = new System.Drawing.Size(106, 24);
            this.lbl_DeviceType.TabIndex = 1;
            this.lbl_DeviceType.Text = "设备类型";
            // 
            // lbl_DeviceLight
            // 
            this.lbl_DeviceLight.AutoSize = true;
            this.lbl_DeviceLight.ForeColor = System.Drawing.Color.Red;
            this.lbl_DeviceLight.Location = new System.Drawing.Point(3, 0);
            this.lbl_DeviceLight.Name = "lbl_DeviceLight";
            this.lbl_DeviceLight.Size = new System.Drawing.Size(26, 24);
            this.lbl_DeviceLight.TabIndex = 0;
            this.lbl_DeviceLight.Text = "⬤";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.toolStrip1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 681);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1137, 42);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanel1_Paint);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btn_Connect,
            this.btn_Disconnect});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(176, 42);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btn_Connect
            // 
            this.btn_Connect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btn_Connect.Image = ((System.Drawing.Image)(resources.GetObject("btn_Connect.Image")));
            this.btn_Connect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(46, 36);
            this.btn_Connect.Text = "连接相机";
            // 
            // btn_Disconnect
            // 
            this.btn_Disconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btn_Disconnect.Image = ((System.Drawing.Image)(resources.GetObject("btn_Disconnect.Image")));
            this.btn_Disconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_Disconnect.Name = "btn_Disconnect";
            this.btn_Disconnect.Size = new System.Drawing.Size(46, 36);
            this.btn_Disconnect.Text = "断开";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.lbl_Interface, 1, 3);
            this.tableLayoutPanel3.Controls.Add(this.label7, 0, 3);
            this.tableLayoutPanel3.Controls.Add(this.lbl_ip, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.lbl_HardwareModel, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.lbl_Serial, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 39);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1137, 636);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // lbl_Interface
            // 
            this.lbl_Interface.AutoSize = true;
            this.lbl_Interface.Location = new System.Drawing.Point(571, 477);
            this.lbl_Interface.Name = "lbl_Interface";
            this.lbl_Interface.Size = new System.Drawing.Size(22, 24);
            this.lbl_Interface.TabIndex = 8;
            this.lbl_Interface.Text = " ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 477);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(106, 24);
            this.label7.TabIndex = 7;
            this.label7.Text = "接口类型";
            // 
            // lbl_ip
            // 
            this.lbl_ip.AutoSize = true;
            this.lbl_ip.Location = new System.Drawing.Point(571, 318);
            this.lbl_ip.Name = "lbl_ip";
            this.lbl_ip.Size = new System.Drawing.Size(22, 24);
            this.lbl_ip.TabIndex = 6;
            this.lbl_ip.Text = " ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 318);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 24);
            this.label5.TabIndex = 5;
            this.label5.Text = "通讯地址";
            // 
            // lbl_HardwareModel
            // 
            this.lbl_HardwareModel.AutoSize = true;
            this.lbl_HardwareModel.Location = new System.Drawing.Point(571, 159);
            this.lbl_HardwareModel.Name = "lbl_HardwareModel";
            this.lbl_HardwareModel.Size = new System.Drawing.Size(22, 24);
            this.lbl_HardwareModel.TabIndex = 4;
            this.lbl_HardwareModel.Text = " ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 159);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 24);
            this.label3.TabIndex = 3;
            this.label3.Text = "硬件型号";
            // 
            // lbl_Serial
            // 
            this.lbl_Serial.AutoSize = true;
            this.lbl_Serial.Location = new System.Drawing.Point(571, 0);
            this.lbl_Serial.Name = "lbl_Serial";
            this.lbl_Serial.Size = new System.Drawing.Size(0, 24);
            this.lbl_Serial.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 24);
            this.label1.TabIndex = 1;
            this.label1.Text = "设备序列";
            // 
            // DeviceInfoUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DeviceInfoUC";
            this.Size = new System.Drawing.Size(1145, 728);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lbl_DeviceName;
        private System.Windows.Forms.Label lbl_DeviceType;
        private System.Windows.Forms.Label lbl_DeviceLight;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lbl_Interface;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbl_ip;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lbl_HardwareModel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl_Serial;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btn_Connect;
        private System.Windows.Forms.ToolStripButton btn_Disconnect;
    }
}
