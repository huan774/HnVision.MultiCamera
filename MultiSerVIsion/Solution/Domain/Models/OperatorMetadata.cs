using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Models
{
    /// <summary>
    /// 算子元数据模型
    /// </summary>
    public class OperatorMetadata
    {
        public string Type { get; set; }          // 唯一标识，如 "GaussianBlur"
        public string DisplayName { get; set; }   // 显示名，如 "高斯滤波"
        public string Category { get; set; }      // 分类，如 "预处理"
        public string Description { get; set; }   // 算子说明
        public List<PortDefinition> Inputs { get; set; } = new List<PortDefinition>();
        public List<PortDefinition> Outputs { get; set; } = new List<PortDefinition>();
        public List<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();
    }
}

