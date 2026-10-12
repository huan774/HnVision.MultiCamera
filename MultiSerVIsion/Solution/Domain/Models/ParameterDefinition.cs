using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Models
{
    /// <summary>
    /// 参数定义模型
    /// </summary>
    public class ParameterDefinition
    {
        public string Name { get; set; }         // 参数名，如 "KernelSize"
        public string DisplayName { get; set; }  // 显示名
        public string DataType { get; set; }     // 类型，如 "int" / "double"
        public object DefaultValue { get; set; } // 默认值
        public double? Min { get; set; }         // 最小值
        public double? Max { get; set; }         // 最大值
    }
}
