using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Contexts
{
    public interface IDeviceContext
    {
        string CurrentDeviceId { get; }
        CameraDeviceDto CurrentOnlineCamera { get; }
        bool IsCurrentDeviceConnected { get; }

        // 设置选中状态
        void SetOnlineCamera(CameraDeviceDto dto);
        void SetConfigDevice(string deviceId, CameraEntity entity);
        void ClearSelection();

        // 更新连接状态
        void UpdateConnectionStatus(string deviceId, bool isConnected);
    }
}
