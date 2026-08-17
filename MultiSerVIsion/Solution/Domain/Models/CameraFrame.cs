namespace MultiSerVIsion.Solution.Domain.Models
{
    /// <summary>
    /// 相机帧数据标准模型（跨层统一格式，无任何 SDK 依赖）
    /// 由硬件驱动层采集后填充，向上层业务透传。
    /// </summary>
    public class CameraFrame
    {
        /// <summary>图像宽度（像素）</summary>
        public int Width { get; set; }

        /// <summary>图像高度（像素）</summary>
        public int Height { get; set; }

        /// <summary>像素格式（统一业务枚举，与具体 SDK 枚举解耦）</summary>
        public PixelFormatEnum PixelFormat { get; set; }

        /// <summary>图像数据缓冲区（托管内存独立拷贝，脱离 SDK 生命周期）</summary>
        public byte[] Data { get; set; }

        /// <summary>帧时间戳（单位：纳秒）</summary>
        public long Timestamp { get; set; }

        /// <summary>帧序号</summary>
        public ulong FrameId { get; set; }
    }

    /// <summary>
    /// 统一像素格式枚举（业务层使用，不与具体 SDK 枚举绑定）
    /// </summary>
    public enum PixelFormatEnum
    {
        /// <summary>未知格式</summary>
        Unknown,
        /// <summary>8 位灰度</summary>
        Mono8,
        /// <summary>16 位灰度</summary>
        Mono16,
        /// <summary>24 位 RGB</summary>
        RGB24,
        /// <summary>24 位 BGR</summary>
        BGR24,
        /// <summary>32 位 RGB</summary>
        RGB32,
        /// <summary>32 位 BGR</summary>
        BGR32
    }
}
