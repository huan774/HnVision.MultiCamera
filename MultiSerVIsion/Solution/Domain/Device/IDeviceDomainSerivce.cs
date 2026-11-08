using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using MultiSerVIsion.Solution.Shared.Models;

namespace MultiSerVIsion.Solution.Domain.Device
{
    public interface IDeviceDomainSerivce
    {
        ValidationResult CanCopyDevice(DeviceEntity source);
        DeviceEntity CloneDevice(DeviceEntity source);
        ValidationResult CheckToggleEnable(DeviceEntity device);
        ValidationResult ValidateDeviceEntity(DeviceEntity entity);

    }
}
