using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Domain.Enums.NodeEnums;

namespace MultiSerVIsion.Solution.Presentation.Events
{
    /// <summary>
    /// 节点拖拽事件参数
    /// </summary>
    public class NodeDropEventArgs
    {
        public string OperatorType { get; set; }  // 拖拽的工具箱算子类型
        public string TargetNodeId { get; set; }  // 落点目标节点ID（null 表示落在空白处）
        public DropPosition Position { get; set; } // 落点位置
    }
}
