using MultiSerVIsion.Solution.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Domain.Enums.NodeEnums;

namespace MultiSerVIsion.Solution.Application.Interfaces
{
    /// <summary>
    /// 校验结果模型
    /// </summary>
    public class ValidationResult
    {
        public ValidationLevel Level { get; set; }
        public string Message { get; set; }
    }
    public interface IRecipeValidator
    {
        /// <summary>
        /// 全量校验整个流程树
        /// </summary>
        IEnumerable<ValidationResult> ValidateRecipe(FlowNodeModel root);

        /// <summary>
        /// 校验节点插入是否合法（用于拖拽红标提示）
        /// </summary>
        ValidationResult ValidateInsertion(FlowNodeModel root, string targetNodeId, DropPosition position, FlowNodeModel newNode);
    }
}
