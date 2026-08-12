using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Enums
{
    public enum CameraStatus
    {
        Idle,
        Disconnected,
        Connected,
        Streaming,
        Error,
        Fault,
        Offline

    }
}
