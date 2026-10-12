using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Models
{
    /// <summary>
    /// 端口定义模型
    /// </summary>
    public class PortDefinition
    {
        public string Name { get; set; }         // 端口名，如 "InputImage"
        public string DataType { get; set; }     // 数据类型，如 "HObject" / "double"
        public bool IsRequired { get; set; }     // 是否必填
        public string DefaultVariable { get; set; } // 默认绑定的变量名
    }
}
