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

namespace MultiSerVIsion.Solution.Application.Services
{
    public interface ICameraAppService
    {
        Task<OperationResult<List<CameraDeviceDto>>> SearchOnlineCamera();
        OperationResult<DeviceEntity> AddScannedCameraToConfig(CameraDeviceDto scannedCamera);
        /*  OperationResult Selectcamera(CameraDeviceDto cameraUiDto, CameraConnectConfig LoginParam);
          OperationResult ConnectCamera();
          OperationResult OpenCameraStream();
          OperationResult CloseCameraStream();
          OperationResult DisconnectCamera();
          CameraStatus GetCurrentCameraStatus();
  */
    }
}
