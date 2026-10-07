using MultiSerVIsion.Solution.Application;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Presentation.Factor;
using MultiSerVIsion.Solution.Presentation.Presenter;
using MultiSerVIsion.Solution.Presentation.UserControls;
using MultiSerVIsion.Solution.Presentation.Views;
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


namespace MultiSerVIsion
{
    public partial class Form1 : Form
    {
        //主Tag动态创建并缓存用户控件 key:TabPage.Name Value:UserControl
        private readonly Dictionary<string, BaseViewUc> _viewCache = new Dictionary<string, BaseViewUc>();
        //MVP架构 presenter工厂 构造注入服务
        private readonly Dictionary<string, BasePresenter> _presenterCache = new Dictionary<string, BasePresenter>();
        //记录当前选中Tag的UserControl
        private BaseViewUc _lastActiveView;

        /// <summary>
        /// 构造注入接口声明
        /// </summary>
        private readonly Solution.Domain.Contexts.IDeviceContext _deviceContext;//限界上下文服务接口
        private readonly IEventBus _eventBus;//事件总线服务接口
        private readonly IDeviceInfoPresenterFactory _deviceinfoPresenterFactory;//演示者工厂服务接口
        private readonly IDeviceTreePresenterFactory _deviceTreePresenterFactory;
        private readonly IDeviceAppService _deviceAppService;//设备管理服务接口
        private readonly ICameraAppService _cameraAppService;//海康相机服务接口
        private readonly IVisonPresenterFactor _visionPresenterFactory;
        private readonly IDeviceDetailPresenterFactory _deviceDetailPresenterFactory;

        // 参数面板视图与 Presenter：全生命周期唯一，保证「Presenter 写入的实例」就是「界面显示的实例」
        private  IDeviceDatailView _deviceDetailView;
        private DeviceDetailPresenter _deviceDetailPresenter;

        //构造注入服务
        public Form1(
            IEventBus eventBus,
            IVisonPresenterFactor visionPresenterFactory,
            IDeviceAppService deviceAppService,
            ICameraAppService cameraAppService,
            IDeviceInfoPresenterFactory devicePresenterFactory,
            IDeviceTreePresenterFactory deviceTreePresenterFactory,
            IDeviceDetailPresenterFactory deviceDetailPresenterFactory,
             Solution.Domain.Contexts.IDeviceContext deviceContext)
        {
          
            _eventBus = eventBus;
            _deviceinfoPresenterFactory = devicePresenterFactory;
            _deviceTreePresenterFactory = deviceTreePresenterFactory;
            _deviceDetailPresenterFactory = deviceDetailPresenterFactory;
            _deviceAppService= deviceAppService;
            _cameraAppService= cameraAppService;
            _visionPresenterFactory=visionPresenterFactory;
            _deviceContext= deviceContext;

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
            IVisionView visionView = new UCVisionView();

            // 参数面板只创建一个视图实例，并让 Presenter 绑定到该实例
            _deviceDetailView = new CameraDateilUC();
            _deviceDetailPresenter = _deviceDetailPresenterFactory.Create(_deviceDetailView);
            var Treepresenter = _deviceTreePresenterFactory.Create(treeView);
            var infopresenter = _deviceinfoPresenterFactory.Create(deviceInfo);

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
            if (_viewCache.ContainsKey(tabKey)) return;
                                        
            BaseViewUc view = null;
            switch (tabKey)
            {
                case "tabPageMonitor":
                    view = new UCMonitorView();
                    break;

                case "tabPageVision":
                    // 视觉页由 Presenter 驱动取流与帧渲染，创建后立即初始化并缓存引用
                    var visionView = new UCVisionView();
                    var visionPresenter = _visionPresenterFactory.Create(visionView);
                    visionPresenter.Init();
                    _presenterCache[tabKey] = visionPresenter;
                    view = visionView;
                    break;
            }

            // 无对应视图类型的标签页（如占位页）直接跳过
            if (view == null) return;

            view.Dock = DockStyle.Fill;
            view.Visible = false;
            targetTab.Controls.Add(view);
            _viewCache.Add(tabKey, view);
        }
        private void SwitchTabView(TabPage targetTab)
        {
            if(targetTab==null)
                return;
            string tabKey = targetTab.Name;

            CreateViewIfNotExist(targetTab);
            if (!_viewCache.TryGetValue(tabKey, out BaseViewUc currView))
                return;

            if(_lastActiveView!=null&& _lastActiveView != currView)
            {
                _lastActiveView.OnViewHide();
                _lastActiveView.Visible = false;
            }

         /*   if (currView is UCMonitorView)
            {
                splitContainer2.SplitterDistance = ClampSplitterDistance(685);
            }
            else
            {
                splitContainer2.SplitterDistance = ClampSplitterDistance(789);
            }
*/
            currView.Visible = true;
            currView.OnViewHide();

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
            TabPage clickTab=tab.TabPages[clickIdx];
            if (clickTab== tab.SelectedTab)
            {
                SwitchTabView(clickTab);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var uc in _viewCache.Values)
            {
                uc.Dispose();
            }
            
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
