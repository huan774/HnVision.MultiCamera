using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Events
{
    public class DeviceConnectionChangedEveent
    {
        public string DeviceId { get; }
        public bool IsConnected { get; }
        public DeviceConnectionChangedEveent(string deviceId, bool isConnected)
        {
            DeviceId = deviceId;
            IsConnected = isConnected;
        }
    }
}
