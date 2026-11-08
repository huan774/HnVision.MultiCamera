using MultiSerVIsion.Solution.Application.Device;
using MultiSerVIsion.Solution.Application.Camera;
using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Presentation.Common;
using MultiSerVIsion.Solution.Presentation.DevicePanel;
using MultiSerVIsion.Solution.Presentation.CameraPreview;
using MultiSerVIsion.Solution.Presentation.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MultiSerVIsion.Solution.Presentation.Shell
{
    /// <summary>
    /// 主窗体：设备树 / 设备信息 / 相机参数面板 / 相机预览页的宿主。
    /// 【范围】项目已收敛为纯相机调试，仅保留相机相关标签页与视图。
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// 标签页条目：视图与其 Presenter 成对缓存，避免两份缓存各自维护 key。
        /// </summary>
        private sealed class ViewEntry
        {
            /// <summary>视图实例（切走时隐藏，宿主关闭时释放）</summary>
            public BaseViewUc View;

            /// <summary>该页 Presenter；无 Presenter 的展示页为 null</summary>
            public BasePresenter Presenter;
        }

        //标签页视图与其 Presenter 成对缓存，key = TabPage.Name
        private readonly Dictionary<string, ViewEntry> _viewEntries = new Dictionary<string, ViewEntry>();
        //记录当前选中Tag的UserControl
        private BaseViewUc _lastActiveView;

        /// <summary>
        /// 构造注入接口声明（仅后台服务；视图与 Presenter 由本窗体直接创建）
        /// </summary>
        private readonly Solution.Domain.Contexts.IDeviceContext _deviceContext;//限界上下文服务接口
        private readonly IEventBus _eventBus;//事件总线服务接口
        private readonly IDeviceAppService _deviceAppService;//设备管理服务接口
        private readonly ICameraAppService _cameraAppService;//海康相机服务接口
        private readonly IDeviceManager _deviceManager;//设备内存管理器（供设备树/信息页 Presenter 使用）

        // 参数面板视图与 Presenter：全生命周期唯一，保证「Presenter 写入的实例」就是「界面显示的实例」
        private  IDeviceDatailView _deviceDetailView;
        private DeviceDetailPresenter _deviceDetailPresenter;

        //构造注入服务
        public Form1(
            IEventBus eventBus,
            IDeviceAppService deviceAppService,
            ICameraAppService cameraAppService,
            IDeviceManager deviceManager,
             Solution.Domain.Contexts.IDeviceContext deviceContext)
        {
           
            _eventBus = eventBus;
            _deviceAppService = deviceAppService;
            _cameraAppService = cameraAppService;
            _deviceManager = deviceManager;
            _deviceContext = deviceContext;

            //事件总线订阅选中设备切换事件
            _eventBus.Subscribe<ConfigDeviceSelectedEvent>(OnDeviceSelectedSwitchTab);
            _eventBus.Subscribe<DeviceConnectionChangedEveent>(OnDeviceConnectionChanged);
            //设计器控件初始化
            InitializeComponent();
            //设备信息区域分割距离设置
            InitLayoutSplit();
            //自定义控件初始化
            InitPresent();
        }

        private void InitPresent()
        {
            IDeviceTreeView treeView = new DeviceTreeUC();
            IDeviceInfoParamView deviceInfo = new DeviceInfoUC();

            // 视图由宿主唯一创建，Presenter 直接构造注入（不再经工厂转发）
            var treePresenter = new DeviceTressPresenter(_eventBus, treeView, _deviceAppService, _deviceContext, _deviceManager, _cameraAppService);
            treePresenter.Init();

            var infoPresenter = new DeviceInfoPresenter(deviceInfo, _deviceManager, _eventBus, _cameraAppService, _deviceContext);
            infoPresenter.Init();

            // 参数面板只创建一个视图实例，并让 Presenter 绑定到该实例
            _deviceDetailView = new CameraDateilUC();
            _deviceDetailPresenter = new DeviceDetailPresenter(_cameraAppService, _deviceDetailView, _eventBus);
            _deviceDetailPresenter.Init();

            split_Devicetree.Panel1.Controls.Add(treeView as DeviceTreeUC);
            split_Devicetree.Panel2.Controls.Add(deviceInfo as DeviceInfoUC);
        }


        /// <summary>
        /// 设备连接状态变化：连接后展开右侧参数面板并回填当前设备参数，断开后收起。
        /// 【关键】必须复用 InitPresent 中创建的唯一视图实例，
        /// 否则会 new 出第二个视图，导致 Presenter 回填的值只写进了不可见的那个实例。
        /// </summary>
        private async void OnDeviceConnectionChanged(DeviceConnectionChangedEveent e)
        {
            if (e == null) return;

            if (e.IsConnected)
            {
                split_inter.SplitterDistance = 685;
                ShowDeviceDetailView();

                // 连接成功后才具备读参条件，此时按当前设备回填参数面板
                if (_deviceDetailPresenter != null)
                {
                    await _deviceDetailPresenter.LoadDeviceParamsAsync(_deviceContext.CurrentDeviceId);
                }
            }
            else
            {
                split_inter.SplitterDistance = 1121;
            }
        }

        /// <summary>把唯一的参数面板实例挂载到右侧面板（重复调用不会重复挂载）</summary>
        private void ShowDeviceDetailView()
        {
            var view = _deviceDetailView as CameraDateilUC;
            if (view == null) return;
            if (view.Parent == split_inter.Panel2) return;

            split_inter.Panel2.Controls.Clear();
            view.Dock = DockStyle.Fill;
            split_inter.Panel2.Controls.Add(view);
        }
        private void OnDeviceSelectedSwitchTab(ConfigDeviceSelectedEvent e)
        {
            // 只做UI切换，不包含任何业务逻辑
            tabControl1.SelectedTab = tabPageVision;
        }
        /// <summary>
        /// 窗体加载：初始化首个标签页视图。
        /// 【说明】SwitchTabView 内部已包含「视图不存在则创建」的懒加载逻辑，此处无需重复创建。
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            // 仅需切换一次：SwitchTabView 内部会按需创建视图
            SwitchTabView(tabControl1.SelectedTab);
        }

        /// <summary>
        /// 收敛布局相关设置，避免与设计器属性重复定义而产生冲突。
        /// 【规范】控件归属与尺寸约束统一由 Form1.Designer.cs 定义，此处仅保留运行期必需的设置。
        /// </summary>
        private void InitLayoutSplit()
        {
            // 左侧设备树区域宽度固定，窗口缩放时仅右侧主工作区自适应
            split_outer.FixedPanel = FixedPanel.Panel1;
        }

        /// <summary>
        /// 按需为指定标签页创建并挂载视图（含其 Presenter）。
        /// 【幂等】已创建过的标签页直接复用缓存，避免重复创建视图与重复订阅事件。
        /// 【约定】视图统一 Dock=Fill 且初始不可见，可见性只由 SwitchTabView 控制。
        /// </summary>
        /// <param name="targetTab">目标标签页；为空时不做处理</param>
        private void CreateViewIfNotExist(TabPage targetTab)
        {
            if (targetTab == null) return;

            string tabKey = targetTab.Name;
            // 已创建过则复用缓存中的视图
            if (_viewEntries.ContainsKey(tabKey)) return;

            BaseViewUc view = null;
            BasePresenter presenter = null;

            switch (tabKey)
            {
                case "tabPageVision":
                    // 视觉页由 Presenter 驱动取流与帧渲染，创建后立即初始化
                    var visionView = new UCVisionView();
                    var visionPresenter = new VisionPreseter(visionView, _eventBus, _deviceContext, _cameraAppService);
                    visionPresenter.Init();
                    presenter = visionPresenter;
                    view = visionView;
                    split_outer.Panel1Collapsed = false;
                    break;

            }

            // 无对应视图类型的标签页（如占位页）直接跳过
            if (view == null) return;

            view.Dock = DockStyle.Fill;
            view.Visible = false;
            targetTab.Controls.Add(view);
            _viewEntries.Add(tabKey, new ViewEntry { View = view, Presenter = presenter });
        }
        private void SwitchTabView(TabPage targetTab)
        {
            if(targetTab==null)
                return;
            string tabKey = targetTab.Name;

            CreateViewIfNotExist(targetTab);
            if (!_viewEntries.TryGetValue(tabKey, out var entry))
                return;

            BaseViewUc currView = entry.View;

            if(_lastActiveView!=null&& _lastActiveView != currView)
            {
                // 切走：仅隐藏并通知视图暂停（如停止取流/刷新），保留实例与状态
                _lastActiveView.OnViewHide();
                _lastActiveView.Visible = false;
            }

            // 显示当前视图：必须调用 OnViewShow，否则子类覆写的展示逻辑永不执行
            currView.Visible = true;
            currView.OnViewShow();

            _lastActiveView = currView;

        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
             var tabCtrl=sender as TabControl;
             SwitchTabView(tabCtrl.SelectedTab);
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            TabControl tab=sender as TabControl;
            if (tab == null) return;
           
            Point pt = e.Location;
            int clickIdx = -1; 
            for (int i = 0; i < tab.TabCount; i++)
            {
                Rectangle rect = tab.GetTabRect(i);
                if (rect.Contains(pt))
                {
                    clickIdx = i;
                    break;
                }
            }
            // 未命中任何页签时直接返回，避免 TabPages[-1] 越界
            if (clickIdx < 0) return;

            TabPage clickTab=tab.TabPages[clickIdx];
            if (clickTab== tab.SelectedTab)
            {
                SwitchTabView(clickTab);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 宿主关闭：释放全部页面的 Presenter（先退订事件）与视图（含非托管资源）
            foreach (var entry in _viewEntries.Values)
            {
                (entry.Presenter as IDisposable)?.Dispose();
                entry.View.Dispose();
            }
            _viewEntries.Clear();

            // 右侧参数面板不在标签页缓存中，需单独释放
            (_deviceDetailPresenter as IDisposable)?.Dispose();
            (_deviceDetailView as IDisposable)?.Dispose();
        }
        private void Vision_ExposureChanged(object sender, EventArgs e)
        {
            var vision = sender as UCVisionView;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
