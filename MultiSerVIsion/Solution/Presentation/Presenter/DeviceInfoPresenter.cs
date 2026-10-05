using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Infrastructure.Events;
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
    /// <summary>
    /// 设备信息面板 Presenter：展示当前选中设备信息，并处理连接/断开交互。
    /// 【职责分工】「当前选中设备 + 连接状态」统一存于 IDeviceContext（会话上下文，唯一状态源）；
    /// 相机领域状态存于 CameraEntity.DetailStatus（应用层维护）。本 Presenter 不持有私有状态副本，
    /// 避免多源状态不一致导致「连接成功却无法断开」的问题。
    /// </summary>
    public class DeviceInfoPresenter : BasePresenter
    {
        private readonly IDeviceInfoParamView _view;
        private readonly ICameraAppService _cameraAppService;
        private readonly IDeviceManager _manager;
        private readonly IEventBus _eventBus;
        private readonly IDeviceContext _deviceContext;

        public DeviceInfoPresenter(
            IDeviceInfoParamView view,
            IDeviceManager manager,
            IEventBus eventBus,
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
            Clear();
        }

        public async Task LoadDeviceInfoAsync(CameraDeviceDto selectedCamera)
        {
            // 当前选中 = 在线相机（统一写入会话上下文，后续连接/断开都从 context 读取）
            _deviceContext.SetOnlineCamera(selectedCamera);

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

            // 当前选中 = 组态设备（统一写入会话上下文）
            _deviceContext.SetConfigDevice(deviceId, device);

            _view.ShowConfigCameraInfo(device);
            // 已组态设备：未连接可连接，已连接可打开参数配置
            _view.UpdateConnectStatus(device.DetailStatus);
        }

        public void Clear()
        {
            // 清空会话上下文选中（含连接状态），Presenter 不单独维护状态
            _deviceContext.ClearSelection();
            _view.ClearDeviceInfo();
        }

        /// <summary>
        /// 连接按钮点击：按选中设备类型严格分流，职责不得混用。
        /// 【在线设备】只做“测试连接”——登录成功后立即释放句柄（即连即断，不占用资源），
        ///             用于验证相机可达性；要正式连接必须先添加到组态。
        /// 【组态设备】执行“正式连接”——建立长连接并占用句柄，成功后更新会话状态并允许断开。
        /// </summary>
        private async void View_OnConnectClicked()
        {
            var onlineCamera = _deviceContext.CurrentOnlineCamera;

            // 分支1：在线扫描设备 → 测试连接（短连接，验证后立即释放，不建立正式连接）
            if (onlineCamera != null)
            {
                var testConfig = new CameraConnectConfig
                {
                    SerialNumber = onlineCamera.SerialNumber,
                    IpAddress = onlineCamera.IpAddress,
                    InterfaceType = onlineCamera.InterfaceType
                };

                var testResult = await _cameraAppService.TestConnectAsync(testConfig);
                _view.ShowMessage(testResult.Success
                    ? "测试连接成功（已自动释放）：该相机可正常连接，请添加到组态后建立正式连接"
                    : $"测试连接失败：{testResult.Message}");
                // 测试连接不改变连接状态、不发布连接事件，避免占用句柄导致组态无法连接
                return;
            }

            // 分支2：组态设备 → 正式连接（长连接，成功后允许断开）
            string deviceId = _deviceContext.CurrentDeviceId;
            if (string.IsNullOrEmpty(deviceId))
            {
                _view.ShowMessage("未选中有效设备，无法连接");
                return;
            }

            var result = await _cameraAppService.ConnectCamera(deviceId);
            if (result.Success)
            {
                // 会话上下文为连接状态唯一来源；连接事件由应用服务统一发布，此处不再重复发布
                _deviceContext.UpdateConnectionStatus(deviceId, true);
                _view.UpdateConnectStatus(CameraStatus.Connected);
                _view.ShowMessage("设备已连接");
            }
            else
            {
                _view.ShowMessage($"连接失败：{result.Message}");
            }
        }

        /// <summary>
        /// 断开按钮点击：仅对“已正式连接的组态设备”生效。
        /// 【说明】在线设备只做测试连接（即连即断），不会进入已连接态，因此无需也无法断开。
        /// </summary>
        private void View_OnDisconnectClicked()
        {
            string deviceId = _deviceContext.CurrentDeviceId;

            // 仅已连接的组态设备可执行断开
            if (string.IsNullOrWhiteSpace(deviceId)
                || _deviceContext.CurrentOnlineCamera != null
                || !_deviceContext.IsCurrentDeviceConnected)
            {
                _view.ShowMessage("当前状态下不可执行断开操作");
                return;
            }

            try
            {
                var result = _cameraAppService.DisconnectCamera(deviceId);
                if (result.Success)
                {
                    // 同步会话上下文连接状态为未连接
                    _deviceContext.UpdateConnectionStatus(deviceId, false);
                    _view.UpdateConnectStatus(CameraStatus.Disconnected);
                    _view.ShowMessage("设备已断开");
                }
                else
                {
                    _view.ShowMessage($"断开失败：{result.Message}");
                }
            }
            catch (Exception ex)
            {
                // 异常兜底：强制将上下文置为未连接，避免 UI 卡死
                _deviceContext.UpdateConnectionStatus(deviceId, false);
                _view.UpdateConnectStatus(CameraStatus.Disconnected);
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
