using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    public partial class UCAlgorithmView
    {
        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.Font = new Font("微软雅黑", 9F);
            this.Size = new Size(1400, 860);
            this.DoubleBuffered = true; // 减少闪烁
        }

        private void BuildLayout()
        {
            BuildToolbar();
            BuildStatusBar();
            BuildSplitters();
            BuildToolbox();
            BuildFlowTree();
            BuildCenterTabs();
            BuildPropertyPanel();
            Compose();
        }

        // 核心组装区：注意 Add 的顺序，避免 Dock 覆盖
        private void Compose()
        {
            _splitRoot.Panel1.Controls.Add(_splitLeft);
            _splitRoot.Panel2.Controls.Add(_splitRight);

            _splitRight.Panel1.Controls.Add(_tabCenter);
            _splitRight.Panel2.Controls.Add(CreateHeaderPanel("参数", _propertyGrid));

            // 填充剩余空间 -> 底部状态栏 -> 顶部工具栏
            this.Controls.Add(_splitRoot);
            this.Controls.Add(_statusStrip);
            this.Controls.Add(_toolStrip);
        }

        private void BuildToolbar()
        {
            _toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _toolStrip.Items.Add(CreateButton("新建", () => MessageBox.Show("新建配方")));
            _toolStrip.Items.Add(CreateButton("保存", () => MessageBox.Show("保存配方")));
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(CreateButton("运行", () => MessageBox.Show("运行")));
            _toolStrip.Items.Add(CreateButton("停止", () => MessageBox.Show("停止")));
            _toolStrip.Items.Add(CreateButton("单步", () => MessageBox.Show("单步")));
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(CreateButton("执行到选中", () => MessageBox.Show("执行到选中")));
            _toolStrip.Items.Add(CreateButton("从选中开始", () => MessageBox.Show("从选中开始")));
        }

        private void BuildStatusBar()
        {
            _statusStrip = new StatusStrip();
            _lblStatus = new ToolStripStatusLabel("就绪") { Width = 120 };
            _lblCurrentNode = new ToolStripStatusLabel("当前节点：-") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _lblElapsed = new ToolStripStatusLabel("耗时：0 ms") { Width = 120 };
            _statusStrip.Items.AddRange(new ToolStripItem[] { _lblStatus, _lblCurrentNode, _lblElapsed });
        }

        private void BuildSplitters()
        {
            _splitRoot = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 4 };
            _splitLeft = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterWidth = 4 };
            _splitRight = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 4 };
        }

        private void BuildToolbox()
        {
            _treeToolbox = new TreeView { Dock = DockStyle.Fill, ItemHeight = 22, HideSelection = false };
            // 填充测试数据
            _treeToolbox.Nodes.Add("预处理");
            _treeToolbox.Nodes[0].Nodes.Add("高斯滤波");
            _treeToolbox.Nodes[0].Nodes.Add("中值滤波");
            _treeToolbox.Nodes.Add("OCR");
            _treeToolbox.ExpandAll();

            _splitLeft.Panel1.Controls.Add(CreateHeaderPanel("算法工具箱", _treeToolbox));
        }

        private void BuildFlowTree()
        {
            _treeFlow = new TreeView
            {
                Dock = DockStyle.Fill,
                ItemHeight = 22,
                AllowDrop = true, // 允许拖拽
                CheckBoxes = true,
                DrawMode = TreeViewDrawMode.OwnerDrawText // 自绘，为了着色
            };
            _splitLeft.Panel2.Controls.Add(CreateHeaderPanel("流程树", _treeFlow));
        }

        private void BuildCenterTabs()
        {
            _tabCenter = new TabControl { Dock = DockStyle.Fill };
            _tabImage = new TabPage("图像");
            _tabVariables = new TabPage("变量");
            _tabLog = new TabPage("日志");

            // 图像页：外部将 HWindowControl 注入到这里
            _imageHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 48) };
            _tabImage.Controls.Add(_imageHost);

            // 变量页
            _gridVariables = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _gridVariables.Columns.Add("name", "变量名");
            _gridVariables.Columns.Add("type", "类型");
            _gridVariables.Columns.Add("value", "当前值");
            _tabVariables.Controls.Add(_gridVariables);

            // 日志页
            _listLog = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F) };
            _tabLog.Controls.Add(_listLog);

            _tabCenter.TabPages.AddRange(new[] { _tabImage, _tabVariables, _tabLog });
        }

        private void BuildPropertyPanel()
        {
            _propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                PropertySort = PropertySort.Categorized,
                HelpVisible = true
            };
        }

        // ================= 辅助工厂方法 =================
        private ToolStripButton CreateButton(string text, System.Action onClick)
        {
            var btn = new ToolStripButton(text);
            btn.Click += (s, e) => onClick?.Invoke();
            return btn;
        }

        private Control CreateHeaderPanel(string title, Control content)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var lbl = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = SystemColors.ControlLight,
                Padding = new Padding(6, 0, 0, 0)
            };
            panel.Controls.Add(content);
            panel.Controls.Add(lbl);
            return panel;
        }
    }
}

