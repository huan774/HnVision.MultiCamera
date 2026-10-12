using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    [DesignerCategory("Code")]
    public partial class UCAlgorithmView : BaseViewUc/*,IAlgorithmRecipeView*/
    {
        // ================= 控件字段声明（全手写） =================
        private ToolStrip _toolStrip;
        private SplitContainer _splitRoot;
        private SplitContainer _splitLeft;
        private SplitContainer _splitRight;
        private TreeView _treeToolbox;
        private TreeView _treeFlow;
        private TabControl _tabCenter;
        private TabPage _tabImage;
        private TabPage _tabVariables;
        private TabPage _tabLog;
        private Panel _imageHost; // 留给 HWindowControl 注入的容器
        private DataGridView _gridVariables;
        private ListBox _listLog;
        private PropertyGrid _propertyGrid;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _lblStatus;
        private ToolStripStatusLabel _lblCurrentNode;
        private ToolStripStatusLabel _lblElapsed;

        // ================= 对外暴露的事件（供 Presenter 订阅） =================
        public event EventHandler<string> NodeSelected;
        public event EventHandler<string> RunRequested;
        public event EventHandler<string> StopRequested;
        public event EventHandler<string> StepRequested;
        // 这里留空，根据之前的设计后续补充...

        // ================= 对外暴露的状态 =================
        public string SelectedNodeId => _treeFlow?.SelectedNode?.Tag as string;


        // ================= 构造函数 =================
        public UCAlgorithmView()
        {
            InitializeComponents(); // 初始化基础属性
            BuildLayout();          // 构建布局 (在 Layout.cs 中)
            WireEvents();           // 挂载事件 (在 Event.cs 中)
        }

        // ================= 暴露给外部注入 HWindowControl =================
        public void AttachImageViewer(Control viewer)
        {
            viewer.Dock = DockStyle.Fill;
            _imageHost.Controls.Clear();
            _imageHost.Controls.Add(viewer);
        }

        // 提供给 Presenter 更新界面的方法（为了先跑通视图，暂写在此）
        public void AppendLog(string message)
        {
            _listLog?.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            if (_listLog != null && _listLog.Items.Count > 0)
                _listLog.TopIndex = _listLog.Items.Count - 1;
        }
    }
}

