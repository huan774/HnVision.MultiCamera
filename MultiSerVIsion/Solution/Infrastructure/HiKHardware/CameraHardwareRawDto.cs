using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    public class CameraHardwareRawDto
    {
        public string IpAddress { get; set; } = string.Empty;
        /// <summary>序列号</summary>
        public string SerialNumber { get; set; } = string.Empty;
        /// <summary>设备型号</summary>
        public string Model { get; set; } = string.Empty;
        /// <summary>接口类型 GigE/USB</summary>
        public string InterfaceType { get; set; } = string.Empty;
        /// <summary>MAC地址（SDK原始信息）</summary>
        public string MacAddress { get; set; } = string.Empty;
        /// <summary>SDK内部设备唯一标识（底层专用，上层丢弃）</summary>
        public string SdkDeviceKey { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
    }
}
