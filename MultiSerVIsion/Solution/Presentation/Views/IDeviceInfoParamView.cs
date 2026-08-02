using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Views
{
    public interface IDeviceInfoParamView
    {
       /* void ShowDeviceBasicInfo(DeviceEntity deviceEntity, CameraDeviceDto info);*/
        void ClearDeviceInfo();
        void UpdateConnectStatus(CameraStatus status);
        void ShowOnlineCameraInfo(CameraDeviceDto dto);
        void ShowConfigCameraInfo(CameraEntity entity);
        void ShowMessage(string message);

        event Action OnConnectClicked;
        event Action OnDisconnectClicked;
        /*event Action<CameraUiDto> paramValueChanged;*/
        
    }
}
