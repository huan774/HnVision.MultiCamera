using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Application.Dtos
{
    public class CameraDisplayDto
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public string StatusText { get; set; }    // 界面显示的状态文本
        public bool IsOnline { get; set; }
    }
}
