using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Enums
{
    /// <summary>
    /// 节点相关枚举。
    /// 【说明】算法模块已剥离，NodeRunState / DropPosition 随之删除；
    /// ValidationLevel 必须保留——相机参数校验链路（ValidationResult）依赖它。
    /// </summary>
    public class NodeEnums
    {
        public enum ValidationLevel { None, Warning, Error }
    }
}
