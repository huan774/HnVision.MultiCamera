using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Models
{
    public class CameraDeviceDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string InterfaceType { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
    }
}
