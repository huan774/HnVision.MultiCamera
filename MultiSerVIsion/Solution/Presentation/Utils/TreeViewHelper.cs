using MultiSerVIsion.Solution.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Presentation.Utils
{
    /// <summary>
    /// 树视图帮助类，用于将模型树转换为 UI 树
    /// </summary>
    public class TreeViewHelper
    {
        /// <summary>
        /// 将模型树转换为 UI 树
        /// </summary>
        public static TreeNode BuildTreeNode(FlowNodeModel model)
        {
            var node = new TreeNode(model.DisplayName)
            {
                Tag = model.NodeId,
                Checked = model.Enabled
            };
            foreach (var child in model.Children)
            {
                node.Nodes.Add(BuildTreeNode(child));
            }
            return node;
        }
    }
}

