using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Infrastructure.HiKHardware.HikCameraHardwareDriver;

// System.Drawing 也定义了 IDeviceContext，用别名消除歧义，指向领域层会话上下文
using IDeviceContext = MultiSerVIsion.Solution.Domain.Contexts.IDeviceContext;

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

            // 订阅相机帧到达事件：帧在后台取流线程产生，通过事件回到 UI 层渲染
            _cameraAppService.FrameReceived += OnFrameReceived;

            
        }
        public override void Init()
        {
            _view.StartGrabRequested += OnStartGrab;
            _view.StopGrabRequested += OnStopGrab;
        }
        // 事件响应：调用自身业务方法
        private async void OnOnlineCameraSelected(OnlineCameraSelectedEvent e)
        {
            await SwitchDeviceAsync(e.CameraDto.Model);
        }

        private async void OnConfigDeviceSelected(ConfigDeviceSelectedEvent e)
        {
            await SwitchDeviceAsync(e.DeviceId);
        }

        private void OnSelectionCleared(DeviceSelectionClearedEvent e)
        {
            StopCurrentStream();
            _view.ClearDisplay(); 
        }
        // 切换设备：统一入口
        private async Task SwitchDeviceAsync(string newDeviceId)
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
                await StartStreamAsync(newDeviceId);
            }
        }
        /// <summary>
        /// 开启取流：必须等待应用服务返回结果，只有真正成功才记录当前取流设备。
        /// 【说明】原实现忽略返回值且无条件记录设备 ID，导致取流失败被静默吞掉，
        /// 且后续逻辑误判为已在取流，表现为连接成功却取不到流。
        /// </summary>
        private async Task StartStreamAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return;

            var result = await _cameraAppService.StartStream(deviceId);
            if (!result.Success)
            {
                _view.ShowMessage(result.Message);
                _currentStreamingDeviceId = null;
                // 取流失败：恢复按钮可用状态，避免限制卡死
                _view.SetGrabButtonsEnabled(canStart: true, canStop: false);
                return;
            }

            _currentStreamingDeviceId = deviceId;
            // 取流成功：禁用「开始」、启用「停止」
            _view.SetGrabButtonsEnabled(canStart: false, canStop: true);
            // 重置帧率统计，避免沿用上一次取流的旧值导致频率显示跳变
            _frameCount = 0;
            _lastFpsUpdate = DateTime.Now;
          
        }
        private void StopCurrentStream()
        {
            if (!string.IsNullOrEmpty(_currentStreamingDeviceId))
            {
                _cameraAppService.StopStream(_currentStreamingDeviceId);
                _currentStreamingDeviceId = null;
            }
            // 停止后恢复按钮限制：允许开始、禁止停止
            _view.SetGrabButtonsEnabled(canStart: true, canStop: false);
        }
        private async void OnStartGrab()
        {
            var deviceId = _deviceContext.CurrentDeviceId;
            if (string.IsNullOrEmpty(deviceId) || !_deviceContext.IsCurrentDeviceConnected) return;
            await StartStreamAsync(deviceId);
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
                _ = StartStreamAsync(_deviceContext.CurrentDeviceId);
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
        /// <summary>
        /// 帧到达回调：过滤非当前取流设备的帧，将统一帧转换为 Bitmap 后推送到视图。
        /// 【线程】由驱动取流线程触发，视图负责切回 UI 线程绘制。
        /// </summary>
        private void OnFrameReceived(object sender, CameraFrameEventArgs e)
        {
            if (e?.Frame == null) return;
            // 多相机场景下只渲染当前正在取流的设备，避免画面串流
            if (string.IsNullOrEmpty(_currentStreamingDeviceId)
                || e.DeviceId != _currentStreamingDeviceId) return;

            var bitmap = ToBitmap(e.Frame);
            if (bitmap == null) return;

            // 帧率统计：每秒刷新一次
            _frameCount++;
            var elapsed = (DateTime.Now - _lastFpsUpdate).TotalSeconds;
            if (elapsed >= 1)
            {
                _currentFps = _frameCount / elapsed;
                _view.UpdateRunInfo($"帧率: {_currentFps:F1} fps | 分辨率: {e.Frame.Width}×{e.Frame.Height}");
                _frameCount = 0;
                _lastFpsUpdate = DateTime.Now;
            }

            // 视图内部负责释放上一帧图像
            _view.UpdateFrame(bitmap);
        }

        /// <summary>
        /// 统一帧 → Bitmap：按像素格式逐行拷贝，避免 stride 与宽度不一致导致图像错位。
        /// </summary>
        /// <param name="frame">驱动输出的统一帧</param>
        /// <returns>可显示的位图；格式不支持或数据长度不足时返回 null，调用方应丢弃该帧</returns>
        private static Bitmap ToBitmap(CameraFrame frame)
        {
            if (frame?.Data == null || frame.Width <= 0 || frame.Height <= 0)
                return null;

            PixelFormat pixelFormat;
            int stride;
            switch (frame.PixelFormat)
            {
                case PixelFormatEnum.Mono8:
                    pixelFormat = PixelFormat.Format8bppIndexed;
                    stride = frame.Width;
                    break;
                case PixelFormatEnum.RGB24:
                case PixelFormatEnum.BGR24:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    stride = frame.Width * 3;
                    break;
                default:
                    // 不支持的格式直接丢弃，避免构造出错误图像
                    return null;
            }

            // 数据长度不足说明驱动转换失败并回退了原始数据，直接丢弃以免构造异常
            if (frame.Data.Length < stride * frame.Height) return null;

            var bitmap = new Bitmap(frame.Width, frame.Height, pixelFormat);
            if (pixelFormat == PixelFormat.Format8bppIndexed)
                ApplyGrayPalette(bitmap);

            var rect = new Rectangle(0, 0, frame.Width, frame.Height);
            var bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);
            try
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    Marshal.Copy(frame.Data, y * stride, bmpData.Scan0 + y * bmpData.Stride, stride);
                }
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
            return bitmap;
        }

        /// <summary>为 8bppIndexed 位图写入灰度调色板，否则图像显示为纯黑</summary>
        private static void ApplyGrayPalette(Bitmap bitmap)
        {
            var palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 取消订阅，避免事件持有 Presenter 导致内存泄漏
                _view.StartGrabRequested -= OnStartGrab;
                _view.StopGrabRequested -= OnStopGrab;
                _cameraAppService.FrameReceived -= OnFrameReceived;
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
