using MultiSerVIsion.Solution.Application.Device.Dtos;
using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using MultiSerVIsion.Solution.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.DevicePanel
{
    public interface IDeviceInfoParamView
    {
      
        void ClearDeviceInfo();
        void UpdateConnectStatus(CameraStatus status);
        void ShowOnlineCameraInfo(CameraDeviceDto dto);
        void ShowConfigCameraInfo(CameraEntity entity);
        void ShowMessage(string message);

        event Action OnConnectClicked;
        event Action OnDisconnectClicked;
       
        
    }
}
