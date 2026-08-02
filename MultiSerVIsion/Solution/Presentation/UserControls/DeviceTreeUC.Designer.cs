namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    partial class DeviceTreeUC
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceTreeUC));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btn_AddDevice = new System.Windows.Forms.ToolStripButton();
            this.btn_DelDevice = new System.Windows.Forms.ToolStripButton();
            this.btn_RefreshTress = new System.Windows.Forms.ToolStripButton();
            this.treeView_Device = new System.Windows.Forms.TreeView();
            this.contextMenuStrip_Device = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmi_AddChild = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_EnableDisable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.contextMenuStrip_Device.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btn_AddDevice,
            this.btn_DelDevice,
            this.btn_RefreshTress});
            this.toolStrip1.Location = new System.Drawing.Point(1, 669);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1127, 42);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btn_AddDevice
            // 
            this.btn_AddDevice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btn_AddDevice.Image = ((System.Drawing.Image)(resources.GetObject("btn_AddDevice.Image")));
            this.btn_AddDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_AddDevice.Name = "btn_AddDevice";
            this.btn_AddDevice.Size = new System.Drawing.Size(46, 36);
            this.btn_AddDevice.Text = "添加设备";
            // 
            // btn_DelDevice
            // 
            this.btn_DelDevice.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btn_DelDevice.Image = ((System.Drawing.Image)(resources.GetObject("btn_DelDevice.Image")));
            this.btn_DelDevice.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_DelDevice.Name = "btn_DelDevice";
            this.btn_DelDevice.Size = new System.Drawing.Size(46, 36);
            this.btn_DelDevice.Text = "移除设备";
            // 
            // btn_RefreshTress
            // 
            this.btn_RefreshTress.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btn_RefreshTress.Image = ((System.Drawing.Image)(resources.GetObject("btn_RefreshTress.Image")));
            this.btn_RefreshTress.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_RefreshTress.Name = "btn_RefreshTress";
            this.btn_RefreshTress.Size = new System.Drawing.Size(46, 36);
            this.btn_RefreshTress.Text = "刷新设备";
            this.btn_RefreshTress.Click += new System.EventHandler(this.btn_RefreshTress_Click);
            // 
            // treeView_Device
            // 
            this.treeView_Device.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_Device.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.treeView_Device.Location = new System.Drawing.Point(1, 1);
            this.treeView_Device.Name = "treeView_Device";
            this.treeView_Device.Size = new System.Drawing.Size(1127, 668);
            this.treeView_Device.TabIndex = 1;
            this.treeView_Device.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_Device_AfterSelect);
            // 
            // contextMenuStrip_Device
            // 
            this.contextMenuStrip_Device.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip_Device.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_AddChild,
            this.tsmi_Delete,
            this.tsmi_Copy,
            this.tsmi_EnableDisable});
            this.contextMenuStrip_Device.Name = "menueTreeRight";
            this.contextMenuStrip_Device.Size = new System.Drawing.Size(185, 156);
            // 
            // tsmi_AddChild
            // 
            this.tsmi_AddChild.Name = "tsmi_AddChild";
            this.tsmi_AddChild.Size = new System.Drawing.Size(184, 38);
            this.tsmi_AddChild.Text = "添加设备";
            // 
            // tsmi_Delete
            // 
            this.tsmi_Delete.Name = "tsmi_Delete";
            this.tsmi_Delete.Size = new System.Drawing.Size(184, 38);
            this.tsmi_Delete.Text = "移除设备";
            // 
            // tsmi_Copy
            // 
            this.tsmi_Copy.Name = "tsmi_Copy";
            this.tsmi_Copy.Size = new System.Drawing.Size(184, 38);
            this.tsmi_Copy.Text = "复制设备";
            // 
            // tsmi_EnableDisable
            // 
            this.tsmi_EnableDisable.Name = "tsmi_EnableDisable";
            this.tsmi_EnableDisable.Size = new System.Drawing.Size(184, 38);
            this.tsmi_EnableDisable.Text = "禁用设备";
            // 
            // DeviceTreeUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeView_Device);
            this.Controls.Add(this.toolStrip1);
            this.Name = "DeviceTreeUC";
            this.Size = new System.Drawing.Size(1129, 712);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.contextMenuStrip_Device.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TreeView treeView_Device;
        private System.Windows.Forms.ToolStripButton btn_AddDevice;
        private System.Windows.Forms.ToolStripButton btn_DelDevice;
        private System.Windows.Forms.ToolStripButton btn_RefreshTress;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_Device;
        private System.Windows.Forms.ToolStripMenuItem tsmi_AddChild;
        private System.Windows.Forms.ToolStripMenuItem tsmi_Delete;
        private System.Windows.Forms.ToolStripMenuItem tsmi_Copy;
        private System.Windows.Forms.ToolStripMenuItem tsmi_EnableDisable;
    }
}
