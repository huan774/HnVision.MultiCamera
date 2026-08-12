using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Services
{
    public interface ICameraDeviceService
    {
       /// <summary>
    /// 业务校验：判断当前设备是否允许连接
    /// 规则：已连接设备不可重复连接、故障设备需先复位、离线设备不允许直接取流
    /// </summary>
    OperationResult ValidateCanConnect(CameraEntity device);

    /// <summary>
    /// 业务校验：判断当前设备是否允许开启取流
    /// 规则：未连接设备不能取流、正在采集中不能重复开启
    /// </summary>
    OperationResult ValidateCanStartStream(CameraEntity device);

    /// <summary>
    /// 领域逻辑：将扫描结果批量转换为设备实体，自动去重、填充默认参数
    /// </summary>
    /// <param name="scanResult">扫描到的在线相机列表</param>
    /// <param name="existingDevices">已存在的组态设备</param>
    /// <param name="skippedSerials">跳过的重复序列号</param>
   /* List<CameraEntity> CreateEntitiesFromScan(
        List<CameraDeviceDto> scanResult,
        List<CameraEntity> existingDevices,
        out List<string> skippedSerials);*/

    /// <summary>
    /// 领域逻辑：应用连接状态变更，更新实体状态
    /// </summary>
    void ApplyConnectionStatus(CameraEntity device, bool isConnected);

    /// <summary>
    /// 业务校验：参数配置是否符合业务范围
    /// </summary>
    OperationResult ValidateCameraConfig(CameraParamConfig config);
    }
}
