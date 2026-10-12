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
        // 记录当前拖拽预览的节点Id，用于画红框
        private string _dragPreviewNodeId;
        private bool _dragHasError;

        private void WireDragDropEvents()
        {
            _treeToolbox.ItemDrag += (s, e) =>
            {
                var node = e.Item as TreeNode;
                if (node?.Tag is string opType)
                {
                    DoDragDrop(opType, DragDropEffects.Copy);
                }
            };

            _treeFlow.DragEnter += (s, e) =>
            {
                e.Effect = e.Data.GetDataPresent(DataFormats.Text) ? DragDropEffects.Copy : DragDropEffects.None;
            };

            _treeFlow.DragOver += (s, e) =>
            {
                if (!e.Data.GetDataPresent(DataFormats.Text)) return;
                e.Effect = DragDropEffects.Copy;

                var pt = _treeFlow.PointToClient(new Point(e.X, e.Y));
                var target = _treeFlow.GetNodeAt(pt);
                _dragPreviewNodeId = target?.Tag as string;

                // 校验交由 Presenter 处理，这里默认无错
                _dragHasError = false;
                _treeFlow.Invalidate(); // 触发重绘
            };

            _treeFlow.DragLeave += (s, e) =>
            {
                _dragPreviewNodeId = null;
                _treeFlow.Invalidate();
            };

            _treeFlow.DragDrop += (s, e) =>
            {
                if (!e.Data.GetDataPresent(DataFormats.Text)) return;
                var opType = (string)e.Data.GetData(DataFormats.Text);
                var pt = _treeFlow.PointToClient(new Point(e.X, e.Y));
                var target = _treeFlow.GetNodeAt(pt);

                // 临时演示：直接添加节点
                var newNode = new TreeNode(opType) { Tag = Guid.NewGuid().ToString("N") };
                if (target != null)
                {
                    target.Nodes.Add(newNode);
                    target.Expand();
                }
                else
                {
                    _treeFlow.Nodes.Add(newNode);
                }

                _dragPreviewNodeId = null;
                _treeFlow.Invalidate();
                AppendLog($"拖入算子：{opType}");
            };
        }
    }
}

