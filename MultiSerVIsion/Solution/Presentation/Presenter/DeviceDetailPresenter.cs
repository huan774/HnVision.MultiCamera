using MultiSerVIsion.Solution.Application;
using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Presentation.Presenter.Factory;
using MultiSerVIsion.Solution.Presentation.Views;
using MvCameraControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MultiSerVIsion.Solution.Presentation.Presenter
{
    public class DeviceDetailPresenter
    {
        
        private IDeviceDatailView _datailView;
        private readonly IDeviceAppService _appService;
        private string _currentEditDevId = string.Empty;

        private readonly Dictionary<string, IDeviceDatailView> _viewCache = new Dictionary<string, IDeviceDatailView>();

        public event Action<System.Windows.Forms.UserControl> OnCreaateDetailUc;
        public event Action OnClearDetail;
        
        public DeviceDetailPresenter(IDeviceAppService appService)
        {
            _appService = appService;
        }
        public void LoadDevice(string devId)
        {
             var res = _appService.GetDeviceById(devId);
             if (!res.Success|| res.Data == null)
             {
                 _datailView.ShowMessage("设备加载失败:"+res.Message);
              
                 ClearEdit();
                 return ;
             }
             var dev=res.Data;
            string typeKey = GetGroupTypeKey(dev.GroupTage);

            UnBindViewEvent();

            if (!_viewCache.TryGetValue(typeKey, out IDeviceDatailView newView))
            {
                newView = DeviceDetailView.Create(dev.GroupTage);
                _viewCache.Add(typeKey, newView);
            }

            var uc = newView as System.Windows.Forms.UserControl;

           BindViewEvent(newView);
            _datailView = newView;
            _currentEditDevId=devId;

            OnCreaateDetailUc?.Invoke(uc);
            _datailView.LoadDeviceData(dev);
        }

        private void BindViewEvent(IDeviceDatailView view)
        {
            view.SaveConfigRequest += OnSaveConfig;
        }
        private void UnBindViewEvent()
        {
            if (_currentEditDevId == null || _datailView==null) return;
            _datailView.SaveConfigRequest -= OnSaveConfig;
        }
       
        public void ClearEdit()
        {
            _currentEditDevId = string.Empty;
            if(_datailView==null) return;
            _datailView.ClearPanel();
        }
        private void OnSaveConfig()
        {
            if (string.IsNullOrEmpty(_currentEditDevId))
            {
                _datailView.ShowMessage("未选中设备，无法保存");
                return;
            }
            if (!_datailView.ShowConfirmDialog("确认保存当前设备？")) return;
           
            var editData = _datailView.GetEditInput();

            DeviceUpdateIput input = new DeviceUpdateIput
            {
                DeviceId = _currentEditDevId,
                DeviceName = editData.DeviceName,
                GroupTag=editData.GroupTage,
                IpAddress = editData.IpAddress,
                DeviceType = editData.DeviceType,
                IsEnable = editData.IsEnable,

                
               /* CameraConfig = editData.CameraConfig,
                MotionCardConfig=editData.MotionConfig*/
            };

            editData.DeviceId = _currentEditDevId;
            var saveResult=_appService.UpdataDevice(input);
            if (saveResult.Success)
            {
                _datailView.ShowMessage("保存成功");
            }
            else
            {
                _datailView.ShowMessage(saveResult.Message);
            }
        }
        public void OnClearEdit()
        {
            _datailView.ClearPanel();
        }
        public void ClearDetailPanel()
        {
            UnBindViewEvent();
            _currentEditDevId= string.Empty;
            _datailView = null;
            OnClearDetail?.Invoke();
        }
        private string GetGroupTypeKey(string groupTag)
        {
            /*if (groupTag.Contains("PLC")) return "PLC";*/
            if (groupTag.Contains("Camera")) return "Camera";
            if (groupTag.Contains("Card")) return "Card";
            return "Default";
        }
        public void DisposeAllCache()
        {
            foreach (var view in _viewCache.Values)
            {
                view.SaveConfigRequest -= OnSaveConfig;
                /*view.ClearPanel -= OnClearEdit;*/
            }
            _viewCache.Clear();
        }
    }
}
