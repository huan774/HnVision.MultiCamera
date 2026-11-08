using System;

namespace MultiSerVIsion.Solution.Domain.Entities.Configs
{
    /// <summary>
    /// 相机参数规格（可持久化）：描述一个可下发参数的取值范围与当前值。
    /// 【用途】既是 UI 限幅基准，也是下发前二次校验基准，保证「界面合规」与「实体校验」同源。
    /// 【序列化】字段均为简单类型，可直接 JSON 序列化并随实体落盘。
    /// </summary>
    public class CameraParameterSpec
    {
        /// <summary>参数名（与 SDK 参数键一致，如 "ExposureTime" / "Gain"）。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>参数类型（Int / Float / Enum）。</summary>
        public ParameterValueType ValueType { get; set; } = ParameterValueType.Float;

        /// <summary>当前值（字符串存储，便于 JSON 序列化与多类型共存）。</summary>
        public string CurrentValue { get; set; } = string.Empty;

        /// <summary>最小值（数值型有效；Enum 型可空）。</summary>
        public double? Min { get; set; }

        /// <summary>最大值（数值型有效）。</summary>
        public double? Max { get; set; }

        /// <summary>步长（数值型有效）。</summary>
        public double? Step { get; set; }

        /// <summary>是否为只读参数（只读参数禁止下发）。</summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// 是否为数值型参数（需要范围与步长校验）
        /// </summary>
        public bool IsNumeric => ValueType == ParameterValueType.Int || ValueType == ParameterValueType.Float;

        /// <summary>
        /// 深拷贝规格
        /// 【说明】实体克隆与规格缓存若共享引用，改一处会连带另一处，故必须复制。
        /// </summary>
        /// <returns>独立副本</returns>
        public CameraParameterSpec Clone()
        {
            return new CameraParameterSpec
            {
                Name = Name,
                ValueType = ValueType,
                CurrentValue = CurrentValue,
                Min = Min,
                Max = Max,
                Step = Step,
                IsReadOnly = IsReadOnly
            };
        }
    }

    /// <summary>参数值类型。</summary>
    public enum ParameterValueType
    {
        /// <summary>整型</summary>
        Int = 0,

        /// <summary>浮点型</summary>
        Float = 1,

        /// <summary>枚举型（取值以符号名存储）</summary>
        Enum = 2
    }
}
