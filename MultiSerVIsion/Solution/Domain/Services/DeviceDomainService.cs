using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Infrastructure.Repository;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MultiSerVIsion.Solution.Domain.Services
{
    public class DeviceDomainService:IDeviceDomainSerivce
    {
        public ValidationResult CanCopyDevice(DeviceEntity source)
        {
            if (source == null) return ValidationResult.Failure("原设备不存在");
            return ValidationResult.Success();
        }
       public DeviceEntity CloneDevice(DeviceEntity source)
       {
            var copy=source.ShallowClone();
            copy.DeviceId=Guid.NewGuid().ToString("N");
            copy.DeviceName += "_副本";
            copy.IsEnable = false;
            return copy;
       }
       public ValidationResult CheckToggleEnable(DeviceEntity device) 
        {
            return ValidationResult.Success();
        }
       public ValidationResult ValidateDeviceEntity(DeviceEntity entity)
        {
            if (entity == null)
                return ValidationResult.Failure("数据设备不能为空");
            return entity.SelfValidate();
        }
    }
}
