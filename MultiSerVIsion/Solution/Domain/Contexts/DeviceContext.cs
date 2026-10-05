using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Contexts
{
    public class DeviceContext:IDeviceContext
    {
        public string CurrentDeviceId { get; private set; }
        public CameraDeviceDto CurrentOnlineCamera { get; private set; }
        public bool IsCurrentDeviceConnected { get; private set; }

        public void SetOnlineCamera(CameraDeviceDto dto)
        {
            CurrentOnlineCamera = dto;
            if (dto == null)
            {
                CurrentDeviceId = null;
                IsCurrentDeviceConnected = false;
                return;
            }
            // 在线设备无实体ID，用序列号作为会话标识（与驱动按序列号管理一致）
            CurrentDeviceId = string.IsNullOrWhiteSpace(dto.SerialNumber) ? dto.Model : dto.SerialNumber;
            IsCurrentDeviceConnected = false; // 选中默认未连接
        }

        public void SetConfigDevice(string deviceId, CameraEntity entity)
        {
            CurrentDeviceId = deviceId;
            CurrentOnlineCamera = null;
            // Connected/Streaming 均视为已连接，避免采流中无法断开
            IsCurrentDeviceConnected = entity != null
                && (entity.DetailStatus == CameraStatus.Connected
                    || entity.DetailStatus == CameraStatus.Streaming);
        }

        public void ClearSelection()
        {
            CurrentDeviceId = null;
            CurrentOnlineCamera = null;
            IsCurrentDeviceConnected = false;
        }

        public void UpdateConnectionStatus(string deviceId, bool isConnected)
        {
            // 只更新当前选中设备的状态，非当前设备不处理
            if (CurrentDeviceId == deviceId)
            {
                IsCurrentDeviceConnected = isConnected;
            }
        }
    }
}
