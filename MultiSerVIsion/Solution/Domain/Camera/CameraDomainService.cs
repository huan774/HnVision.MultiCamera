using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Shared.Models;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Infrastructure.Camera.HikVision.HikCameraHardwareDriver;

namespace MultiSerVIsion.Solution.Domain.Camera
{
    public class CameraDomainService : ICameraDeviceService
    {
        /// 相机领域服务：封装所有相机相关的业务规则、状态校验、领域对象转换逻辑
        /// 【核心原则】不调用硬件SDK、不依赖基础设施、不处理UI逻辑，纯业务规则沉淀
        /// </summary>
        
            /// <summary>
            /// 校验设备是否允许执行连接操作
            /// 业务规则：已连接不可重复连接、故障状态需先复位、正在采流需先停止
            /// </summary>
            public OperationResult ValidateCanConnect(CameraEntity device)
            {
                if (device == null)
                    return OperationResult.Fail("设备实体不能为空");

                switch (device.DetailStatus)
                {
                    case CameraStatus.Connected:
                    case CameraStatus.Streaming:
                        return OperationResult.Fail("设备已处于连接/采流状态，无需重复连接");
                    case CameraStatus.Fault:
                        return OperationResult.Fail("设备处于故障状态，请先复位后再尝试连接");
                    case CameraStatus.Idle:
                    case CameraStatus.Disconnected:
                    case CameraStatus.Offline:
                        return OperationResult.Succes();
                    default:
                        return OperationResult.Fail($"未知设备状态，无法执行连接：{device.DetailStatus}");
                }
            }

            /// <summary>
            /// 校验设备是否允许开启采流
            /// 业务规则：必须已连接、未在采流中、非故障状态
            /// </summary>
            public OperationResult ValidateCanStartStream(CameraEntity device)
            {
                if (device == null)
                    return OperationResult.Fail("设备实体不能为空");

                if (device.DetailStatus == CameraStatus.Fault)
                    return OperationResult.Fail("设备故障，无法开启采流");

                if (device.DetailStatus == CameraStatus.Streaming)
                    return OperationResult.Fail("设备正在采流中，无需重复开启");

                if (device.DetailStatus != CameraStatus.Connected)
                    return OperationResult.Fail("设备未连接，请先连接设备后再开启采流");

                return OperationResult.Succes();
            }

            /// <summary>
            /// 校验设备是否允许断开连接
            /// 业务规则：采流中需先停流、故障状态可强制断开
            /// </summary>
            public OperationResult ValidateCanDisconnect(CameraEntity device)
            {
                if (device == null)
                    return OperationResult.Fail("设备实体不能为空");

                if (device.DetailStatus == CameraStatus.Disconnected ||
                    device.DetailStatus == CameraStatus.Offline)
                    return OperationResult.Fail("设备已处于断开状态");

                // 采流中允许断开，内部会自动停流，此处仅做提示级校验，可根据业务调整为禁止
                if (device.DetailStatus == CameraStatus.Streaming)
                {
                    // 可返回警告，也可直接禁止，按产品需求选择
                    return OperationResult.Succes("设备正在采流，断开将自动停止采集");
                }

                return OperationResult.Succes();
            }

            /// <summary>
            /// 校验参数下发合法性（下发前二次校验，含范围与步长校验）。
            /// 业务规则：只读参数禁止下发；枚举参数取值非空；数值参数需可解析、在范围内且为步长整数倍。
            /// </summary>
            /// <param name="device">目标相机实体</param>
            /// <param name="spec">参数规格（含目标值）</param>
            /// <returns>校验结果；不合法时 Message 说明原因</returns>
            public OperationResult ValidateParameter(CameraEntity device, CameraParameterSpec spec)
            {
                if (device == null)
                    return OperationResult.Fail("设备实体不能为空");

                if (spec == null || string.IsNullOrWhiteSpace(spec.Name))
                    return OperationResult.Fail("参数规格不能为空");

                if (spec.IsReadOnly)
                    return OperationResult.Fail($"参数 {spec.Name} 为只读，不允许下发");

                // 枚举型：取值以符号名存储，只要求非空
                if (spec.ValueType == ParameterValueType.Enum)
                {
                    return string.IsNullOrWhiteSpace(spec.CurrentValue)
                        ? OperationResult.Fail($"参数 {spec.Name} 的枚举取值不能为空")
                        : OperationResult.Succes();
                }

                if (!double.TryParse(spec.CurrentValue, out var numeric))
                    return OperationResult.Fail($"参数 {spec.Name} 需要数值");

                if (spec.Min.HasValue && numeric < spec.Min.Value)
                    return OperationResult.Fail($"参数 {spec.Name} 不能小于 {spec.Min.Value}");

                if (spec.Max.HasValue && numeric > spec.Max.Value)
                    return OperationResult.Fail($"参数 {spec.Name} 不能大于 {spec.Max.Value}");

                // 步长校验：以 Min 为基准判断是否落在步长整点上（带容差，享避浮点误差导致误拦）
                if (spec.Step.HasValue && spec.Step.Value > 0 && spec.Min.HasValue)
                {
                    double ratio = (numeric - spec.Min.Value) / spec.Step.Value;
                    if (Math.Abs(ratio - Math.Round(ratio)) > 1e-3)
                        return OperationResult.Fail($"参数 {spec.Name} 必须为步长 {spec.Step.Value} 的整数倍");
                }

                return OperationResult.Succes();
            }

            /// <summary>
            /// 将参数值限幅到 [Min, Max] 并按 Step 对齐（UI 限幅基准）。
            /// 【说明】与 <see cref="ValidateParameter"/> 共用同一份规格，保证界面与实体校验规则一致。
            /// </summary>
            /// <param name="spec">参数规格</param>
            /// <param name="rawValue">原始输入值</param>
            /// <returns>限幅并取整后的安全值（字符串形式）；无法解析时原样返回</returns>
            public string ClampParameter(CameraParameterSpec spec, string rawValue)
            {
                if (spec == null) return rawValue;
                if (spec.ValueType == ParameterValueType.Enum) return rawValue;

                if (!double.TryParse(rawValue, out var numeric)) return rawValue;

                double result = numeric;
                if (spec.Min.HasValue && result < spec.Min.Value) result = spec.Min.Value;
                if (spec.Max.HasValue && result > spec.Max.Value) result = spec.Max.Value;

                if (spec.Step.HasValue && spec.Step.Value > 0)
                {
                    double baseValue = spec.Min ?? 0;
                    result = baseValue + Math.Round((result - baseValue) / spec.Step.Value) * spec.Step.Value;

                    // 对齐后可能又越界，二次夹取保证结果始终落在合法区间
                    if (spec.Min.HasValue && result < spec.Min.Value) result = spec.Min.Value;
                    if (spec.Max.HasValue && result > spec.Max.Value) result = spec.Max.Value;
                }

                // 整型参数不允许输出小数，否则下发会被相机拒绝
                if (spec.ValueType == ParameterValueType.Int)
                    return Math.Round(result).ToString();

                return result.ToString();
            }

            /// <summary>
            /// 校验是否允许切换采集模式（连续 / 软触发互斥，采流中禁止切换）。
            /// 业务规则：采流中必须先停流；故障状态不允许切换。
            /// </summary>
            /// <param name="device">目标相机实体</param>
            /// <param name="targetMode">目标采集模式</param>
            /// <returns>校验结果</returns>
            public OperationResult ValidateCanSwitchAcquisitionMode(CameraEntity device, AcquisitionMode targetMode)
            {
                if (device == null)
                    return OperationResult.Fail("设备实体不能为空");

                if (device.AcquisitionMode == targetMode)
                    return OperationResult.Succes("采集模式未变化");

                if (device.DetailStatus == CameraStatus.Streaming)
                    return OperationResult.Fail("设备正在采流，请先停止采集后再切换采集模式");

                if (device.DetailStatus == CameraStatus.Fault)
                    return OperationResult.Fail("设备处于故障状态，无法切换采集模式");

                return OperationResult.Succes();
            }

            /// <summary>
            /// 校验是否允许执行软触发单步（仅软触发模式且已连接）。
            /// 业务规则：未连接不可触发；连续采集模式下禁止单步。
            /// </summary>
            /// <param name="device">目标相机实体</param>
            /// <returns>校验结果</returns>
            public OperationResult ValidateCanSoftwareTrigger(CameraEntity device)
            {
                if (device == null)
                    return OperationResult.Fail("设备实体不能为空");

                if (device.DetailStatus != CameraStatus.Connected &&
                    device.DetailStatus != CameraStatus.Streaming)
                    return OperationResult.Fail("设备未连接，无法执行软触发");

                if (device.AcquisitionMode != AcquisitionMode.SoftTrigger)
                    return OperationResult.Fail("当前为连续采集模式，请先切换为软触发模式再执行单步");

                return OperationResult.Succes();
            }

            /// <summary>
            /// 将扫描结果批量转换为相机实体，自动按序列号去重
            /// 业务规则：同序列号已存在则跳过，不存在则生成新实体并填充默认配置
            /// </summary>
            /// <param name="scanResult">扫描到的在线相机列表</param>
            /// <param name="existingDevices">已存在的组态设备列表</param>
            /// <param name="skippedSerials">输出：因重复被跳过的序列号</param>
            /// <returns>待新增的新设备实体列表</returns>
           /* public List<CameraEntity> CreateEntitiesFromScan(
                List<CameraDeviceDto> scanResult,
                List<CameraEntity> existingDevices,
                out List<string> skippedSerials)
            {
                skippedSerials = new List<string>();
                var newEntities = new List<CameraEntity>();

                if (scanResult == null || scanResult.Count == 0)
                    return newEntities;

                // 已存在设备的序列号哈希表，用于快速去重
                var existSerialSet = new HashSet<string>(
                    existingDevices?.Where(d => !string.IsNullOrEmpty(d.SerialNumber))
                                    .Select(d => d.SerialNumber)
                    ?? Enumerable.Empty<string>());

                foreach (var dto in scanResult)
                {
                    if (string.IsNullOrEmpty(dto.SerialNumber))
                        continue;

                    if (existSerialSet.Contains(dto.SerialNumber))
                    {
                        skippedSerials.Add(dto.SerialNumber);
                        continue;
                    }

                    // 生成新实体，填充扫描到的基础信息 + 业务默认值
                    var entity = new CameraEntity
                    {
                        DeviceId = Guid.NewGuid().ToString("N"), // 业务主键
                        DeviceName = string.IsNullOrEmpty(dto.DeviceName)
                            ? $"相机_{dto.SerialNumber.Substring(dto.SerialNumber.Length - 4)}"
                            : dto.DeviceName,
                        SerialNumber = dto.SerialNumber,
                        IpAddress = dto.IpAddress,
                        InterfaceType = dto.InterfaceType,
                        Manufacturer = dto.Manufacturer,
                        Model = dto.Model,
                        DetailStatus = CameraStatus.Disconnected,
                        IsTemporary = false, // 正式生成的组态设备，非临时
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    };

                    // 填充默认参数配置（业务默认值，领域规则）
                    entity.ApplyDefaultConfig();

                    newEntities.Add(entity);
                }

                return newEntities;
            }*/

            /// <summary>
            /// 应用连接状态变更到领域实体
            /// 业务规则：统一管理状态流转，避免外部直接修改实体状态
            /// </summary>
            public void ApplyConnectionStatus(CameraEntity device, bool isConnected)
            {
                if (device == null) return;

                if (isConnected)
                {
                    // 连接成功：从空闲/断开/离线/故障转为已连接
                    if (device.DetailStatus == CameraStatus.Idle ||
                        device.DetailStatus == CameraStatus.Disconnected ||
                        device.DetailStatus == CameraStatus.Offline ||
                        device.DetailStatus == CameraStatus.Fault)
                    {
                        device.DetailStatus = CameraStatus.Connected;
                        device.LastConnectTime = DateTime.Now;
                    }
                }
                else
                {
                    // 断开连接：采流中自动先停流，再转为断开
                    if (device.DetailStatus == CameraStatus.Streaming)
                    {
                        device.DetailStatus = CameraStatus.Connected;
                    }
                    device.DetailStatus = CameraStatus.Disconnected;
                   /* device.LastDisconnectTime = DateTime.Now;*/
                }

              /*  device.UpdateTime = DateTime.Now;*/
            }

            /// <summary>
            /// 应用采流状态变更到领域实体
            /// </summary>
         /*   public void ApplyStreamStatus(CameraEntity device, bool isStreaming)
            {
                if (device == null) return;

                if (isStreaming)
                {
                    if (device.DetailStatus == CameraStatus.Connected)
                    {
                        device.DetailStatus = CameraStatus.Streaming;
                    }
                }
                else
                {
                    if (device.DetailStatus == CameraStatus.Streaming)
                    {
                        device.DetailStatus = CameraStatus.Connected;
                    }
                }

                device.UpdateTime = DateTime.Now;
            }*/

            /// <summary>
            /// 校验相机参数配置是否符合业务规则
            /// 业务规则：曝光时间范围、增益范围、帧率上限等业务约束
            /// </summary>
            public OperationResult ValidateCameraConfig(CameraParamConfig config)
            {
                if (config == null)
                    return OperationResult.Fail("配置参数不能为空");

                // 示例业务规则：曝光时间 10us ~ 1000000us
                if (config.ExposureTime < 10 || config.ExposureTime > 1_000_000)
                    return OperationResult.Fail("曝光时间超出允许范围（10~1000000μs）");

                // 示例业务规则：增益 0 ~ 24dB
                if (config.Gain < 0 || config.Gain > 24)
                    return OperationResult.Fail("增益值超出允许范围（0~24dB）");

                // 示例业务规则：帧率不能超过相机型号最大支持
                if (config.FrameRate <= 0 || config.FrameRate > 60)
                    return OperationResult.Fail("帧率超出允许范围（0~60fps）");

                return OperationResult.Succes();
            }

            /// <summary>
            /// 批量自动创建设备的业务逻辑封装（供应用层调用）
            /// 对应你原接口的 AutoCreateFromScanAsync
            /// </summary>
         /*   public OperationResult<List<CameraEntity>> AutoCreateFromScan(
                List<CameraDeviceDto> scanResult,
                List<CameraEntity> existingDevices)
            {
                var newEntities = CreateEntitiesFromScan(scanResult, existingDevices, out var skipped);

                var result = OperationResult<List<CameraEntity>>.Succes(newEntities);
                if (skipped.Count > 0)
                {
                    result.Message = $"新增{newEntities.Count}台，跳过{skipped.Count}台已存在设备";
                }

                return result;
            }*/
        }
    }
       

