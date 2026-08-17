using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Shared.Extensions;
using MultiSerVIsion.Solution.Shared.Helpers;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Infrastructure.HiKHardware.HikCameraHardwareDriver;

namespace MultiSerVIsion.Solution.Application.Services
{
    /// <summary>
    /// 相机应用服务：编排相机的扫描、组态、连接、断开、采流等用例。
    /// 【职责】调用领域服务做业务校验、调用硬件驱动执行实际操作、发布领域事件。
    /// 【原则】不直接依赖 SDK，仅通过 ICameraHardwareDriver 接口与硬件交互，驱动内部按序列号管理多相机。
    /// </summary>
    public class CameraApplicationoService : ICameraAppService
    {
        private readonly ICameraDeviceService _cameraDomainService;   // 相机领域服务（业务规则校验）
        private readonly IDeviceManager _deviceManager;               // 设备内存管理器（组态设备）
        private readonly ICameraHardwareDriver _driver;               // 硬件驱动（内部按序列号管理多相机）
        private readonly IEventBus _eventBus;                         // 全局事件总线

        public CameraApplicationoService(
            ICameraDeviceService cameraDomainService,
            IDeviceManager cameraDeviceManager,
            ICameraHardwareDriver driver,
            IEventBus eventBus)
        {
            _cameraDomainService = cameraDomainService;
            _deviceManager = cameraDeviceManager;
            _driver = driver;
            _eventBus = eventBus;
        }

        /// <summary>相机帧到达事件：驱动采集到统一帧后向上层透传</summary>
        public event EventHandler<CameraFrameEventArgs> FrameReceived;

        /// <summary>
        /// 搜索所有在线相机（驱动内部已转换为统一 DTO 列表）
        /// </summary>
        public async Task<OperationResult<List<CameraDeviceDto>>> SearchOnlineCamera()
        {
            try
            {
                // 直接调用驱动扫描，ScanAsync 返回的已是 OperationResult<List<CameraDeviceDto>>
                return await _driver.ScanAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Error("搜索在线相机异常", ex);
                return OperationResult<List<CameraDeviceDto>>.Fail($"搜索失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 将扫描到的在线相机一键导入组态（落盘 JSON）
        /// </summary>
        /// <param name="scannedCamera">扫描得到的相机 DTO</param>
        public OperationResult<DeviceEntity> AddScannedCameraToConfig(CameraDeviceDto scannedCamera)
        {
            try
            {
                // 1. 防重复：按序列号判断是否已添加过
                var exist = _deviceManager.GetDevices<CameraEntity>()
                    .FirstOrDefault(c => c.CameraAllConfig.ConnectConfig.SerialNumber
                                         == scannedCamera.SerialNumber);
                if (exist != null)
                    return OperationResult<DeviceEntity>.Fail("该相机已在组态中，请勿重复添加");

                // 2. 扫描 DTO → 持久化实体（调用映射扩展方法）
                var cameraEntity = scannedCamera.ToNewCameraEntity();

                // 3. 加入内存管理器（内部自动执行校验+去重）
                bool addOk = _deviceManager.AddDevice(cameraEntity);
                if (!addOk)
                    return OperationResult<DeviceEntity>.Fail("添加失败，设备校验不通过");

                return OperationResult<DeviceEntity>.Succes(cameraEntity);
            }
            catch (Exception ex)
            {
                LogHelper.Error("添加扫描相机异常", ex);
                return OperationResult<DeviceEntity>.Fail($"添加失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 测试连接（短连接，登录后立即释放，不占用句柄）
        /// 【用途】保存组态前验证参数可达性，验证完即释放，不建立持久连接。
        /// </summary>
        /// <param name="connectConfig">连接参数</param>
        public async Task<OperationResult> TestConnectAsync(CameraConnectConfig connectConfig)
        {
            if (connectConfig == null)
                return OperationResult.Fail("连接参数不能为空");

            try
            {
                // 直接委托驱动执行短连接测试，驱动内部登录成功后立即释放资源
                return await _driver.TestConnectAsync(connectConfig);
            }
            catch (Exception ex)
            {
                LogHelper.Error("测试连接异常", ex);
                return OperationResult.Fail($"测试连接异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 连接已组态相机（长连接，句柄由驱动内部按序列号缓存）
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public async Task<OperationResult> ConnectCamera(string deviceId)
        {
            // 1. 取设备实体
            var device = _deviceManager.GetDeviceById(deviceId) as CameraEntity;
            if (device == null)
                return OperationResult.Fail("设备不存在");

            // 2. 业务规则校验：交给领域服务判断能否连接
            var validateResult = _cameraDomainService.ValidateCanConnect(device);
            if (!validateResult.Success)
                return validateResult;

            // 3. 组装硬件连接参数
            var connectConfig = new CameraConnectConfig
            {
                SerialNumber = device.CameraAllConfig.ConnectConfig.SerialNumber,
                IpAddress = device.IpAddress,
                InterfaceType = device.CameraAllConfig.ConnectConfig.InterfaceType
            };

            try
            {
                // 4. 调用驱动正式连接
                var connectResult = await _driver.ConnectAsync(connectConfig);
                if (!connectResult.Success)
                    return connectResult;

                // 5. 更新领域对象状态
                _cameraDomainService.ApplyConnectionStatus(device, true);

                // 6. 发布全局连接事件
                _eventBus.Publish(new DeviceConnectionChangedEveent(deviceId, true));

                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                LogHelper.Error("连接相机异常", ex);
                return OperationResult.Fail($"连接异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 在线扫描设备直连（无实体，跳过实体校验，直接调用驱动正式连接）
        /// </summary>
        /// <param name="config">连接参数</param>
        public async Task<OperationResult> ConnectOnlineCamera(CameraConnectConfig config)
        {
            if (config == null)
                return OperationResult.Fail("连接参数不能为空");

            try
            {
                var result = await _driver.ConnectAsync(config);
                if (result.Success)
                {
                    _eventBus.Publish(new DeviceConnectionChangedEveent(config.SerialNumber, true));
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("在线设备连接异常", ex);
                return OperationResult.Fail($"连接异常：{ex.Message}");
            }
        }

        /// <summary>获取相机实体，非相机类型返回 null</summary>
        private CameraEntity GetCameraEntity(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return null;
            return _deviceManager.GetDeviceById(deviceId) as CameraEntity;
        }

        /// <summary>
        /// 断开相机（驱动内部会先停流、释放缓冲、关闭设备）
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public OperationResult DisconnectCamera(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            var serialNumber = camera.CameraAllConfig.ConnectConfig.SerialNumber;
            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("设备缺少序列号，无法断开");

            try
            {
                // 同步等待异步断开结果（驱动内部为纯同步 SDK 操作，无死锁风险）
                var result = _driver.DisconnectAsync(serialNumber).GetAwaiter().GetResult();

                // 更新领域状态（成功与否都置为断开，避免半连接残留）
                _cameraDomainService.ApplyConnectionStatus(camera, false);

                return result.Success
                    ? OperationResult.Succes()
                    : OperationResult.Fail($"断开失败：{result.Message}");
            }
            catch (Exception ex)
            {
                LogHelper.Error("断开相机异常", ex);
                _cameraDomainService.ApplyConnectionStatus(camera, false);
                return OperationResult.Fail($"相机断开异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 开启连续采流（主动取流模式），帧通过 FrameReceived 事件透传
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public async Task<OperationResult> StartStream(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            // 业务校验：交给领域服务判断能否采流
            var validateResult = _cameraDomainService.ValidateCanStartStream(camera);
            if (!validateResult.Success)
                return validateResult;

            var serialNumber = camera.CameraAllConfig.ConnectConfig.SerialNumber;
            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("设备缺少序列号，无法开启采流");

            try
            {
                // 帧回调：将驱动统一帧包装为带设备 ID 的事件参数，向上层透传
                Action<CameraFrame> frameCallback = frame =>
                {
                    FrameReceived?.Invoke(this, new CameraFrameEventArgs(deviceId, frame));
                };

                // 调用驱动开启采流
                var result = await _driver.StartStreamAsync(serialNumber, frameCallback);
                if (!result.Success)
                    return result;

                camera.DetailStatus = CameraStatus.Streaming;
                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                LogHelper.Error("开启采流异常", ex);
                return OperationResult.Fail($"开启采流异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 停止采流，回到已连接状态
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public OperationResult StopStream(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            var serialNumber = camera.CameraAllConfig.ConnectConfig.SerialNumber;
            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("设备缺少序列号，无法停止采流");

            try
            {
                // 同步等待异步停流结果
                var result = _driver.StopStreamAsync(serialNumber).GetAwaiter().GetResult();

                // 停流成功回到已连接状态
                camera.DetailStatus = CameraStatus.Connected;

                return result.Success
                    ? OperationResult.Succes()
                    : OperationResult.Fail($"停止采流失败：{result.Message}");
            }
            catch (Exception ex)
            {
                LogHelper.Error("停止采流异常", ex);
                camera.DetailStatus = CameraStatus.Connected;
                return OperationResult.Fail($"停止采流异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 释放驱动资源（驱动内部会停流、断开所有相机并反初始化 SDK）
        /// </summary>
        public void ReleaseAll()
        {
            try
            {
                _driver.Dispose();
            }
            catch
            {
                // 忽略释放异常，保证清理不中断
            }
        }
    }
}
