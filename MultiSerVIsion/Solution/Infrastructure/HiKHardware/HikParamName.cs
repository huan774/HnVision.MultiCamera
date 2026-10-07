namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    /// <summary>
    /// 海康相机参数节点名常量集合。
    /// 【职责】集中管理驱动读写参数所用的 GenICam 节点字符串，避免散落的魔法字符串。
    /// 【分层】仅硬件层内部使用，不向上层暴露 SDK 节点细节。
    /// </summary>
    internal static class HikParamName
    {
        // ---------- 高频参数：用户运行过程中需要频繁微调，可在采流中下发 ----------

        /// <summary>曝光时间（单位：微秒）</summary>
        public const string ExposureTime = "ExposureTime";

        /// <summary>增益（单位：dB）</summary>
        public const string Gain = "Gain";

        /// <summary>采集帧率（单位：fps）</summary>
        public const string AcquisitionFrameRate = "AcquisitionFrameRate";

        /// <summary>采集帧率使能开关：未使能时写入帧率不生效</summary>
        public const string AcquisitionFrameRateEnable = "AcquisitionFrameRateEnable";

        // ---------- 基础参数：连接后一次性配置，写入前需停止采集 ----------

        /// <summary>像素格式</summary>
        public const string PixelFormat = "PixelFormat";

        /// <summary>触发模式（连续 / 触发）</summary>
        public const string TriggerMode = "TriggerMode";

        /// <summary>触发源（软触发 / 硬触发线路）</summary>
        public const string TriggerSource = "TriggerSource";
    }
}
