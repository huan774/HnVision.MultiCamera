using System;
using System.Collections.Generic;

namespace MultiSerVIsion.Solution.Domain.Entities.Configs
{
    /// <summary>参数值类型：决定界面控件形态与校验分支（与具体 SDK 解耦）</summary>
    public enum ParamValueType
    {
        /// <summary>浮点型（曝光时间、增益、帧率等）</summary>
        Float,

        /// <summary>整型</summary>
        Int,

        /// <summary>枚举型（像素格式、触发模式等）</summary>
        Enum,

        /// <summary>布尔型</summary>
        Bool
    }

    /// <summary>参数分组：决定下发时机</summary>
    public enum ParamGroup
    {
        /// <summary>高频参数：运行中频繁调节，可在采流过程中下发</summary>
        HighFrequency,

        /// <summary>基础参数：连接后一次性配置，写入前需停止采集</summary>
        Basic
    }

    /// <summary>
    /// 单个可调参数的规格：范围、步长、可选项等元数据。
    /// 【职责】连接成功后由硬件层从相机回读并填充，随实体一并持久化，
    /// 作为「实体侧参数校验」与「界面限幅」共同遵守的唯一权威来源，
    /// 从而在未连接相机时也能校验与限幅。
    /// </summary>
    public class ParamSpec
    {
        /// <summary>浮点比较容差：规避相机上报步长与界面 double 值的精度误差</summary>
        private const double Epsilon = 1e-6;

        /// <summary>步长倍数判定容差：值可能不是步长的精确整数倍</summary>
        private const double StepTolerance = 1e-3;

        /// <summary>参数名（相机节点名，如 ExposureTime）</summary>
        public string ParamName { get; set; } = string.Empty;

        /// <summary>界面显示名</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>单位（无单位时为空字符串）</summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>参数分组</summary>
        public ParamGroup Group { get; set; } = ParamGroup.Basic;

        /// <summary>参数值类型</summary>
        public ParamValueType ValueType { get; set; } = ParamValueType.Float;

        /// <summary>允许的最小值</summary>
        public double Min { get; set; }

        /// <summary>允许的最大值</summary>
        public double Max { get; set; }

        /// <summary>最小步长；≤0 表示无步长约束</summary>
        public double Step { get; set; }

        /// <summary>是否可写（只读节点不允许下发）</summary>
        public bool IsWritable { get; set; } = true;

        /// <summary>枚举型参数的可选项（符号名），非枚举参数为空集合</summary>
        public List<string> Options { get; set; } = new List<string>();

        /// <summary>是否为数值型参数（需要范围与步长校验）</summary>
        public bool IsNumeric =>
            ValueType == ParamValueType.Float || ValueType == ParamValueType.Int;

        /// <summary>是否存在有效范围（Max ≤ Min 视为尚未回读，不做范围约束）</summary>
        public bool HasRange => Max > Min;

        /// <summary>
        /// 判断数值是否落在合法范围内
        /// </summary>
        /// <param name="value">待校验数值</param>
        /// <returns>非数值型或范围未回读时视为通过</returns>
        public bool IsInRange(double value)
        {
            if (!IsNumeric || !HasRange) return true;
            return value >= Min - Epsilon && value <= Max + Epsilon;
        }

        /// <summary>
        /// 判断数值是否为步长的整数倍（以 Min 为基准）
        /// </summary>
        /// <param name="value">待校验数值</param>
        /// <returns>非数值型或无步长约束时视为通过</returns>
        public bool IsOnStep(double value)
        {
            if (!IsNumeric || Step <= Epsilon) return true;

            double ratio = (value - Min) / Step;
            return Math.Abs(ratio - Math.Round(ratio)) < StepTolerance;
        }

        /// <summary>
        /// 把数值吸附到最近的合法值：先按范围夹取，再按步长取整。
        /// 【用途】界面限幅与「非法输入纠正为最近合法值」共用同一算法。
        /// </summary>
        /// <param name="value">原始数值</param>
        /// <returns>夹取并吸附后的合法值</returns>
        public double Snap(double value)
        {
            if (!IsNumeric) return value;

            double result = value;
            if (HasRange)
            {
                if (result < Min) result = Min;
                else if (result > Max) result = Max;
            }

            if (Step > Epsilon)
            {
                double ratio = Math.Round((result - Min) / Step);
                result = Min + ratio * Step;
            }

            return result;
        }

        /// <summary>
        /// 深拷贝规格
        /// 【说明】实体克隆与规格缓存若共享引用，改一处会连带另一处，故必须复制。
        /// </summary>
        /// <returns>独立副本</returns>
        public ParamSpec Clone()
        {
            return new ParamSpec
            {
                ParamName = ParamName,
                DisplayName = DisplayName,
                Unit = Unit,
                Group = Group,
                ValueType = ValueType,
                Min = Min,
                Max = Max,
                Step = Step,
                IsWritable = IsWritable,
                Options = Options == null ? new List<string>() : new List<string>(Options)
            };
        }
    }

    /// <summary>
    /// 相机参数节点名常量
    /// 【职责】领域侧唯一参数名清单，与硬件层节点名保持一致，
    /// 供实体校验与界面声明共用，避免各处硬编码字符串而出现拼写分叉。
    /// </summary>
    public static class CameraParamNames
    {
        /// <summary>曝光时间</summary>
        public const string ExposureTime = "ExposureTime";

        /// <summary>增益</summary>
        public const string Gain = "Gain";

        /// <summary>采集帧率</summary>
        public const string AcquisitionFrameRate = "AcquisitionFrameRate";

        /// <summary>像素格式</summary>
        public const string PixelFormat = "PixelFormat";

        /// <summary>触发模式</summary>
        public const string TriggerMode = "TriggerMode";

        /// <summary>触发源</summary>
        public const string TriggerSource = "TriggerSource";
    }
}
