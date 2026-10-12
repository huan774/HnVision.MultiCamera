using MultiSerVIsion.Solution.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Application.Interfaces
{
    /// <summary>
    /// 算子注册表接口，用于获取算子元数据
    /// </summary>
    public interface IOperatorRegistry
    {
        /// <summary>
        /// 获取所有内置算子元数据（用于生成左侧工具箱）
        /// </summary>
        IEnumerable<OperatorMetadata> GetAllOperators();

        /// <summary>
        /// 根据算子类型获取特定元数据（用于创建节点时初始化参数）
        /// </summary>
        OperatorMetadata GetOperator(string type);
    }
}
