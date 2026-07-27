using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Shared.Models;

namespace MultiSerVIsion.Solution.Domain.Services
{
    public interface IDeviceDomainSerivce
    {
        ValidationResult CanCopyDevice(DeviceEntity source);
        DeviceEntity CloneDevice(DeviceEntity source);
        ValidationResult CheckToggleEnable(DeviceEntity device);
        ValidationResult ValidateDeviceEntity(DeviceEntity entity);

       
    }
}
