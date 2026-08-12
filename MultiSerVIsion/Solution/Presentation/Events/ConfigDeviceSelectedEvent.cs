using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Events
{
    public class ConfigDeviceSelectedEvent
    {
        public string DeviceId {  get;}
        public ConfigDeviceSelectedEvent(string deviceId)=>DeviceId = deviceId;
    }
}
