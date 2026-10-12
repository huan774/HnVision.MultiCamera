using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Models
{
    /// <summary>
    /// 流程节点模型
    /// </summary>
    public class FlowNodeModel
    {
        public string NodeId { get; set; } = System.Guid.NewGuid().ToString("N");
        public string DisplayName { get; set; }
        public string OperatorType { get; set; } // 对应 OperatorMetadata.Type
        public bool Enabled { get; set; } = true;
        public bool IsBreakpoint { get; set; } = false;
        public Dictionary<string, string> InputBindings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> OutputBindings { get; set; } = new Dictionary<string, string>();

        // 树形结构
        public List<FlowNodeModel> Children { get; set; } = new List<FlowNodeModel>();
    }
}
