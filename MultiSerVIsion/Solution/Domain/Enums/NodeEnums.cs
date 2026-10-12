using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Enums
{
    /// <summary>
    /// 节点相关枚举
    /// </summary>
    public class NodeEnums
    {
        public enum NodeRunState { NotRun, Running, Success, Failed, Skipped }
        public enum ValidationLevel { None, Warning, Error }
        public enum DropPosition { Before, After, AsChild }
    }
}
