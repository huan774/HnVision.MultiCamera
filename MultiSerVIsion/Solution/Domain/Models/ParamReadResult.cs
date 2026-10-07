using MultiSerVIsion.Solution.Domain.Entities.Configs;

namespace MultiSerVIsion.Solution.Domain.Models
{
    /// <summary>
    /// 相机参数读取结果：同时携带「完整规格」与「当前值」。
    /// 【职责】作为跨层传递的参数读取契约，使上层一次调用即可拿到限幅/校验所需的全部信息，
    /// 且不必依赖硬件层类型。
    /// </summary>
    public class ParamReadResult
    {
        /// <summary>参数规格（范围 / 步长 / 可选项 / 可写性）</summary>
        public ParamSpec Spec { get; set; }

        /// <summary>参数当前值（double / long / string / bool）</summary>
        public object Value { get; set; }
    }
}
