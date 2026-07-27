using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Application.Dtos
{
    public class ImageCaptureResult
    {
        public bool IsSuccess { get; set; }
        public byte[] ImageData { get; set; }  // 图像帧数据
        public int Width { get; set; }
        public int Height { get; set; }
        public string PixelFormat { get; set; }
        public long FrameId { get; set; }       // 帧号
        public DateTime CaptureTime { get; set; } // 采集时间戳
        public string ErrorMessage { get; set; }
    }
}
