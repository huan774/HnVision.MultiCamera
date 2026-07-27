using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Services
{
    public class CameraDomainService : ICameraDeviceService
    {
        private readonly IDeviceManager _deviceManager;
        private readonly ICameraHardwareDriver _hardwareDriver;


        public CameraDomainService(IDeviceManager deviceManager, ICameraHardwareDriver hardwareDriver)
        {
            _deviceManager = deviceManager;
            _hardwareDriver = hardwareDriver;
        }
        public async Task<List<CameraHardwareRawDto>> ScanAllCamerasAsync()
        {
            return await _hardwareDriver.ScanAllCameraAsync();
        }

        public async Task<OperationResult<List<CameraEntity>>> AutoCreateFromScanAsync()
        {
            var scanResults = await ScanAllCamerasAsync();
            var createdList=new List<CameraEntity>();

            foreach (var scan in scanResults)
            {
                var exist = _deviceManager.GetDevices<CameraEntity>()
                    .FirstOrDefault(c => c.CameraAllConfig.ConnectConfig.SerialNumber == scan.SerialNumber);

                if (exist != null) continue;

                var newCamera = CameraEntity.CreateFormScanResult(scan);

                bool addSuccess = _deviceManager.AddDevice(newCamera);
                if (addSuccess)
                    createdList.Add(newCamera);
            }
            return OperationResult<List<CameraEntity>>.Succes(createdList);

        }
        public OperationResult ConnectCamera(string devId)
        {
            var device = _deviceManager.GetDeviceById(devId);

            if (!(device is CameraEntity camera))
                return OperationResult.Fail("设备不存在或不是相机类型");

            if (camera.DetailStatus != CameraStatus.Disconnected)
                    return OperationResult.Fail($"当前状态{camera.DetailStatus}");

                try
                {
                    int handle = _hardwareDriver.Login(camera.CameraAllConfig.ConnectConfig);

                    camera.Handle = handle;
                    camera.ConnectionStatus = DeviceConnectionStatue.Connected;
                    camera.DetailStatus = CameraStatus.Connected;

                    return OperationResult.Succes();
                }
                catch (Exception ex)
                {
                    return OperationResult.Fail($"相机连接失败：{ex.Message}");
                }
            

        }
        public OperationResult DisconnectCamera(string devId)
        {
            var device = _deviceManager.GetDeviceById(devId);

            if (!(device is CameraEntity camera))
                return OperationResult.Fail("设备不存在或不是相机类型");

            if (camera.Handle <= 0)
                {
                    camera.ConnectionStatus = DeviceConnectionStatue.Disconnected;
                    camera.DetailStatus = CameraStatus.Disconnected;
                    return OperationResult.Succes();
                }

                try
                {
                    if (camera.DetailStatus == CameraStatus.Streaming)
                        StopStream(devId);

                    _hardwareDriver.Logout(camera.Handle);

                    camera.Handle = -1;
                    camera.ConnectionStatus = DeviceConnectionStatue.Disconnected;
                    camera.DetailStatus = CameraStatus.Idle;

                    return OperationResult.Succes();
                }
                catch (Exception ex)
                {
                    return OperationResult.Fail($"相机断开失败：{ex.Message}");
                }
            
        }
        public OperationResult OpenStream(string devId) 
        {
            var device=_deviceManager.GetDeviceById(devId);
            if(!(device is CameraEntity camera))
                return OperationResult.Fail("设备不存在或不是相机类型");

            if (camera.ConnectionStatus != DeviceConnectionStatue.Connected)
                    return OperationResult.Fail("请先连接相机，在开始取流");

            try
            {
                _hardwareDriver.OpenStream(camera.Handle);
                camera.ConnectionStatus = DeviceConnectionStatue.Running;
                camera.DetailStatus = CameraStatus.Streaming;
                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Failed to open {ex.Message}");
            }
            
        }
        public OperationResult StopStream(string devId) 
        {
            var device= _deviceManager.GetDeviceById(devId);
            if (!(device is CameraEntity camera))
                return OperationResult.Fail("设备不存在或不是相机类型");

            if (camera.DetailStatus != CameraStatus.Streaming)
                return OperationResult.Fail("相机未在取流状态");

            try
            {
                _hardwareDriver.CloseStream(camera.Handle);
                camera.ConnectionStatus = DeviceConnectionStatue.Connected;
                camera.DetailStatus = CameraStatus.Connected;
                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"停止取流失败：{ex.Message}");
            }
        }
        /*public OperationResult SetExposure(string deviceId, double exposureTimeUs)
        {
            var device = _deviceManager.GetDeviceById(deviceId);
            if (device is not CameraDeviceEntity camera)
                return OperationResult.Fail("设备不存在或不是相机类型");

            try
            {
                // 先下发到硬件
                _hardwareDriver.SetExposure(camera.Handle, exposureTimeUs);
                // 再更新本地组态配置
                camera.CameraConfig.ParamConfig.ExposureTime = exposureTimeUs;
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"设置曝光失败：{ex.Message}");
            }
        }

        public OperationResult<CameraRuntimeStateDto> GetRuntimeState(string deviceId)
        {
            var device = _deviceManager.GetDeviceById(deviceId);
            if (device is not CameraDeviceEntity camera)
                return OperationResult<CameraRuntimeStateDto>.Fail("设备不存在");

            if (camera.ConnectionStatus == DeviceConnectionStatus.Disconnected)
                return OperationResult<CameraRuntimeStateDto>.Fail("相机未连接");

            try
            {
                var state = _hardwareDriver.GetRuntimeState(camera.Handle);
                return OperationResult<CameraRuntimeStateDto>.Success(state);
            }
            catch (Exception ex)
            {
                return OperationResult<CameraRuntimeStateDto>.Fail($"读取状态失败：{ex.Message}");
            }
        }
*/
    }
       
}
