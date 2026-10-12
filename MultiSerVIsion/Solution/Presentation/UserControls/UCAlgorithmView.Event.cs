using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    public partial class UCAlgorithmView
    {
        private void WireEvents()
        {
            // 流程树选中节点
            _treeFlow.AfterSelect += (s, e) =>
            {
                if (e.Node?.Tag is string nodeId)
                {
                    NodeSelected?.Invoke(this, nodeId);
                }
                else
                {
                    // 临时演示：如果 Tag 为空，直接弹窗
                    MessageBox.Show($"选中节点: {e.Node?.Text}");
                }
            };

            // 临时绑定工具栏事件（后续由 Presenter 接管）
            // 这里直接连到 MessageBox 演示交互
            foreach (ToolStripItem item in _toolStrip.Items)
            {
                if (item is ToolStripButton btn)
                {
                    var originalText = btn.Text;
                    btn.Click += (s, e) =>
                    {
                        if (originalText == "运行") RunRequested?.Invoke(this, SelectedNodeId);
                        else if (originalText == "停止") StopRequested?.Invoke(this, null);
                        else if (originalText == "单步") StepRequested?.Invoke(this, SelectedNodeId);
                        else MessageBox.Show($"触发了 [{originalText}] 事件");
                    };
                }
            }
        }
    }
}

