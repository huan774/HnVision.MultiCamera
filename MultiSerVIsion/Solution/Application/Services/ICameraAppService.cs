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
    /// <summary>
    /// 相机应用服务接口：编排扫描、组态、连接、断开、采流等用例。
    /// </summary>
    public interface ICameraAppService
    {
        /// <summary>相机帧到达事件：驱动采集到帧后由应用服务向上层透传（订阅方负责释放 Bitmap）</summary>
        event EventHandler<CameraFrameEventArgs> FrameReceived;

        /// <summary>搜索所有在线相机</summary>
        Task<OperationResult<List<CameraDeviceDto>>> SearchOnlineCamera();

        /// <summary>将扫描到的在线相机导入组态</summary>
        OperationResult<DeviceEntity> AddScannedCameraToConfig(CameraDeviceDto scannedCamera);

        /// <summary>测试连接（短连接，登录成功后立即释放，不占用句柄）</summary>
        Task<OperationResult> TestConnectAsync(CameraConnectConfig connectConfig);

        /// <summary>在线相机直连（长连接，会占用句柄）</summary>
        Task<OperationResult> ConnectOnlineCamera(CameraConnectConfig config);

        /// <summary>连接已组态相机（长连接）</summary>
        Task<OperationResult> ConnectCamera(string deviceId);

        /// <summary>断开相机</summary>
        OperationResult DisconnectCamera(string deviceId);

        /// <summary>开启连续采流</summary>
        Task<OperationResult> StartStream(string deviceId);

        /// <summary>停止采流</summary>
        OperationResult StopStream(string deviceId);
    }
}
