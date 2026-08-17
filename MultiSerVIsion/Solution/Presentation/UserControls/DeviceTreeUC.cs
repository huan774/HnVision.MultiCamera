using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Presentation.Presenter;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*using System.Windows.Input;*/

namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    public partial class DeviceTreeUC : BaseViewUc,IDeviceTreeView
    {
        private Func<string, bool> GetDeviceEnableStatus {  get; set; }
        public Func<string,(bool Enable,bool Online)> GetDeviceStatus {  get; set; }

        public event Action AddToConfigRequsted;
        public event Action RefreshSearchRequested;
        public event Action DeviceNodeUnSelect;
        public event Action AddDeviceRequest;

        public event Action<string> RemoveDeviceRequest;
        public event Action<string> CopyDeviceRequest;
        public event Action<string> ToggleDeviceEnableRequest;
        public event Action<string> DeviceNodeSelected;

        public event Action ViewLoaded;

        public event Action<string> ConfigDeviceSelected;
        public event Action<CameraDeviceDto> OnlineDeviceSelected;
        public event Action NoDeviceSelected;

        private TreeNode _rightClickGroupNode;
        private TreeNode _rightClickNode;

        private  CameraDeviceDto _selectedOnlineCamera;
        public CameraDeviceDto SelectedOnlineCamera => _selectedOnlineCamera;
        public DeviceTreeUC()
        {
            InitializeComponent();
            SetUIPlaceholder();


            this.Dock = DockStyle.Fill;

            treeView_Device.ContextMenuStrip = contextMenuStrip_Device;
            treeView_Device.MouseDown += TreeView_MouseDown;
            treeView_Device.NodeMouseClick += TreeView_Device_NodeMouseClick;
            contextMenuStrip_Device.Opening += ContextMenuStrip_Device_Opening;

            btn_AddDevice.Click += Btn_AddDevice_Click;
            btn_DelDevice.Click += Btn_DelDevice_Click;
            
            tsmi_AddChild.Click += Tsim_AddChild_Click;
            tsmi_Copy.Click += Tsim_Copy_Click;
            tsmi_EnableDisable.Click += Tsmi_EnableDisable_Click;
            tsmi_Delete.Click += Tsmi_Delete_Click;

        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 只在这里触发一次初始化
            ViewLoaded?.Invoke();
        }

        private void RaiseDeviceSelected(string device)
        {
            DeviceNodeSelected?.Invoke(device);
        }
        private void btn_RefreshTress_Click(object sender, EventArgs e)
        {
            RefreshSearchRequested?.Invoke();
        }
        private void RaiseDeviceUnSelect()
        {
            DeviceNodeUnSelect?.Invoke();
        }
        private void RaiseAddDevice()
        {
            AddDeviceRequest?.Invoke();
        }
        private void RaiseRemoveDevice(string devId)
        {
            RemoveDeviceRequest?.Invoke(devId);
        }
        private void RaiseCopyDevice(string devId)
        {
            CopyDeviceRequest?.Invoke(devId);
        }
        private void RaiseToggleEnable(string devId)
        {
            ToggleDeviceEnableRequest?.Invoke(devId);
        }
        private void treeView_Device_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                DeviceNodeUnSelect?.Invoke();
                return;
            }

            // 按节点Tag类型分发事件
            switch (e.Node.Tag)
            {
                case CameraDeviceDto onlineCam:
                    OnlineDeviceSelected?.Invoke(onlineCam);
                    break;

                case string deviceId when !deviceId.StartsWith("Group_"):
                    ConfigDeviceSelected?.Invoke(deviceId);
                    break;

                default:
                    // 分组节点或其他
                    DeviceNodeUnSelect?.Invoke();
                    break;
            }
        }
       private void TreeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var node=treeView_Device.GetNodeAt(e.X, e.Y);

            if(node == null) return;

            treeView_Device.SelectedNode = node;
            _rightClickNode=node;
            string tar = node.Tag?.ToString() ?? string.Empty;
            bool isGroupNode = tar.StartsWith("Group_");

           /* bool isOnlineDeviceNode = node.Tag is CameraDeviceDto;*/

           tsmi_AddChild.Visible = isGroupNode;
            tsmi_Copy.Visible = !isGroupNode /*&& !isOnlineDeviceNode*/;
            tsmi_Delete.Visible = !isGroupNode /*&& !isOnlineDeviceNode*/;
            tsmi_EnableDisable.Visible = !isGroupNode/* && !isOnlineDeviceNode*/;

        }
        private void TreeView_Device_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _rightClickNode = null;
                _rightClickGroupNode = null;
                _selectedOnlineCamera = null; // 左键点击先清空在线选中状态

                // ========== 新增：先判断Tag类型 ==========
                if (e.Node.Tag is CameraDeviceDto onlineCam)
                {
                    // 分支1：在线设备节点 → 仅记录在线相机，不触发原有设备联动
                    _selectedOnlineCamera = onlineCam;
                    _rightClickGroupNode = e.Node.Parent;
                    _rightClickNode = e.Node;
                    // 在线设备不触发组态设备选中事件，避免右侧详情面板错乱
                    RaiseDeviceUnSelect();
                }
                else if (e.Node.Tag is string nodeTag)
                {
                    // ========== 原有逻辑整体移到这里 ==========
                    if (nodeTag.StartsWith("Group_"))
                    {
                        RaiseDeviceUnSelect();
                    }
                    else if (!string.IsNullOrEmpty(nodeTag))
                    {
                        _rightClickGroupNode = e.Node.Parent;
                        _rightClickNode = e.Node;
                        RaiseDeviceSelected(nodeTag);
                    }
                    else
                    {
                        RaiseDeviceUnSelect();
                    }
                }
                else
                {
                    RaiseDeviceUnSelect();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // ========== 新增：右键同样区分节点类型 ==========
                if (e.Node.Tag is CameraDeviceDto onlineCam)
                {
                    // 在线设备节点
                    _selectedOnlineCamera = onlineCam;
                    _rightClickNode = e.Node;
                    _rightClickGroupNode = e.Node.Parent;
                }
                else if (e.Node.Tag is string tag)
                {
                    // ========== 原有逻辑整体移到这里，一行不改 ==========
                    if (tag.StartsWith("Group_"))
                    {
                        _rightClickGroupNode = e.Node;
                        _rightClickNode = null;
                    }
                    else
                    {
                        _rightClickNode = e.Node;
                        _rightClickGroupNode = e.Node.Parent;
                    }
                }
            }

         
        }
        private void ContextMenuStrip_Device_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var rightNode = _rightClickNode;
            var groupNode = _rightClickGroupNode;

            if (rightNode == null && groupNode==null)
            {
                e.Cancel = true;
                return;
            }

            bool isDeviceNode = rightNode!= null;

            tsmi_AddChild.Visible = true;

            tsmi_Copy.Visible = isDeviceNode;
            tsmi_EnableDisable.Visible = isDeviceNode;
            tsmi_Delete.Visible = isDeviceNode;

            if (isDeviceNode && GetDeviceStatus != null)
            {
                string devId = rightNode.Tag.ToString();
                var (enable,_)=GetDeviceStatus(devId);
                tsmi_EnableDisable.Text = enable ? "禁用设备" : "启动设备";
            }
        }
        private void Btn_AddDevice_Click(object sender, EventArgs e)
        {
            AddToConfigRequsted?.Invoke();
            RaiseAddDevice();
        }
        private void Btn_DelDevice_Click(object sender, EventArgs e)
        {
            if (treeView_Device.SelectedNode?.Tag is string deviceId)
            {
                RaiseRemoveDevice(deviceId);
            }
        }
        private void Tsim_AddChild_Click(object sender, EventArgs e)
        {
            RaiseAddDevice();
        }
        private void Tsim_Copy_Click(object sender, EventArgs e)
        {
            
            if (_rightClickNode == null)
            {
                ShowMessage("未选中节点");
                return;
            }
           
            string tag = _rightClickNode.Tag?.ToString() ?? "";
            if (tag.StartsWith("Group_"))
            {
                ShowMessage("分组不支持复制");
                return;
            }
            if (!string.IsNullOrEmpty(tag))
            {
                RaiseCopyDevice(tag);
            }
        }
        private void Tsmi_EnableDisable_Click(object sender, EventArgs e)
        {
            
            if(_rightClickNode?.Tag is string deviceId)
            {
                RaiseToggleEnable(deviceId);
            }
        }
        private void Tsmi_Delete_Click(object sender,EventArgs e)
        {
           
            if(_rightClickNode?.Tag is string deviceId)
            {
                RaiseRemoveDevice(deviceId);
            }
        }
        public void AddTreeNode(string groupTag,string devId,string deveName)
        {

            TreeNode GroupNode = null;
            foreach (TreeNode g in treeView_Device.Nodes)
            {
                if (g.Tag?.ToString() == groupTag)
                {
                    GroupNode = g;
                    break;
                }
            }

            // 兜底：分组 Tag 不匹配（如历史 JSON 中 GroupTage 为空/非标准值）时，
            // 默认归入相机分组，避免设备静默丢失
            if (GroupNode == null)
            {
                foreach (TreeNode g in treeView_Device.Nodes)
                {
                    if (g.Tag?.ToString() == "Group_Camera")
                    {
                        GroupNode = g;
                        break;
                    }
                }
            }
            if(GroupNode == null) return;

            if (GroupNode.Nodes.Cast<TreeNode>().Any(n => n.Tag?.ToString() == devId))
                return;

            TreeNode devNode = new TreeNode(deveName) { Tag = devId };
           
            GroupNode.Nodes.Add(devNode);
            GroupNode.Expand();

        }
        public void BindOnlineCameraTree(List<CameraDeviceDto> onlineCameras)
        {

            // 找到在线设备分组
            TreeNode onlineGroup = null;
            foreach (TreeNode node in treeView_Device.Nodes)
            {
                if (node.Tag?.ToString() == "Group_Online")
                {
                    onlineGroup = node;
                    break;
                }
            }
            if (onlineGroup == null) return;

            // 清空旧的扫描结果，重新填充
            onlineGroup.Nodes.Clear();
            foreach (var cam in onlineCameras)
            {
                var node = new TreeNode($"{cam.Model} [{cam.IpAddress}]")
                {
                    Tag = cam, // 挂载在线相机Dto，选中/右键时读取
                    ForeColor = Color.Gray // 视觉区分：未添加的设备置灰
                };
                onlineGroup.Nodes.Add(node);
            }
            onlineGroup.Expand();
            /* // BindOnlineCameraTree 里判断一下序列号是否已存在
             var existSerials = _deviceManager.GetDevices<CameraDeviceEntity>()
                 .Select(c => c.CameraConfig.ConnectConfig.SerialNumber)
                 .ToHashSet();

             foreach (var cam in onlineCameras)
             {
                 bool isAdded = existSerials.Contains(cam.SerialNumber);
                 var node = new TreeNode(isAdded ? $"{cam.Model} [已添加]" : $"{cam.Model} [{cam.IpAddress}]")
                 {
                     Tag = cam,
                     ForeColor = isAdded ? Color.Green : Color.Gray,
                     Enabled = !isAdded // 已添加的禁止重复点击添加
                 };
                 onlineGroup.Nodes.Add(node);*/
        }
            
        public void RemoveTreeNode(string devid)
        {

            foreach(TreeNode g in treeView_Device.Nodes)
            {
                for(int i=g.Nodes.Count-1; i>=0; i--)
                {
                    TreeNode n = g.Nodes[i];
                    if(n.Tag!=null && n.Tag.ToString() == devid)
                    {
                        n.Remove();
                        return;
                    }
                }
            }
        }
        private TreeNode FindNode(TreeNodeCollection nodes, string tag)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag?.ToString() == tag) return node;
                var child = FindNode(node.Nodes, tag);
                if (child != null) return child;
            }
            return null;
        }
        public bool ShowConfirmDialog(string msg)
        {
            return MessageBox.Show(msg, "提示", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }
        public string GetRightClickGroupKey()
        {
            return _rightClickGroupNode?.Tag?.ToString() ?? string.Empty;
        }
        public override void OnViewShow()
        {
            base.OnViewShow();
            RefreshDeviceStatusIcon();
        }
        public override void SetUIPlaceholder()
        {
            treeView_Device.Nodes.Clear();
            treeView_Device.Nodes.Add("相机分组").Tag = "Group_Camera";
            treeView_Device.Nodes.Add("运动轴分组").Tag = "Group_Motion";
            treeView_Device.Nodes.Add("局域网在线设备").Tag = "Group_Online";
            
        }
        public void ClearSelectNode()
        {
            treeView_Device.SelectedNode = null;
            _rightClickNode= null;
            RaiseDeviceUnSelect();
        }

        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }
        public void RefreshConfigDeviceTree(List<DeviceEntity> configDevices)
        {

            // 先清空两个业务分组的子节点，保留分组本身
            foreach (TreeNode node in treeView_Device.Nodes)
            {
                string tag = node.Tag?.ToString();
                if (tag is "Group_Camera" ||tag is "Group_Motion")
                    node.Nodes.Clear();
            }

            // 按类型挂到对应分组，复用你原有AddTreeNode
            foreach (var dev in configDevices)
            {
                string groupTag = dev.DeviceType;
                switch (dev.DeviceType) {
                    case "Camera":
                      groupTag=  "Group_Camera";
                        break;
                    case "MotionCard":
                        groupTag = "Group_Motion";
                        break;
                    default:
                        groupTag = string.Empty;
                        break;
                }
                
                if (!string.IsNullOrEmpty(groupTag))
                {
                    AddTreeNode(groupTag, dev.DeviceId, dev.DeviceName);
                }
            }
        }
        public void RefreshDeviceStatusIcon() {

            if (GetDeviceStatus == null) return;

            foreach (TreeNode groupNode in treeView_Device.Nodes)
            {
                foreach (TreeNode node in groupNode.Nodes)
                {
                    string devId = node.Tag?.ToString();

                    if(devId== "Group_Online") continue;
                    if (string.IsNullOrEmpty(devId)) continue;

                    var (enable, onlie) = GetDeviceStatus(devId);
                    if (!enable)
                    {
                        node.ImageIndex = 1;
                        node.SelectedImageIndex = 1;

                    }
                    else if (!onlie)
                    {
                        node.ImageIndex = 2;
                        node.SelectedImageIndex = 2;
                    }
                    else
                    {
                        node.ImageIndex = 0;
                        node.SelectedImageIndex = 0;
                    }
                }
            }
        }
    }
}
