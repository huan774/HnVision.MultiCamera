
using MultiSerVIsion.Solution.Domain.Camera;

using MultiSerVIsion.Solution.Domain.Device;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Camera
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

    /// <summary>校验参数下发合法性（下发前二次校验，含范围与步长校验）。</summary>
    /// <param name="device">目标相机实体。</param>
    /// <param name="spec">参数规格（含目标值）。</param>
    /// <returns>校验结果；不合法时 Message 说明原因。</returns>
    OperationResult ValidateParameter(CameraEntity device, CameraParameterSpec spec);

    /// <summary>将参数值限幅到 [Min, Max] 并按 Step 对齐（UI 限幅基准）。</summary>
    /// <param name="spec">参数规格。</param>
    /// <param name="rawValue">原始输入值。</param>
    /// <returns>限幅并取整后的安全值（字符串形式）。</returns>
    string ClampParameter(CameraParameterSpec spec, string rawValue);

    /// <summary>校验是否允许切换采集模式（连续 / 软触发互斥，采流中禁止切换）。</summary>
    /// <param name="device">目标相机实体。</param>
    /// <param name="targetMode">目标采集模式。</param>
    /// <returns>校验结果。</returns>
    OperationResult ValidateCanSwitchAcquisitionMode(CameraEntity device, AcquisitionMode targetMode);

    /// <summary>校验是否允许执行软触发单步（仅软触发模式且已连接）。</summary>
    /// <param name="device">目标相机实体。</param>
    /// <returns>校验结果。</returns>
    OperationResult ValidateCanSoftwareTrigger(CameraEntity device);
    }
}
