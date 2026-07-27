using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Services
{
    public interface ICameraDeviceService
    {
        Task<List<CameraHardwareRawDto>> ScanAllCamerasAsync();

        /// <summary>扫描后批量自动生成组态设备（不存在则新增，存在则跳过）</summary>
        Task<OperationResult<List<CameraEntity>>> AutoCreateFromScanAsync();

        /// <summary>连接指定相机</summary>
        OperationResult ConnectCamera(string deviceId);

        /// <summary>断开指定相机</summary>
        OperationResult DisconnectCamera(string deviceId);

        /// <summary>开启指定相机取流</summary>
        OperationResult OpenStream(string deviceId);

        /// <summary>停止指定相机取流</summary>
        OperationResult StopStream(string deviceId);

        /// <summary>设置相机曝光时间</summary>
       /* OperationResult SetExposure(string deviceId, double exposureTimeUs);

        /// <summary>从硬件读取实时运行状态</summary>
        OperationResult<CameraRuntimeStateDto> GetRuntimeState(string deviceId);*/
    }
}
