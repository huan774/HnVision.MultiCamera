using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Views
{
    public interface IDeviceTreeView
    {
        event Action ViewLoaded;
        event Action AddDeviceRequest;
        event Action AddToConfigRequsted;
        event Action RefreshSearchRequested;

        event Action<string> CopyDeviceRequest;
        event Action<string> RemoveDeviceRequest;
        event Action<string> ToggleDeviceEnableRequest;
        event Action<string> DeviceNodeSelected;

        event Action DeviceNodeUnSelect;

        event Action<string> ConfigDeviceSelected;
        event Action<CameraDeviceDto> OnlineDeviceSelected;
        event Action NoDeviceSelected;

        void AddTreeNode(string groupTag, string devId, string devName);
        void BindOnlineCameraTree(List<CameraDeviceDto> onlineCameras);
        void RefreshConfigDeviceTree(List<DeviceEntity> configDevices);
        CameraDeviceDto SelectedOnlineCamera {  get; }
        void RemoveTreeNode(string devId);
        void ClearSelectNode();
        void RefreshDeviceStatusIcon();

        bool ShowConfirmDialog(string message);
        void ShowMessage(string msg);
        string GetRightClickGroupKey();
    }
}
