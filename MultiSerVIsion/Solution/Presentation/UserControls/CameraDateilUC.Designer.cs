namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    partial class CameraDateilUC
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.chk_AutoExposureDefault = new System.Windows.Forms.CheckBox();
            this.num_CamGiain = new System.Windows.Forms.NumericUpDown();
            this.num_CamExposureUs = new System.Windows.Forms.NumericUpDown();
            this.lbl_CamGiain = new System.Windows.Forms.Label();
            this.lbl_CamExposureUs = new System.Windows.Forms.Label();
            this.cbx_CamTrigger = new System.Windows.Forms.ComboBox();
            this.cbx_CamType = new System.Windows.Forms.ComboBox();
            this.lbl_CamChannel = new System.Windows.Forms.Label();
            this.lbl_CamPort = new System.Windows.Forms.Label();
            this.lbl_CamTriggerMode = new System.Windows.Forms.Label();
            this.lbl_CamType = new System.Windows.Forms.Label();
            this.nud_CamChannel = new System.Windows.Forms.NumericUpDown();
            this.num_CamPort = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lbl_DeviceName = new System.Windows.Forms.Label();
            this.lbl_deviceType = new System.Windows.Forms.Label();
            this.lbl_deviceStuate = new System.Windows.Forms.Label();
            this.lbl_deviceID = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_CamGiain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_CamExposureUs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_CamChannel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_CamPort)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 73);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1137, 1035);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数配置";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoScroll = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.chk_AutoExposureDefault, 1, 6);
            this.tableLayoutPanel2.Controls.Add(this.num_CamGiain, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.num_CamExposureUs, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.lbl_CamGiain, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.lbl_CamExposureUs, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.cbx_CamTrigger, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.cbx_CamType, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbl_CamChannel, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.lbl_CamPort, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.lbl_CamTriggerMode, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lbl_CamType, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.nud_CamChannel, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.num_CamPort, 1, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 31);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 7;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1131, 1001);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // chk_AutoExposureDefault
            // 
            this.chk_AutoExposureDefault.AutoSize = true;
            this.chk_AutoExposureDefault.Location = new System.Drawing.Point(568, 603);
            this.chk_AutoExposureDefault.Name = "chk_AutoExposureDefault";
            this.chk_AutoExposureDefault.Size = new System.Drawing.Size(138, 28);
            this.chk_AutoExposureDefault.TabIndex = 23;
            this.chk_AutoExposureDefault.Text = "自动曝光";
            this.chk_AutoExposureDefault.UseVisualStyleBackColor = true;
            // 
            // num_CamGiain
            // 
            this.num_CamGiain.Location = new System.Drawing.Point(568, 503);
            this.num_CamGiain.Name = "num_CamGiain";
            this.num_CamGiain.Size = new System.Drawing.Size(120, 35);
            this.num_CamGiain.TabIndex = 22;
            // 
            // num_CamExposureUs
            // 
            this.num_CamExposureUs.Location = new System.Drawing.Point(568, 403);
            this.num_CamExposureUs.Maximum = new decimal(new int[] {
            6000,
            0,
            0,
            0});
            this.num_CamExposureUs.Minimum = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.num_CamExposureUs.Name = "num_CamExposureUs";
            this.num_CamExposureUs.Size = new System.Drawing.Size(120, 35);
            this.num_CamExposureUs.TabIndex = 21;
            this.num_CamExposureUs.Value = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            // 
            // lbl_CamGiain
            // 
            this.lbl_CamGiain.AutoSize = true;
            this.lbl_CamGiain.Location = new System.Drawing.Point(3, 500);
            this.lbl_CamGiain.Name = "lbl_CamGiain";
            this.lbl_CamGiain.Size = new System.Drawing.Size(106, 24);
            this.lbl_CamGiain.TabIndex = 20;
            this.lbl_CamGiain.Text = "默认增益";
            // 
            // lbl_CamExposureUs
            // 
            this.lbl_CamExposureUs.AutoSize = true;
            this.lbl_CamExposureUs.Location = new System.Drawing.Point(3, 400);
            this.lbl_CamExposureUs.Name = "lbl_CamExposureUs";
            this.lbl_CamExposureUs.Size = new System.Drawing.Size(106, 24);
            this.lbl_CamExposureUs.TabIndex = 18;
            this.lbl_CamExposureUs.Text = "默认曝光";
            // 
            // cbx_CamTrigger
            // 
            this.cbx_CamTrigger.FormattingEnabled = true;
            this.cbx_CamTrigger.Location = new System.Drawing.Point(568, 103);
            this.cbx_CamTrigger.Name = "cbx_CamTrigger";
            this.cbx_CamTrigger.Size = new System.Drawing.Size(150, 32);
            this.cbx_CamTrigger.TabIndex = 9;
            // 
            // cbx_CamType
            // 
            this.cbx_CamType.FormattingEnabled = true;
            this.cbx_CamType.Location = new System.Drawing.Point(568, 3);
            this.cbx_CamType.Name = "cbx_CamType";
            this.cbx_CamType.Size = new System.Drawing.Size(150, 32);
            this.cbx_CamType.TabIndex = 8;
            // 
            // lbl_CamChannel
            // 
            this.lbl_CamChannel.AutoSize = true;
            this.lbl_CamChannel.Location = new System.Drawing.Point(3, 300);
            this.lbl_CamChannel.Name = "lbl_CamChannel";
            this.lbl_CamChannel.Size = new System.Drawing.Size(106, 24);
            this.lbl_CamChannel.TabIndex = 7;
            this.lbl_CamChannel.Text = "触发通道";
            // 
            // lbl_CamPort
            // 
            this.lbl_CamPort.AutoSize = true;
            this.lbl_CamPort.Location = new System.Drawing.Point(3, 200);
            this.lbl_CamPort.Name = "lbl_CamPort";
            this.lbl_CamPort.Size = new System.Drawing.Size(106, 24);
            this.lbl_CamPort.TabIndex = 6;
            this.lbl_CamPort.Text = "通信端口";
            // 
            // lbl_CamTriggerMode
            // 
            this.lbl_CamTriggerMode.AutoSize = true;
            this.lbl_CamTriggerMode.Location = new System.Drawing.Point(3, 100);
            this.lbl_CamTriggerMode.Name = "lbl_CamTriggerMode";
            this.lbl_CamTriggerMode.Size = new System.Drawing.Size(106, 24);
            this.lbl_CamTriggerMode.TabIndex = 5;
            this.lbl_CamTriggerMode.Text = "触发模式";
            // 
            // lbl_CamType
            // 
            this.lbl_CamType.AutoSize = true;
            this.lbl_CamType.Location = new System.Drawing.Point(3, 0);
            this.lbl_CamType.Name = "lbl_CamType";
            this.lbl_CamType.Size = new System.Drawing.Size(106, 24);
            this.lbl_CamType.TabIndex = 4;
            this.lbl_CamType.Text = "接口类型";
            // 
            // nud_CamChannel
            // 
            this.nud_CamChannel.Location = new System.Drawing.Point(568, 303);
            this.nud_CamChannel.Name = "nud_CamChannel";
            this.nud_CamChannel.Size = new System.Drawing.Size(120, 35);
            this.nud_CamChannel.TabIndex = 10;
            // 
            // num_CamPort
            // 
            this.num_CamPort.Location = new System.Drawing.Point(568, 203);
            this.num_CamPort.Maximum = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.num_CamPort.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.num_CamPort.Name = "num_CamPort";
            this.num_CamPort.Size = new System.Drawing.Size(120, 35);
            this.num_CamPort.TabIndex = 13;
            this.num_CamPort.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AllowDrop = true;
            this.flowLayoutPanel1.Controls.Add(this.lbl_DeviceName);
            this.flowLayoutPanel1.Controls.Add(this.lbl_deviceType);
            this.flowLayoutPanel1.Controls.Add(this.lbl_deviceStuate);
            this.flowLayoutPanel1.Controls.Add(this.lbl_deviceID);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1137, 64);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // lbl_DeviceName
            // 
            this.lbl_DeviceName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbl_DeviceName.AutoSize = true;
            this.lbl_DeviceName.Location = new System.Drawing.Point(3, 0);
            this.lbl_DeviceName.Name = "lbl_DeviceName";
            this.lbl_DeviceName.Size = new System.Drawing.Size(82, 24);
            this.lbl_DeviceName.TabIndex = 0;
            this.lbl_DeviceName.Text = "设备名";
            // 
            // lbl_deviceType
            // 
            this.lbl_deviceType.AutoSize = true;
            this.lbl_deviceType.Location = new System.Drawing.Point(91, 0);
            this.lbl_deviceType.Name = "lbl_deviceType";
            this.lbl_deviceType.Size = new System.Drawing.Size(106, 24);
            this.lbl_deviceType.TabIndex = 2;
            this.lbl_deviceType.Text = "设备类型";
            // 
            // lbl_deviceStuate
            // 
            this.lbl_deviceStuate.AutoSize = true;
            this.lbl_deviceStuate.Location = new System.Drawing.Point(203, 0);
            this.lbl_deviceStuate.Name = "lbl_deviceStuate";
            this.lbl_deviceStuate.Size = new System.Drawing.Size(106, 24);
            this.lbl_deviceStuate.TabIndex = 3;
            this.lbl_deviceStuate.Text = "在线状态";
            // 
            // lbl_deviceID
            // 
            this.lbl_deviceID.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbl_deviceID.AutoSize = true;
            this.lbl_deviceID.Location = new System.Drawing.Point(315, 0);
            this.lbl_deviceID.Name = "lbl_deviceID";
            this.lbl_deviceID.Size = new System.Drawing.Size(82, 24);
            this.lbl_deviceID.TabIndex = 1;
            this.lbl_deviceID.Text = "设备ID";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AllowDrop = true;
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1143, 1181);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // CameraDateilUC
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CameraDateilUC";
            this.Size = new System.Drawing.Size(1145, 1240);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_CamGiain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_CamExposureUs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_CamChannel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.num_CamPort)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lbl_DeviceName;
        private System.Windows.Forms.Label lbl_deviceType;
        private System.Windows.Forms.Label lbl_deviceStuate;
        private System.Windows.Forms.Label lbl_deviceID;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lbl_CamChannel;
        private System.Windows.Forms.Label lbl_CamPort;
        private System.Windows.Forms.Label lbl_CamTriggerMode;
        private System.Windows.Forms.Label lbl_CamType;
        private System.Windows.Forms.ComboBox cbx_CamTrigger;
        private System.Windows.Forms.ComboBox cbx_CamType;
        private System.Windows.Forms.NumericUpDown nud_CamChannel;
        private System.Windows.Forms.NumericUpDown num_CamPort;
        private System.Windows.Forms.NumericUpDown num_CamGiain;
        private System.Windows.Forms.NumericUpDown num_CamExposureUs;
        private System.Windows.Forms.Label lbl_CamGiain;
        private System.Windows.Forms.Label lbl_CamExposureUs;
        private System.Windows.Forms.CheckBox chk_AutoExposureDefault;
    }
}
