using System.Collections.Generic;
using System.Linq;

namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    /// <summary>参数值类型：决定使用哪一组 SDK 读写 API</summary>
    public enum HikParamValueType
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

    /// <summary>参数分组：区分参数的调节时机</summary>
    public enum HikParamGroup
    {
        /// <summary>高频参数：用户运行中频繁调节，可在采流过程中下发</summary>
        HighFrequency,

        /// <summary>基础参数：连接后一次性配置，写入前需停止采集</summary>
        Basic
    }

    /// <summary>
    /// 相机参数描述符：描述一个参数节点的名称、类型、分组与完整规格（范围 / 步长 / 可选项）。
    /// 【职责】把「SDK 节点名 ↔ 业务参数」的映射集中在硬件层，
    /// 供驱动做通用读写，并为上层提供界面限幅与实体校验所需的全部元信息。
    /// 【副本语义】<see cref="Find"/> 一律返回副本，回填范围/步长不会污染全局参数目录。
    /// </summary>
    public class HikDescriptor
    {
        /// <summary>参数名（即 SDK 节点名，如 ExposureTime）</summary>
        public string ParamName { get; }

        /// <summary>参数分组（高频 / 基础），决定下发前是否需要停止采集</summary>
        public HikParamGroup Group { get; }

        /// <summary>节点值类型</summary>
        public HikParamValueType ValueType { get; internal set; }

        /// <summary>界面显示名</summary>
        public string DisplayName { get; }

        /// <summary>单位（无单位时为空字符串）</summary>
        public string Unit { get; }

        /// <summary>允许的最小值（查询参数后由相机回填，用于界面限幅与实体校验）</summary>
        public double Min { get; internal set; }

        /// <summary>允许的最大值（查询参数后由相机回填，用于界面限幅与实体校验）</summary>
        public double Max { get; internal set; }

        /// <summary>最小步长；≤0 表示相机未提供步长约束（如多数浮点节点）</summary>
        public double Step { get; internal set; }

        /// <summary>是否可写（只读节点不允许下发）</summary>
        public bool IsWritable { get; internal set; } = true;

        /// <summary>枚举型参数支持的符号名集合（非枚举参数为空集合）</summary>
        public IReadOnlyList<string> EnumEntries { get; internal set; } = new List<string>();

        /// <summary>
        /// 读取结果携带的当前值（double / long / string / bool）。
        /// 【说明】仅在读取接口返回的描述符上有意义；仅查询规格时为 null。
        /// </summary>
        public object CurrentValue { get; internal set; }

        /// <summary>是否存在有效范围（Max &gt; Min 表示范围已回填）</summary>
        public bool HasRange => Max > Min;

        /// <summary>
        /// 构造参数描述符
        /// </summary>
        /// <param name="paramName">参数名（SDK 节点名）</param>
        /// <param name="group">参数分组</param>
        /// <param name="valueType">节点值类型</param>
        /// <param name="displayName">界面显示名</param>
        /// <param name="unit">单位，可为空</param>
        public HikDescriptor(string paramName, HikParamGroup group, HikParamValueType valueType,
            string displayName, string unit = "")
        {
            ParamName = paramName;
            Group = group;
            ValueType = valueType;
            DisplayName = displayName;
            Unit = unit ?? string.Empty;
        }

        /// <summary>
        /// 深拷贝描述符
        /// 【说明】全局目录中的实例是共享的，回填前必须复制，否则多相机/多次读取会互相覆盖范围。
        /// </summary>
        /// <returns>独立副本</returns>
        public HikDescriptor Clone()
        {
            return new HikDescriptor(ParamName, Group, ValueType, DisplayName, Unit)
            {
                Min = Min,
                Max = Max,
                Step = Step,
                IsWritable = IsWritable,
                EnumEntries = EnumEntries == null
                    ? new List<string>()
                    : new List<string>(EnumEntries),
                CurrentValue = CurrentValue
            };
        }

        /// <summary>高频参数：用户运行过程中频繁调节，可在采流中下发</summary>
        public static readonly HikDescriptor[] HighFrequency =
        {
            new HikDescriptor(HikParamName.ExposureTime, HikParamGroup.HighFrequency,
                HikParamValueType.Float, "曝光时间", "μs"),
            new HikDescriptor(HikParamName.Gain, HikParamGroup.HighFrequency,
                HikParamValueType.Float, "增益", "dB"),
            new HikDescriptor(HikParamName.AcquisitionFrameRate, HikParamGroup.HighFrequency,
                HikParamValueType.Float, "采集帧率", "fps")
        };

        /// <summary>基础参数：连接后一次性配置，写入前需停止采集</summary>
        public static readonly HikDescriptor[] Basic =
        {
            new HikDescriptor(HikParamName.PixelFormat, HikParamGroup.Basic,
                HikParamValueType.Enum, "像素格式"),
            new HikDescriptor(HikParamName.TriggerMode, HikParamGroup.Basic,
                HikParamValueType.Enum, "触发模式"),
            new HikDescriptor(HikParamName.TriggerSource, HikParamGroup.Basic,
                HikParamValueType.Enum, "触发源")
        };

        /// <summary>全部已登记参数（高频 + 基础）</summary>
        public static readonly HikDescriptor[] All =
            HighFrequency.Concat(Basic).ToArray();

        /// <summary>
        /// 按参数名查找已登记的描述符副本
        /// </summary>
        /// <param name="paramName">参数名（SDK 节点名）</param>
        /// <returns>找到时返回副本（可安全回填），未登记时返回 null</returns>
        public static HikDescriptor Find(string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName)) return null;

            var matched = All.FirstOrDefault(
                d => string.Equals(d.ParamName, paramName, System.StringComparison.OrdinalIgnoreCase));

            return matched?.Clone();
        }
    }
}
