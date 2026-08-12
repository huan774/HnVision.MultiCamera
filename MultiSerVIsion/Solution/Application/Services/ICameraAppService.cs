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
using static MultiSerVIsion.Solution.Infrastructure.HiKHardware.HikCameraHardwareDriver;

namespace MultiSerVIsion.Solution.Application.Services
{
    public interface ICameraAppService
    {
        event EventHandler<CameraFrameEventArgs> FrameReceived;
        Task<OperationResult<List<CameraDeviceDto>>> SearchOnlineCamera();
        OperationResult<DeviceEntity> AddScannedCameraToConfig(CameraDeviceDto scannedCamera);
        //测试连接
        /* Task<OperationResult> TestConnectAsync(CameraConnectConfig connectConfig);*/
        Task<OperationResult> ConnectOnlineCamera(CameraConnectConfig config);
        /// 连接相机
        Task<OperationResult> ConnectCamera(string deviceId);
        /// 断开相机
        OperationResult DisconnectCamera(string deviceId);
        /// 开启连续采流
        Task<OperationResult> StartStream(string deviceId);
        /// 停止采流
        OperationResult StopStream(string deviceId);
        /// 获取相机基础信息（供信息面板展示）
       /* OperationResult<CameraEntity> GetCameraInfo(string deviceId);*/
  
    }
}
