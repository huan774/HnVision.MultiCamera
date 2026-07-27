using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Entities.Configs
{
    public class CameraConnectConfig
    {

        public string SerialNumber { get; set; }   // 唯一标识，用来匹配硬件
        public string IpAddress { get; set; }      // 目标IP
        public int Port { get; set; }              // 通讯端口
        public string InterfaceType { get; set; }  // 接口类型
        public int ConnectTimeoutMs { get; set; }  // 连接超时
                                                   // 部分品牌相机需要用户名密码
        public string UserName { get; set; }
        public string Password { get; set; }

    }
    public class CameraParamConfig
    {
        public double ExposureTime { get; set; } = 1000;   // 曝光时间
        public double Gain { get; set; } = 1.0;           // 增益
        public string PixelFormat { get; set; } = "Mono8";   // 像素格式
        public double FrameRate { get; set; } = 30;      // 帧率
        public string TriggerMode { get; set; }="Continuous";    // 触发模式：连续/触发
        public string TriggerSource { get; set; }  // 触发源：软触发/硬触发
    }
}
