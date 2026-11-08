using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Device
{
    public static class DeviceFactory
    {
        public static DeviceEntity CreateDevice(string deviceType)
        {
            switch (deviceType)
            {
                case "Camera":
                    return new CameraEntity();
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType));
            }

        }           
            
    }
}
