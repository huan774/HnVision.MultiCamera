using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Application.Services
{
    public class CameraApplicationoService/*:ICameraAppService*/
    {
        private readonly ICameraDeviceService _cameraDeviceService;
        private readonly IDeviceManager _cameraDeviceManager;
        public CameraApplicationoService(ICameraDeviceService cameraDeviceService, IDeviceManager cameraDeviceManager)
        {
            _cameraDeviceService = cameraDeviceService;
            _cameraDeviceManager = cameraDeviceManager;
        }
        public async Task <List<CameraHardwareRawDto>> SearchCameraForUi()
        {
            List<CameraHardwareRawDto> domainList =await _cameraDeviceService.ScanAllCamerasAsync();
            return domainList.Select(d=>new CameraHardwareRawDto
            {
                IpAddress=d.IpAddress,
                SerialNumber=d.SerialNumber,
                ModelName=d.ModelName,
                InterfaceType=d.InterfaceType,
                
            }).ToList();
           
        }
        /*public OperationResult Selectcamera(CameraUiDto cameraUiDto, CameraConnectConfig LoginParam)
        {
            CameraDeviceDto domainDevice = new CameraDeviceDto(
                cameraUiDto.SerialNumber,cameraUiDto.ModelName,cameraUiDto.IpAddress,cameraUiDto.IntrefaceType
                
                );
            return _cameraDeviceService.SelectCameraDevice(domainDevice, LoginParam);
            
        }
        public OperationResult ConnectCamera()
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
