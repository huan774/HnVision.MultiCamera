using MultiSerVIsion.Solution.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Events
{
    public class OnlineCameraSelectedEvent
    {
/*        public string DeviceId { get; }*/
        public CameraDeviceDto CameraDto { get; }
        public OnlineCameraSelectedEvent(/*string deviceId,*/ CameraDeviceDto config)
        {
            /*DeviceId = deviceId;*/
            CameraDto = config;
        }
    }
}
