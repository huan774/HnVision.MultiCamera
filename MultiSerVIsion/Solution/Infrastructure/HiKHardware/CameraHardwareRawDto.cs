using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    public class CameraHardwareRawDto
    {
        public string SerialNumber { get; set; }  // 硬件唯一序列号
        public string IpAddress { get; set; }     // 硬件当前IP
        public string ModelName { get; set; }     // 硬件型号
        public string InterfaceType { get; set; } // GigE/USB/CameraLink
                                                  // SDK原生设备信息结构体，仅连接时用，不持久化
        public object RawDeviceInfo { get; set; }
    }
}
