using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Presenter
{
    public class DeviceInfoPresenter
    {
        private readonly IDeviceInfoParamView _view;
        private readonly ICameraAppService _cameraAppService;
        private readonly IDeviceManager _manager;

        public DeviceInfoPresenter(IDeviceInfoParamView view,IDeviceManager manager, ICameraAppService cameraAppService)
        {
            _view = view;
            _cameraAppService = cameraAppService;
            _manager = manager;

          /*  _view.OnConnectClicked += View_OnConnectClicked;
            _view.OnDisconnectClicked += View_OnDisconnectClicked;*/

            
        }
        public async Task LoadDeviceInfoAsync(CameraDeviceDto selectedCamera)
        {
            Clear();
            if (selectedCamera == null)
            {
                _view.ClearDeviceInfo();
                return;
            }
            _view.ShowOnlineCameraInfo(selectedCamera);
        }

        public void LoadConfigCamera(string deviceId)
        {
            Clear();
            var device = _manager.GetDeviceById(deviceId) as CameraEntity;
            if (device == null)
            {
                _view.ClearDeviceInfo();
                return;
            }

            _view.ShowConfigCameraInfo(device);
            // 已组态设备：未连接可连接，已连接可打开参数配置
        }
        public void Clear()
        {
            _view.ClearDeviceInfo();
        }

        /* private void View_OnDisconnectClicked()
         {
             try
             {
                 var result = _cameraAppService.DisconnectCamera();
                 if (result.Success)
                 {
                     _view.UpdateConnectStatus(Domain.Enums.CameraStatus.Disconnected);

                 }
                 else
                 {
                     _view.ShowMessage($"断开连接：{result.Message}");
                 }
             }
             catch (Exception ex)
             {
                 _view.ShowMessage($"异常：{ex.Message}");
             }
         }*/
    }
}
