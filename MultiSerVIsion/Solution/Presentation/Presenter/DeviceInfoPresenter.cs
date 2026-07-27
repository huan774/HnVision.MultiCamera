using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Entities;
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

        public DeviceInfoPresenter(IDeviceInfoParamView view, ICameraAppService cameraAppService)
        {
            _view = view;
            _cameraAppService = cameraAppService;

            _view.OnConnectClicked += View_OnConnectClicked;
            _view.OnDisconnectClicked += View_OnDisconnectClicked;
        }
        public async Task LoadDeviceInfoAsync(CameraHardwareRawDto selectedCamera)
        {
            if (selectedCamera == null)
            {
                _view.ClearDeviceInfo();
                return;
            }

          /*  var uiModel = new DeviceEntity
            {
                DeviceType = "工业相机",
                DeviceName = selectedCamera.SerialNumber,

            };*/
            /*   _view.ShowDeviceBasicInfo(uiModel);*/

            var status = _cameraAppService.GetCurrentCameraStatus();
            _view.UpdateConnectStatus(status);

        }
        public void View_OnConnectClicked()
        {
            try
            {
                var result = _cameraAppService.ConnectCamera();
                if (result.Success)
                {
                    _view.UpdateConnectStatus(Domain.Enums.CameraStatus.Connected);
                }
                else
                {
                    _view.ShowMessage($"连接失败：{result.Message}");
                }

            }
            catch (Exception ex)
            {
                _view.ShowMessage($"Failed to connect {ex.Message}");
            }
        }

        private void View_OnDisconnectClicked()
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
        }
    }
    
}
