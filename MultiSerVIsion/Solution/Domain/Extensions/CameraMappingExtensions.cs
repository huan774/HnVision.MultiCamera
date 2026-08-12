using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Factory;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Shared.Extensions
{
    public static class CameraMappingExtensions
    {
        public static CameraDeviceDto ToCameraDeviceDto(this CameraHardwareRawDto raw)
        {
            return new CameraDeviceDto
            {
                IpAddress = raw.IpAddress,
                SerialNumber = raw.SerialNumber,
                Model = raw.Model,
                InterfaceType = raw.InterfaceType,
                MacAddress = raw.MacAddress
                // 丢弃SdkDeviceKey底层字段，上层不需要
            };
        }
      
        public static List<CameraDeviceDto> ToCameraDeviceDtoList(this List<CameraHardwareRawDto> rawList)
        {
            return rawList.Select(x => x.ToCameraDeviceDto()).ToList();
        }

        /// <summary>
        /// 扫描到的在线相机 → 生成待新增设备实体（用户点击添加设备时调用）
        /// </summary>
        public static CameraEntity ToNewCameraEntity(this CameraDeviceDto scannedCamera)
        {
            var entity = DeviceFactory.CreateDevice("Camera") as CameraEntity;
            if (entity == null)
                throw new InvalidOperationException("创建设备实体失败，设备类型错误");

            entity.DeviceId = Guid.NewGuid().ToString("N");
            entity.DeviceName = scannedCamera.Model;
            entity.IpAddress = scannedCamera.IpAddress;
            entity.DeviceType = "Camera";
            entity.GroupTage = "Default";
            entity.IsEnable = false;

            // 填充相机连接参数
            entity.CameraAllConfig.ConnectConfig.SerialNumber = scannedCamera.SerialNumber;
           /* entity.CameraAllConfig.ConnectConfig.IpAddress = scannedCamera.IpAddress;*/
            entity.CameraAllConfig.ConnectConfig.InterfaceType = scannedCamera.InterfaceType;

            return entity;
        }
    }
}

