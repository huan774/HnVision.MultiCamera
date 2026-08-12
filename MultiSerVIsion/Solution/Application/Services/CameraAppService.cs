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
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Infrastructure.HiKHardware.HikCameraHardwareDriver;

namespace MultiSerVIsion.Solution.Application.Services
{
    public class CameraApplicationoService : ICameraAppService
    {
        private readonly ICameraDeviceService _cameraDomainService;
        private readonly IDeviceManager _deviceManager;
        private readonly ICameraHardwareDriver _driver;       // 硬件驱动
        private readonly IEventBus _eventBus;

        private readonly Dictionary<string, HikCameraHardwareDriver> _driverMap = new Dictionary<string, HikCameraHardwareDriver>();
        private readonly object _lockObj = new object();
        public CameraApplicationoService(
            ICameraDeviceService cameraDomainService,
            IDeviceManager cameraDeviceManager,
            ICameraHardwareDriver driver,
            IEventBus eventBus
            )
        {
            _cameraDomainService = cameraDomainService;
            _deviceManager = cameraDeviceManager;
            _driver = driver;
            _eventBus = eventBus;
        }

        public event EventHandler<CameraFrameEventArgs> FrameReceived;
        public async Task<OperationResult<List<CameraDeviceDto>>> SearchOnlineCamera()
        {
            try
            {
                var list = await _driver.ScanAllCameraAsync();
                var dto= list.ToCameraDeviceDtoList(); // 扩展方法：扫描结果转换为Dto列表
                return OperationResult<List<CameraDeviceDto>>.Succes(dto);
            }
            catch (Exception ex)
            {
                LogHelper.Error("搜索在线相机异常", ex);
                return OperationResult<List<CameraDeviceDto>>.Fail($"搜索失败：{ex.Message}");
            }
        }
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

                // 2. 扫描Dto → 持久化实体（调用映射扩展方法）
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

        public async Task<OperationResult> ConnectCamera(string deviceId)
        {
            // 1. 基础校验：取设备实体
            var device = _deviceManager.GetDeviceById(deviceId) as CameraEntity;
            if (device == null)
                return OperationResult.Fail("设备不存在");

            // 2. 业务规则校验：交给领域服务判断能不能连
            var validateResult = _cameraDomainService.ValidateCanConnect(device);
            if (!validateResult.Success)
                return validateResult;

            // 3. 组装硬件参数，调用驱动执行连接
            var connectConfig = new CameraConnectConfig
            {
                SerialNumber = device.CameraAllConfig.ConnectConfig.SerialNumber,
                IpAddress = device.IpAddress,
                InterfaceType = device.CameraAllConfig.ConnectConfig.InterfaceType
            };
            var connectResult = await _driver.LoginAsync(connectConfig);
            if (connectResult.errorCode!=0)
                return OperationResult.Fail($"错误码{connectResult.errorCode}");

            // 4. 更新领域对象状态：交给领域服务处理状态流转
            _cameraDomainService.ApplyConnectionStatus(device, true);

            // 5. 发布全局事件
            _eventBus.Publish(new DeviceConnectionChangedEveent(deviceId, true));

            return OperationResult.Succes();
        }

        // 在线设备直连同理：先校验，再调驱动，再更新上下文/状态
        public async Task<OperationResult> ConnectOnlineCamera(CameraConnectConfig config)
        {
            // 在线设备没有实体，跳过实体校验，直接调驱动
            var result = await _driver.LoginAsync(config);
            if (result.errorCode==0)
            {
                _eventBus.Publish(new DeviceConnectionChangedEveent(config.SerialNumber, true));
            }
            return OperationResult.Succes();
        }
        /* public async Task<OperationResult> TestConnectAsync(CameraConnectConfig connectConfig)
         {
             // 1. 前置参数校验
             if (connectConfig == null)
                 return OperationResult.Fail("连接参数不能为空");

             bool hasSerial = !string.IsNullOrWhiteSpace(connectConfig.SerialNumber);
             bool hasIp = !string.IsNullOrWhiteSpace(connectConfig.IpAddress);
             if (!hasSerial && !hasIp)
                 return OperationResult.Fail("序列号和IP不能同时为空");

             HikCameraHardwareDriver tempDriver = null;
             MyCamera cameraObj = null;

             try
             {
                 // 2. 创建临时驱动实例，仅用于本次测试
                 tempDriver = new HikCameraHardwareDriver();

                 // 3. 执行登录测试
                 var (errorCode, cam) = await tempDriver.LoginAsync(connectConfig);
                 if (errorCode != 0)
                 {
                     return OperationResult.Fail($"连接测试失败，错误码：{errorCode}");
                 }

                 cameraObj = cam;
                 // 登录成功即代表连通性正常
                 return OperationResult.Succes();
             }
             catch (Exception ex)
             {
                 return OperationResult.Fail($"连接测试异常：{ex.Message}");
             }
             finally
             {
                 // 4. 无论成功失败，必须完整释放资源，绝对不残留连接
                 if (cameraObj != null && tempDriver != null)
                 {
                     try
                     {
                         tempDriver.Logout(cameraObj);
                     }
                     catch
                     {
                         // 忽略释放阶段的异常，保证资源回收不中断
                     }
                 }
             }
         }
         public async Task<OperationResult> ConnectCamera(string deviceId)
         {
             var camera = GetCameraEntity(deviceId);
             if (camera == null)
                 return OperationResult.Fail("设备不存在或不是相机类型");

             // 前置状态校验：仅未连接状态可执行连接
             if (camera.DetailStatus != CameraStatus.Disconnected)
                 return OperationResult.Fail($"当前状态[{camera.DetailStatus}]，无法执行连接");

             try
             {
                 // 创建该相机专属的驱动实例
                 var driver = new HikCameraHardwareDriver();

                 // 调用底层异步登录
                 var (errorCode, cameraObj) = await driver.LoginAsync(camera.CameraAllConfig.ConnectConfig);
                 if (errorCode != 0)
                 {
                     return OperationResult.Fail($"相机连接失败，错误码：{errorCode}");
                 }
                 else
                 {
                     _driverMap[deviceId] = driver;
                 }

                 // 订阅该相机的帧事件
                 driver.FrameReceived += (sender, e) =>
                 {
                     // 包装为带设备ID的事件参数，向上透传
                     FrameReceived?.Invoke(this, new CameraFrameEventArgs(
                         deviceId, e.Frame, e.Width, e.Height, e.FrameId, e.Timestamp));
                 };

                 // 写入实体：缓存驱动实例、相机句柄、更新状态
                 lock (_lockObj)
                 {
                     _driverMap[deviceId] = driver;
                     camera.CameraHandle = cameraObj;
                     camera.ConnectionStatus = DeviceConnectionStatue.Connected;
                     camera.DetailStatus = CameraStatus.Connected;
                 }

                 return OperationResult.Succes();
             }
             catch (Exception ex)
             {
                 // 异常兜底：强制回滚状态，避免半连接
                 lock (_lockObj)
                 {
                     if (_driverMap.ContainsKey(deviceId))
                     {
                         _driverMap.Remove(deviceId);
                     }
                     camera.CameraHandle = null;
                     camera.ConnectionStatus = DeviceConnectionStatue.Disconnected;
                     camera.DetailStatus = CameraStatus.Disconnected;
                 }
                 return OperationResult.Fail($"相机连接异常：{ex.Message}");
             }
         }*/
        private CameraEntity GetCameraEntity(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return null;
            return _deviceManager.GetDeviceById(deviceId) as CameraEntity;
        }
        public OperationResult DisconnectCamera(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            // 幂等处理：已断开直接返回
            if (camera.CameraHandle == null || !_driverMap.ContainsKey(deviceId))
            {
                camera.ConnectionStatus = DeviceConnectionStatue.Disconnected;
                camera.DetailStatus = CameraStatus.Disconnected;
                return OperationResult.Succes();
            }

            try
            {
                var driver = _driverMap[deviceId];

                // 采流中先强制停流，再断开（状态+硬件双重兜底）
                if (camera.DetailStatus == CameraStatus.Streaming)
                {
                    StopStreamInternal(deviceId, driver, camera);
                }

                // 底层完整登出：关流→关设备→销毁句柄
                int logoutCode = driver.Logout(camera.CameraHandle);

                // 清理资源与状态
                lock (_lockObj)
                {
                    _driverMap.Remove(deviceId);
                    camera.CameraHandle = null;
                    camera.ConnectionStatus = DeviceConnectionStatue.Disconnected;
                    camera.DetailStatus = CameraStatus.Idle;
                }

                return logoutCode == 0
                    ? OperationResult.Succes()
                    : OperationResult.Fail($"相机断开异常，错误码：{logoutCode}");
            }
            catch (Exception ex)
            {
                // 异常兜底：强制清理状态，避免卡死
                lock (_lockObj)
                {
                    _driverMap.Remove(deviceId);
                    camera.CameraHandle = null;
                    camera.ConnectionStatus = DeviceConnectionStatue.Disconnected;
                    camera.DetailStatus = CameraStatus.Disconnected;
                }
                return OperationResult.Fail($"相机断开异常：{ex.Message}");
            }
        }
        public async Task<OperationResult> StartStream(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            if (camera.CameraHandle == null || !_driverMap.TryGetValue(deviceId, out var driver))
                return OperationResult.Fail("设备未连接，无法开启采流");

            // 幂等：已在采流直接返回成功
            if (camera.DetailStatus == CameraStatus.Streaming)
                return OperationResult.Succes();

            try
            {
                int ret = await Task.Run(() => driver.OpenStream(camera.CameraHandle));
                if (ret != 0)
                    return OperationResult.Fail($"开启采流失败，错误码：{ret}");

                camera.DetailStatus = CameraStatus.Streaming;
                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"开启采流异常：{ex.Message}");
            }
        }
        public OperationResult StopStream(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            if (!_driverMap.TryGetValue(deviceId, out var driver))
                return OperationResult.Fail("设备未连接");

            return StopStreamInternal(deviceId, driver, camera);
        }
        private OperationResult StopStreamInternal(string deviceId, HikCameraHardwareDriver driver, CameraEntity camera)
        {
            try
            {
                int ret = driver.CloseStream(camera.CameraHandle);
                camera.DetailStatus = CameraStatus.Connected;

                return ret == 0
                    ? OperationResult.Succes()
                    : OperationResult.Fail($"停止采流失败，错误码：{ret}");
            }
            catch (Exception ex)
            {
                // 异常也强制更新状态，避免UI卡死
                camera.DetailStatus = CameraStatus.Connected;
                return OperationResult.Fail($"停止采流异常：{ex.Message}");
            }
        }
        public void ReleaseAll()
        {
            lock (_lockObj)
            {
                foreach (var deviceId in _driverMap.Keys.ToList())
                {
                    try
                    {
                        DisconnectCamera(deviceId);
                    }
                    catch
                    {
                        // 忽略单台释放异常，保证全部执行
                    }
                }
                _driverMap.Clear();
            }
        }
    }
}
