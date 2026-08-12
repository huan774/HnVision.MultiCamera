using MultiSerVIsion.Solution.Application;
using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Presentation.Views;
using MultiSerVIsion.Solution.Presentation.Winforms;
using MultiSerVIsion.Solution.Shared.Exceptions;
using MultiSerVIsion.Solution.Shared.Helpers;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
/*using System.Drawing;*/
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Presentation.Presenter
{
    public class DeviceTressPresenter:BasePresenter
    {
      
        private readonly IDeviceTreeView _view;
        private readonly IDeviceAppService _appService;
        private readonly ICameraAppService _cameraAppService;
        private readonly IEventBus _eventBus;
        private readonly IDeviceContext _deviceContext;
        private readonly IDeviceManager _deviceManager;

        public DeviceTressPresenter(
            IEventBus eventBus,
            IDeviceTreeView view,
            IDeviceAppService appService,
            IDeviceContext deviceContext,
            IDeviceManager deviceManager,
            ICameraAppService cameraAppService)
        {
            _view = view;
            _eventBus=eventBus;
            _appService = appService;
            _deviceContext = deviceContext;
            _deviceManager = deviceManager;
            _cameraAppService = cameraAppService;


            _appService.LoadProject();
           /* LoadAllDeviceOnStart();*/
            
        }
        public override void Init()
        {
          /*  _view.DeviceNodeSelected += OnDeviceSelected;*/
            _view.ConfigDeviceSelected += OnConfigDeviceSelected;
            _view.NoDeviceSelected += OnNoDeviceSelected;
            _view.OnlineDeviceSelected += OnOnlineDeviceSelected;

            _view.AddDeviceRequest += OnAddDeviceRequest;
            _view.RemoveDeviceRequest += OnRemoveDeviceRequest;
            _view.CopyDeviceRequest += OnCopyDeviceRequest;
            _view.ToggleDeviceEnableRequest += OnToggleDeviceEnableRequest;
            _view.ViewLoaded += OnViewLoaded;
            _view.AddToConfigRequsted += () => AddSelectedCameraToConfig();
            _view.RefreshSearchRequested += async () => await SearchOnlineCameras();
            // 初始化加载设备树
           /* RefreshTree();*/
        }
        public async void OnViewLoaded()
        {
            // 先加载本地已组态设备
            LoadAllDeviceOnStart();
            // 自动搜索局域网在线相机
            await SearchOnlineCameras();
        }
        private void OnOnlineDeviceSelected(CameraDeviceDto dto)
        {
            // 1. 先更新全局上下文（状态落地）
            _deviceContext.SetOnlineCamera(dto);
            // 2. 发布事件，通知所有订阅者
            _eventBus.Publish(new OnlineCameraSelectedEvent(dto));
          
        }

        private void OnConfigDeviceSelected(string deviceId)
        {
            var device = _deviceManager.GetDeviceById(deviceId) as CameraEntity;
            if (device == null) return;

            _deviceContext.SetConfigDevice(deviceId, device);
            // 发布组态设备选中事件
            _eventBus.Publish(new ConfigDeviceSelectedEvent(deviceId));
        }

        private void OnNoDeviceSelected()
        {
            _deviceContext.ClearSelection();
            // 发布取消选中事件
            _eventBus.Publish(new DeviceSelectionClearedEvent());
        }
    
        public async Task SearchOnlineCameras()
        {
         /*   _view.SetSearchButtonEnabled(false);*/
            try
            {
                var result = await _cameraAppService.SearchOnlineCamera();
                if (!result.Success)
                {
                    _view.ShowMessage($"搜索相机失败：{result.Message}");
                    return;
                }

                // 绑定到UI设备树「在线设备」节点
                _view.BindOnlineCameraTree(result.Data);
                _view.ShowMessage($"搜索完成，共发现 {result.Data.Count} 台在线相机");
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"搜索异常：{ex.Message}");
            }
            finally
            {
              /*  _view.SetSearchButtonEnabled(true);*/
            }
        }

        public void AddSelectedCameraToConfig()
        {
            var selected = _view.SelectedOnlineCamera;
            if (selected == null)
            {
                _view.ShowMessage("请先选中一台在线相机");
                return;
            }

            var result = _cameraAppService.AddScannedCameraToConfig(selected);
            if (!result.Success)
            {
                _view.ShowMessage($"添加失败：{result.Message}");
                return;
            }

            _view.ShowMessage("添加成功");
            // 添加后刷新已组态设备树
            LoadAllDeviceOnStart();
        }
     /*   public void LoadConfigDevices()
        {
            var result = _appService.GetAllDevices();
            if (!result.Success)
            {
                _view.ShowMessage($"加载设备失败：{result.Message}");
                return;
            }
            _view.RefreshDeviceStatusIcon();
        }*/

        private void LoadAllDeviceOnStart()
        {
            try
            {
                var result = _appService.GetAllDevices();
                
                if (!result.Success || result.Data == null)
                {
                    _view.ShowMessage("设备数据加载失败：" + result.Message);
                    return;
                }
                foreach (var device in result.Data)
                {
                   _view.AddTreeNode(device.GroupTage, device.DeviceId, device.DeviceName);
                }
                _view.RefreshDeviceStatusIcon();
            }
            catch (Exception ex)
            {
                _view?.ShowMessage($"加载设备异常{ex.Message}");
            }
        }
        private void OnAddDeviceRequest()
        {
            string groupId = _view.GetRightClickGroupKey();
            if (string.IsNullOrEmpty(groupId))
            {
                MessageBox.Show("请右键左侧分组后再添加设备");
                return;
            }
            using (FrmAddDevice frm = new FrmAddDevice())
            {
                try
                {
                    frm.TargetGroupId = groupId;


                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        DeviceCreateInput input = frm.GetInput();
                        OperationResult<DeviceEntity> res = _appService.CreateDevice(input);

                        if (res.Success && res.Data != null)
                        {
                            _view.AddTreeNode(input.GroupTag, res.Data.DeviceId, res.Data.DeviceName);
                            _view.RefreshDeviceStatusIcon();
                        }
                        else
                        {
                            _view.ShowMessage(res.Message);
                        }
                    }
                }
                catch (StorageIoException ex)
                {
                    _view.ShowMessage($"JSON写入失败：{ex.Message}");
                    LogHelper.Error("复制设备写入JSON失败", ex);
                }
                catch (Exception ex)
                {
                    _view.ShowMessage($"操作异常：{ex.Message}");
                    LogHelper.Error("复制设备未知异常", ex);
                }
            }
        }
        private void OnRemoveDeviceRequest(string deviceId)
        {
            DialogResult res = MessageBox.Show($"确认删除设备 {deviceId}? 删除后配置将丢失", "确认删除",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (res != DialogResult.Yes)
                return;

            var reSer=_appService.DeleteDevice(deviceId);
            if (reSer.Success)
            {
                _view.RemoveTreeNode(deviceId);
                _view.RefreshDeviceStatusIcon();
            }
            else
            {
                _view.ShowMessage(reSer.Message);
            }
            
        }
       
        private void OnCopyDeviceRequest(string sourceDevId)
        {
            if (!_view.ShowConfirmDialog("确认复制该设备？")) return;
            var res = _appService.CopyDevice(sourceDevId);
            if (res.Success && res.Data != null)
            {
                _view.AddTreeNode(res.GroupTag, res.Data.DeviceId, res.Data.DeviceName);
                _view.RefreshDeviceStatusIcon();
            }
            else
            {
                _view.ShowMessage(res.Message);
            }
        }
        private void OnToggleDeviceEnableRequest(string deviceId)
        {
            var res =_appService.ToggleDeviceEnable(deviceId);
            if (res.Success)
            {
                _view.RefreshDeviceStatusIcon();
            }
            else
            {
                _view.ShowMessage(res.Message);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _view.ConfigDeviceSelected -= OnConfigDeviceSelected;
                _view.NoDeviceSelected -= OnNoDeviceSelected;
                _view.OnlineDeviceSelected -= OnOnlineDeviceSelected;

                _view.AddDeviceRequest -= OnAddDeviceRequest;
                _view.RemoveDeviceRequest -= OnRemoveDeviceRequest;
                _view.CopyDeviceRequest -= OnCopyDeviceRequest;
                _view.ToggleDeviceEnableRequest -= OnToggleDeviceEnableRequest;
                _view.ViewLoaded -= OnViewLoaded;
                _view.AddToConfigRequsted -= () => AddSelectedCameraToConfig();
                _view.RefreshSearchRequested -= async () => await SearchOnlineCameras();
            }
            base.Dispose(disposing);
        }
    }
}
