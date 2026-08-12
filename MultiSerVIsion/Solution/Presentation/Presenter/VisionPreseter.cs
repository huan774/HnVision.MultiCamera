using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
/*using System.Drawing;*/
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Infrastructure.HiKHardware.HikCameraHardwareDriver;

namespace MultiSerVIsion.Solution.Presentation.Presenter
{
    public class VisionPreseter : BasePresenter
    {
        private readonly IVisionView _view;
        private readonly ICameraAppService _cameraAppService;
        private readonly IEventBus _eventBus;
        private readonly IDeviceContext _deviceContext;

        private string _currentStreamingDeviceId;

        private int _frameCount = 0;
        private DateTime _lastFpsUpdate = DateTime.Now;
        private double _currentFps = 0;

        public VisionPreseter(
            IVisionView view,
            IEventBus eventBus,
            IDeviceContext deviceContext,
            ICameraAppService cameraAppService)
        {
            _view = view;
            _eventBus = eventBus;
            _deviceContext=deviceContext;
            _cameraAppService = cameraAppService;

            // 订阅自己关心的事件
            _eventBus.Subscribe<OnlineCameraSelectedEvent>(OnOnlineCameraSelected);
            _eventBus.Subscribe<ConfigDeviceSelectedEvent>(OnConfigDeviceSelected);
            _eventBus.Subscribe<DeviceSelectionClearedEvent>(OnSelectionCleared);
        }
        public override void Init()
        {
            /*            _view.StartGrabRequested += OnStartGrabRequested;
                        _view.StopGrabRequested += OnStopGrabRequested;*/
            /* _cameraAppService.FrameReceived += OnFrameReceived;*/


            _view.StartGrabRequested += OnStartGrab;
            _view.StopGrabRequested += OnStopGrab;
        }
        // 事件响应：调用自身业务方法
        private async void OnOnlineCameraSelected(OnlineCameraSelectedEvent e)
        {
            SwitchDevice(e.CameraDto.Model);
        }

        private void OnConfigDeviceSelected(ConfigDeviceSelectedEvent e)
        {
            SwitchDevice(e.DeviceId);
        }

        private void OnSelectionCleared(DeviceSelectionClearedEvent e)
        {
            StopCurrentStream();
            _view.ClearDisplay(); 
        }
        // 切换设备：统一入口
        private void SwitchDevice(string newDeviceId)
        {
            // 同一个设备不重复处理
            if (_currentStreamingDeviceId == newDeviceId) return;

            // 1. 先停掉上一个设备的流
            StopCurrentStream();
            // 2. 清空画面
            _view.ClearDisplay();
            // 3. 如果设备已连接，直接开始取流
            if (_deviceContext.IsCurrentDeviceConnected)
            {
                StartStream(newDeviceId);
            }
        }
        private void StartStream(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return;

          /*  _cameraAppService.StartStream(deviceId, frame =>
            {
                _view.RenderFrame(frame); // 回调渲染画面
            });*/
          _cameraAppService.StartStream(deviceId);
            _currentStreamingDeviceId = deviceId;
          /*  _view.UpdateGrabStatus(true);*/
        }
        private void StopCurrentStream()
        {
            if (!string.IsNullOrEmpty(_currentStreamingDeviceId))
            {
                _cameraAppService.StopStream(_currentStreamingDeviceId);
                _currentStreamingDeviceId = null;
            }
        /*    _view.UpdateGrabStatus(false);*/
        }
        private void OnStartGrab()
        {
            var deviceId = _deviceContext.CurrentDeviceId;
            if (string.IsNullOrEmpty(deviceId) || !_deviceContext.IsCurrentDeviceConnected) return;
            StartStream(deviceId);
        }
        private void OnStopGrab()
        {
            StopCurrentStream();
        }
        // 页面切换时调用（主窗体Tab切换时触发）
        public void OnPageEnter()
        {
            // 进入页面：如果设备已连接且未取流，自动恢复
            if (_deviceContext.IsCurrentDeviceConnected
                && string.IsNullOrEmpty(_currentStreamingDeviceId))
            {
                StartStream(_deviceContext.CurrentDeviceId);
            }
        }

        /*  private async void OnStartGrabRequested()
          {
              _view.SetGrabButtonsEnabled(canStart: false, canStop: false);
              var deviceId = _deviceContext.CurrentDeviceId;
              var result = await _cameraAppService.StartStream(deviceId);
              if (!result.Success)
              {
                  _view.ShowMessage(result.Message, isError: true);
                  _view.SetGrabButtonsEnabled(canStart: true, canStop: false);
                  return;
              }

              _frameCount = 0;
              _lastFpsUpdate = DateTime.Now;
              _view.SetGrabButtonsEnabled(canStart: false, canStop: true);
          }
          // 动态切换设备，每次切设备调用一次
          public override void LoadDevice(string deviceId)
          {
              // 先停掉旧设备
              if (!string.IsNullOrEmpty(CurrentDeviceId))
              {
                  _cameraAppService.StopStream(CurrentDeviceId);
                  _view.ClearDisplay();
              }

              base.LoadDevice(deviceId);

              // 加载新设备参数、更新界面
              *//*            var deviceInfo = _cameraAppService.(deviceId);
                          _view.UpdateRunInfo(deviceInfo);*//*
          }
          private void OnStopGrabRequested()
          {
              _view.SetGrabButtonsEnabled(canStart: false, canStop: false);
              var deviceId = _deviceContext.CurrentDeviceId;
              var result = _cameraAppService.StopStream(deviceId);
              if (!result.Success)
              {
                  _view.ShowMessage(result.Message, isError: true);
              }

              _view.ClearDisplay();
              _view.SetGrabButtonsEnabled(canStart: true, canStop: false);
          }
          // 释放资源：取消事件订阅*/
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
               /* _view.StartGrabRequested -= OnStartGrabRequested;
                _view.StopGrabRequested -= OnStopGrabRequested;*/
            }
            base.Dispose(disposing);
        }
        /*  private void OnFrameReceived(object sender, CameraFrameEventArgs e)
          {
              // 只处理当前绑定设备的帧（多相机场景下过滤）
              if (sender is  CameraDomainService service || e.DeviceId != _deviceId)
                  return;

              // 帧率统计：每秒更新一次
              _frameCount++;
              if ((DateTime.Now - _lastFpsUpdate).TotalSeconds >= 1)
              {
                  _currentFps = _frameCount / (DateTime.Now - _lastFpsUpdate).TotalSeconds;
                  _frameCount = 0;
                  _lastFpsUpdate = DateTime.Now;

                  // 更新运行信息
                  string info = $"分辨率: {e.Width}×{e.Height} | 帧率: {_currentFps:F1} fps | 帧号: {e.FrameId}";
                  _view.UpdateRunInfo(info);
              }

              // 推送图像到视图
              _view.UpdateFrame(e.Frame);
          }*/
    }
}
