using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Shared.Extensions;
using MultiSerVIsion.Solution.Shared.Models;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Infrastructure.HiKHardware.HikCameraHardwareDriver;

namespace MultiSerVIsion.Solution.Domain.Services
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
                    // 连接成功：从断开/离线转为已连接
                    if (device.DetailStatus == CameraStatus.Disconnected ||
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
       

