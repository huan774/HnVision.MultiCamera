using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Presentation.Views;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Presenter
{
    public class DeviceInfoPresenter:BasePresenter
    {
        private readonly IDeviceInfoParamView _view;
        private readonly ICameraAppService _cameraAppService;
        private readonly IDeviceManager _manager;
        private readonly IEventBus _eventBus;
        private readonly IDeviceContext _deviceContext;

        private CameraDeviceDto _currentOnlineDto;
        private string _currentConfigDeviceId;
        private CameraStatus _currentStatus = CameraStatus.Disconnected;

        public DeviceInfoPresenter(
            IDeviceInfoParamView view,
            IDeviceManager manager,
            IEventBus eventBus ,
            ICameraAppService cameraAppService,
            IDeviceContext deviceContext)
        {
            _view = view;
            _cameraAppService = cameraAppService;
            _manager = manager;
            _eventBus = eventBus;
            _deviceContext = deviceContext;


            // ===== 只在这里订阅事件，业务逻辑完全复用公开方法 =====
            _eventBus.Subscribe<OnlineCameraSelectedEvent>(OnOnlineCameraSelected);
            _eventBus.Subscribe<ConfigDeviceSelectedEvent>(OnConfigDeviceSelected);
            _eventBus.Subscribe<DeviceSelectionClearedEvent>(OnSelectionCleared);
/*
            // 订阅连接状态变更：连接成功自动取流
            _eventBus.Subscribe<DeviceConnectionChangedEvent>(OnConnectionChanged);*/

        }
        public override void Init()
        {
            _view.OnConnectClicked += View_OnConnectClicked;
            _view.OnDisconnectClicked += View_OnDisconnectClicked;

        }
        private async void OnOnlineCameraSelected(OnlineCameraSelectedEvent e)
        {
            // 直接调用公开方法，逻辑复用
            await LoadDeviceInfoAsync(e.CameraDto);
        }

        private void OnConfigDeviceSelected(ConfigDeviceSelectedEvent e)
        {
            // 直接调用公开方法，逻辑复用
            LoadConfigCamera(e.DeviceId);
        }

        private void OnSelectionCleared(DeviceSelectionClearedEvent e)
        {
           /* StopCurrentStream();
            _view.ClearDisplay();*/
           Clear();
        }

        public async Task LoadDeviceInfoAsync(CameraDeviceDto selectedCamera)
        {
          
            _currentOnlineDto = selectedCamera;
            _currentConfigDeviceId = null;

            if (selectedCamera == null)
            {
                _view.ClearDeviceInfo();
                return;
            }

            _view.ShowOnlineCameraInfo(selectedCamera);
            _view.UpdateConnectStatus(CameraStatus.Disconnected);
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
          /*  _currentOnlineDto = null;
            _currentConfigDeviceId = deviceId;*/
            _currentStatus = device.DetailStatus;

            _view.ShowConfigCameraInfo(device);
            // 已组态设备：未连接可连接，已连接可打开参数配置
            _view.UpdateConnectStatus(device.DetailStatus);
        }
        public void Clear()
        {
           /* _currentOnlineDto = null;
            _currentConfigDeviceId = null;*/
            _currentStatus = CameraStatus.Disconnected;
            _view.ClearDeviceInfo();
        }
        private async void View_OnConnectClicked()
        {
            OperationResult result;
            string currentDeviceId = _deviceContext.CurrentDeviceId;

            // 分支1：在线扫描设备 → 直连模式，不依赖实体库
            if (_currentOnlineDto != null)
            {
                var connectConfig = new CameraConnectConfig
                {
                    SerialNumber = _currentOnlineDto.SerialNumber,
                    IpAddress = _currentOnlineDto.IpAddress,
                    InterfaceType = _currentOnlineDto.InterfaceType
                };
                result = await _cameraAppService.ConnectOnlineCamera(connectConfig);
            }
            // 分支2：已组态设备 → 走实体库标准连接流程
            else if (!string.IsNullOrEmpty(_currentConfigDeviceId))
            {
                result = await _cameraAppService.ConnectCamera(_currentConfigDeviceId);
            }
            else
            {
                _view.ShowMessage("未选中有效设备，无法连接");
                return;
            }

            if (result.Success)
            {
                // 1. 更新全局上下文连接状态
                _deviceContext.UpdateConnectionStatus(currentDeviceId, true);
                // 2. 更新本地视图显示
                _view.UpdateConnectStatus(CameraStatus.Connected);
                // 3. 发布全局事件：通知视觉页、参数页等其他模块
                _eventBus.Publish(new DeviceConnectionChangedEveent(currentDeviceId, true));
            }
            else
            {
                _view.ShowMessage($"连接失败：{result.Message}");
            }
         
        }
        
        private void View_OnDisconnectClicked()
         {
            // 仅已连接的组态设备可执行断开
            if (string.IsNullOrWhiteSpace(_currentConfigDeviceId)
                || (_currentStatus != CameraStatus.Connected
                    && _currentStatus != CameraStatus.Streaming))
            {
                _view.ShowMessage("当前状态下不可执行断开操作");
                return;
            }

            try
            {
                var result = _cameraAppService.DisconnectCamera(_currentConfigDeviceId);
                if (result.Success)
                {
                    _currentStatus = CameraStatus.Idle;
                    _view.ShowMessage("设备已断开");
                }
                else
                {
                    _view.ShowMessage($"断开失败：{result.Message}");
                }

                _view.UpdateConnectStatus(_currentStatus);
          /*      LoadConfigCamera(_currentConfigDeviceId);*/
            }
            catch (Exception ex)
            {
                _currentStatus = CameraStatus.Disconnected;
                _view.UpdateConnectStatus(_currentStatus);
                _view.ShowMessage($"断开异常：{ex.Message}");
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放时必须取消订阅，避免内存泄漏
                _view.OnConnectClicked -= View_OnConnectClicked;
                _view.OnDisconnectClicked -= View_OnDisconnectClicked;

                _eventBus.Unsubscribe<OnlineCameraSelectedEvent>(OnOnlineCameraSelected);
                _eventBus.Unsubscribe<ConfigDeviceSelectedEvent>(OnConfigDeviceSelected);
                _eventBus.Unsubscribe<DeviceSelectionClearedEvent>(OnSelectionCleared);

            }
            base.Dispose(disposing);
        }
    }
}
