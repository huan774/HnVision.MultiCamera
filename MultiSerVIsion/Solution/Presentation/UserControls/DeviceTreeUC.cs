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

namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    public partial class DeviceTreeUC : BaseViewUc,IDeviceTreeView
    {
       
        private Func<string, bool> GetDeviceEnableStatus {  get; set; }
        public Func<string,(bool Enable,bool Online)> GetDeviceStatus {  get; set; }

        public event Action<string> DeviceNodeSelected;
        public event Action DeviceNodeUnSelect;
        public event Action AddDeviceRequest;
        public event Action<string> RemoveDeviceRequest;
        public event Action<string> CopyDeviceRequest;
        public event Action<string> ToggleDeviceEnableRequest;

        private TreeNode _rightClickGroupNode;
        private TreeNode _rightClickNode;
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

        private void RaiseDeviceSelected(string device)
        {
            DeviceNodeSelected?.Invoke(device);
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
        private void TreeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var node=treeView_Device.GetNodeAt(e.X, e.Y);

            if(node == null) return;

            treeView_Device.SelectedNode = node;
            _rightClickNode=node;
            string tar =node.Tag?.ToString()?? string.Empty;
            bool isGroupNode = tar.StartsWith("Group_");

            tsmi_AddChild.Visible = isGroupNode;
            tsmi_Copy.Visible = !isGroupNode;
            tsmi_Delete.Visible = !isGroupNode   ;
            tsmi_EnableDisable.Visible = !isGroupNode;

        }
        private void TreeView_Device_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
      
         if (e.Button == MouseButtons.Left)
            {
                _rightClickNode = null;
                _rightClickGroupNode= null;
               
                string nodeTage=e.Node.Tag?.ToString() ?? string.Empty;
                if (nodeTage != null && nodeTage.StartsWith("Group_"))
                {
                    RaiseDeviceUnSelect();
                }
                else if (!string.IsNullOrEmpty(nodeTage))
                {
                    
                    _rightClickGroupNode = e.Node.Parent;
                    _rightClickNode = e.Node;
                    RaiseDeviceSelected(nodeTage);
                }
                else
                {
                    RaiseDeviceUnSelect();
                }
            } 
            else if(e.Button == MouseButtons.Right)
            {
                string tag = e.Node.Tag?.ToString() ?? string.Empty;
                if (tag.StartsWith("Group_"))
                {
                    _rightClickGroupNode=e.Node;
                    _rightClickNode=null;
                }
                else
                {
                    _rightClickNode = e.Node;
                    _rightClickGroupNode = e.Node.Parent;
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

            if(GroupNode == null) return;

            TreeNode devNode = new TreeNode(deveName) { Tag = devId };
           
            GroupNode.Nodes.Add(devNode);
            GroupNode.Expand();

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
           /* treeView_Device.Nodes.Add("PLC分组").Tag = "Group_PLCser";*/
            
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
        public void RefreshDeviceStatusIcon() {

            if (GetDeviceStatus == null) return;

            foreach (TreeNode groupNode in treeView_Device.Nodes)
            {
                foreach (TreeNode node in groupNode.Nodes)
                {
                    string devId = node.Tag?.ToString();
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
