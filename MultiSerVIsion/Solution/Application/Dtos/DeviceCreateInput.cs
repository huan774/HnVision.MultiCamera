using MultiSerVIsion.Solution.Domain.Entities.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Application.Dtos
{
    public class DeviceCreateInput
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; }=string.Empty;
        public string GroupTag {  get; set; } = string.Empty;
        public string IpAddress {  get; set; } = string.Empty;
        public bool IsEnable {  get; set; }

      
        public CameraConnectConfig CameraConfig { get; set; }
        public MotionCardConnectConfig MotionCardConfig { get; set; }
    }
}
