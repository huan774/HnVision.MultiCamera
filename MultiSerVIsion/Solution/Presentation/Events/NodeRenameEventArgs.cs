using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Events
{
    /// <summary>
    /// 节点重命名事件参数
    /// </summary>
    public class NodeRenameEventArgs
    {
        public string NodeId { get; set; }
        public string NewName { get; set; }
    }
}
