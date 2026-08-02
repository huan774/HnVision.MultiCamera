using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;

/*using MultiSerVIsion.Solution.Presentation.Winforms;*/
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Application
{
    public interface IDeviceAppService
    {
        OperationResult<DeviceEntity> CreateDevice(DeviceCreateInput input);
        OperationResult<DeviceEntity> CopyDevice(string sourceDevId);
        OperationResult<bool> DeleteDevice(string devId);
        OperationResult<bool>ToggleDeviceEnable(string devId);
        OperationResult<List<DeviceEntity>> GetDeviceByGroup(string devId);
        OperationResult<List<DeviceEntity>> GetAllDevices();
        OperationResult<DeviceEntity> UpdataDevice(DeviceUpdateIput input);
        OperationResult<DeviceEntity> GetDeviceById(string devId);
        OperationResult LoadProject();
        OperationResult SaveProject();

    }
}
