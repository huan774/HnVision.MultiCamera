using MultiSerVIsion.Solution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Factory
{
    public static class DeviceFactory
    {
        public static DeviceEntity CreateDevice(string deviceType)
        {
            switch (deviceType)
            {
                case "Camera":
                    return new CameraEntity();
                 /*case "Motion":
                     return new MotionDeviceEntity();*/
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType));
            }

        }           
            
    }
}
