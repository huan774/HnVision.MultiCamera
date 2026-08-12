using MultiSerVIsion.Solution.Application;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Infrastructure.Repository;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Presentation.Factor;
using MultiSerVIsion.Solution.Presentation.Presenter;
using MultiSerVIsion.Solution.Presentation.Presenter.Factory;
using MultiSerVIsion.Solution.Presentation.UserControls;
using MultiSerVIsion.Solution.Presentation.Views;
using MultiSerVIsion.Solution.Presentation.Winforms;
using MultiSerVIsion.Solution.Shared.Helpers;
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
        private readonly Dictionary<string, BaseViewUc> _viewCache = new Dictionary<string, BaseViewUc>();
        private readonly Dictionary<string, BasePresenter> _presenterCache = new Dictionary<string, BasePresenter>();
        private BaseViewUc _lastActiveView;

        private const int DatailPanelWidth = 300;

        private readonly Solution.Domain.Contexts.IDeviceContext _deviceContext;
        private readonly IEventBus _eventBus;
        private readonly IDeviceInfoPresenterFactory _deviceinfoPresenterFactory;
        private readonly IDeviceTreePresenterFactory _deviceTreePresenterFactory;
        private readonly IDeviceAppService _deviceAppService;
        private readonly ICameraAppService _cameraAppService;
        private readonly IVisonPresenterFactor _visionPresenterFactory;
       
      /* private Form1()
        {
            InitializeComponent();
            InitLayoutSplit();
            InitPresent();
        }*/
        public Form1(
            IEventBus eventBus,
            IVisonPresenterFactor visionPresenterFactory,
            IDeviceAppService deviceAppService,
            ICameraAppService cameraAppService,
            IDeviceInfoPresenterFactory devicePresenterFactory,
            IDeviceTreePresenterFactory deviceTreePresenterFactory,
             Solution.Domain.Contexts.IDeviceContext deviceContext)
        {
          
            _eventBus = eventBus;
            _deviceinfoPresenterFactory = devicePresenterFactory;
            _deviceTreePresenterFactory = deviceTreePresenterFactory;
            _deviceAppService= deviceAppService;
            _cameraAppService= cameraAppService;
            _visionPresenterFactory=visionPresenterFactory;
            _deviceContext= deviceContext;

            _eventBus.Subscribe<ConfigDeviceSelectedEvent>(OnDeviceSelectedSwitchTab);

            InitializeComponent();
            InitLayoutSplit();
            InitPresent();
        }
        private void OnDeviceSelectedSwitchTab(ConfigDeviceSelectedEvent e)
        {
            // 只做UI切换，不包含任何业务逻辑
            tabControl1.SelectedTab = tabPageVision;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            splitContainer2.Panel1.Controls.Add(tabControl1);
            var firstTap = tabControl1.SelectedTab;
            CreateViewIfNotExist(firstTap);
            SwitchTabView(firstTap);
        }

        private void InitPresent()
        {
           
            IDeviceTreeView treeView = new DeviceTreeUC();
            IDeviceDatailView detailView = new CameraDateilUC();

            IDeviceInfoParamView deviceInfo = new DeviceInfoUC();
            IVisionView visionView = new UCVisionView();
        
            var Treepresenter = _deviceTreePresenterFactory.Create(treeView);
            var infopresenter = _deviceinfoPresenterFactory.Create(deviceInfo);
          

            split_Devicetree.Panel1.Controls.Add(treeView as DeviceTreeUC);
            split_Devicetree.Panel2.Controls.Add(deviceInfo as DeviceInfoUC);
        }

       /*     treeView.ConfigDeviceSelected += deviceId =>
            {
                infoPresent.LoadConfigCamera(deviceId);
            };

            treeView.OnlineDeviceSelected += async dto =>
            {
               await infoPresent.LoadDeviceInfoAsync(dto);
            };

            treeView.NoDeviceSelected += () =>
            {
                infoPresent.Clear();
            };

           detailPresenter.OnCreaateDetailUc += (System.Windows.Forms.UserControl uc) =>
            {
                split_inter.Panel2.SuspendLayout();
               
                try
                {
                    foreach (Control c in split_inter.Panel2.Controls)
                    {
                        if (c is UserControl existUc)
                            existUc.Visible = false;
                    }
                    if (!split_inter.Panel2.Controls.Contains(uc))
                    {
                        uc.Dock = DockStyle.Fill;
                        split_inter.Panel2.Controls.Add(uc);
                    }
                    uc.Visible = true;
                    split_inter.SplitterDistance = split_inter.Width - DatailPanelWidth;
                }
                finally
                {
                    split_inter.Panel2.ResumeLayout(true);
                }
            };

            treeView.DeviceNodeSelected += devId =>
            {
                detailPresenter.LoadDevice(devId);
            };
            treeView.DeviceNodeUnSelect += () =>
            {
                detailPresenter.ClearEdit();
                split_inter.SplitterDistance = split_inter.Width;
            };
           
        }*/
       
        private void InitLayoutSplit()
        {
            split_outer.FixedPanel = FixedPanel.Panel1;
            split_outer.Panel1MinSize = 180;
            split_outer.SplitterDistance = 220;

            split_inter.Dock = DockStyle.Fill;
            split_inter.FixedPanel = FixedPanel.None;
            split_inter.SplitterDistance = split_inter.Width;

            tabControl1.Dock = DockStyle.Fill;
            split_inter.Panel1.Controls.Add(tabControl1);
        }

        private void CreateViewIfNotExist(TabPage targetTab)
        {
            string tabKey = targetTab.Name;
            if (_viewCache.ContainsKey(tabKey)) return;
                                        
            BaseViewUc view = null;
            switch (tabKey)
            {
                case "tabPageMonitor":
                    view = new UCMonitorView();
                    break;
                case "tabPageVision":
                    view=new UCVisionView();
                    var vision = view as IVisionView;
                    var visionPresenter = _visionPresenterFactory.Create(vision);
                    visionPresenter.Init();
                    _presenterCache[tabKey] = visionPresenter;

                    break;
            }
            if(view==null) return;
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

            if (currView is UCMonitorView)
            {
                
                splitContainer2.SplitterDistance = 685;
            }
            else
            {
                
                splitContainer2.SplitterDistance = 789;
            }

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
