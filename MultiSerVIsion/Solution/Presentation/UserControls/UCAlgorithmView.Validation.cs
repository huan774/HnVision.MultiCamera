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
        // 缓存状态，由 Presenter 更新
        private readonly System.Collections.Generic.Dictionary<string, NodeRunState> _runStateCache = new System.Collections.Generic.Dictionary<string, NodeRunState>();
        private readonly System.Collections.Generic.Dictionary<string, ValidationLevel> _validationCache = new System.Collections.Generic.Dictionary<string, ValidationLevel>();

        private void WireValidationEvents()
        {
            _treeFlow.DrawNode += TreeFlow_DrawNode;
        }

        private void TreeFlow_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) return;

            var nodeId = e.Node.Tag as string;

            // 获取状态
            var state = NodeRunState.NotRun;
            if (nodeId != null && _runStateCache.ContainsKey(nodeId)) state = _runStateCache[nodeId];

            var level = ValidationLevel.None;
            if (nodeId != null && _validationCache.ContainsKey(nodeId)) level = _validationCache[nodeId];

            // 根据状态确定文字颜色
            Color foreColor = Color.Black;
            switch (state)
            {
                case NodeRunState.Success: foreColor = Color.Green; break;
                case NodeRunState.Failed: foreColor = Color.Red; break;
                case NodeRunState.Running: foreColor = Color.DodgerBlue; break;
            }
            if (level == ValidationLevel.Error) foreColor = Color.Red;

            bool isSelected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;

            // 绘制背景和文字
            if (isSelected)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, _treeFlow.Font, e.Bounds, SystemColors.HighlightText, TextFormatFlags.VerticalCenter);
            }
            else
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, _treeFlow.Font, e.Bounds, foreColor, TextFormatFlags.VerticalCenter);
            }

            // 画警告感叹号
            if (level == ValidationLevel.Error)
            {
                using (var font = new Font("Arial", 10, FontStyle.Bold))
                {
                    e.Graphics.DrawString("!", font, Brushes.Red, e.Bounds.Left - 14, e.Bounds.Top + 3);
                }
            }

            // 画拖拽预览红框
            if (nodeId != null && nodeId == _dragPreviewNodeId && _dragHasError)
            {
                using (var pen = new Pen(Color.Red, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    e.Graphics.DrawRectangle(pen, e.Bounds);
                }
            }
        }

        // ================= 供 Presenter 调用的更新方法 =================
        public void SetNodeState(string nodeId, NodeRunState state)
        {
            _runStateCache[nodeId] = state;
            _treeFlow.Invalidate();
        }

        public void SetNodeValidation(string nodeId, ValidationLevel level)
        {
            _validationCache[nodeId] = level;
            _treeFlow.Invalidate();
        }
    }

    // 状态枚举
    public enum NodeRunState { NotRun, Running, Success, Failed, Skipped }
    public enum ValidationLevel { None, Warning, Error }
}

