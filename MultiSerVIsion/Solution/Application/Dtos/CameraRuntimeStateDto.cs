using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Application.Dtos
{
    public class CameraRuntimeStateDto
    {
        public double Temperature { get; set; }     // 相机温度
        public double ActualFrameRate { get; set; } // 实际帧率
        public long LostFrameCount { get; set; }    // 丢帧计数
        public double ActualExposure { get; set; }  // 实际曝光值
        public double ActualGain { get; set; }      // 实际增益
        public string LastError { get; set; }      // 最近一次错误
    }
}
