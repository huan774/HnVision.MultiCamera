using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Shared.Extensions;
using MultiSerVIsion.Solution.Shared.Helpers;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Application.Services
{
    public class CameraApplicationoService:ICameraAppService
    {
        private readonly ICameraDeviceService _cameraDeviceService;
        private readonly IDeviceManager _deviceManager;
        public CameraApplicationoService(ICameraDeviceService cameraDeviceService, IDeviceManager cameraDeviceManager)
        {
            _cameraDeviceService = cameraDeviceService;
            _deviceManager = cameraDeviceManager;
        }
     
        public async Task<OperationResult<List<CameraDeviceDto>>> SearchOnlineCamera()
        {
            try
            {
                var list = await _cameraDeviceService.ScanAllCamerasAsync();
                return OperationResult<List<CameraDeviceDto>>.Succes(list);
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
        /*  public OperationResult Selectcamera(CameraDeviceDto cameraUiDto, CameraConnectConfig LoginParam)
          {
              CameraDeviceDto domainDevice = new CameraDeviceDto(
                *//*  cameraUiDto.SerialNumber,cameraUiDto.ModelName,cameraUiDto.IpAddress,cameraUiDto.InterfaceType*//*
                  );
              return _cameraDeviceService.SelectCameraDevice(domainDevice, LoginParam);

          }*/
        /* public OperationResult ConnectCamera()
         {
             return _cameraDeviceService.ConnectCurrentCamera();
         }
         public OperationResult OpenCameraStream()
         {
             return _cameraDeviceService.OpenCurrentCameraStream();
         }

         public OperationResult CloseCameraStream()
         {
             return _cameraDeviceService.CloseCurrentCameraStream();
         }

         public OperationResult DisconnectCamera()
         {
             return _cameraDeviceService.DisconnectCurrentCamera();
         }
         public CameraStatus GetCurrentCameraStatus()
         {
             return _cameraDeviceService.GetCurrentCameraStatus();
         }
 */
    }
}
