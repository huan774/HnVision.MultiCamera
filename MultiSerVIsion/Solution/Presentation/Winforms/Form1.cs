using MultiSerVIsion.Solution.Application;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Infrastructure.Repository;
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
        private readonly Dictionary<string, UserControl> _cachedDetailUc = new Dictionary<string, UserControl>();
        private BaseViewUc _lastActiveView;

        private const int DatailPanelWidth = 300;

        private readonly IDeviceDatailView _treeView;
        private readonly DeviceTressPresenter _treePresent;

        public Form1()
        {
            InitializeComponent();
            InitLayoutSplit();
            InitPresent();
            
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
            IDeviceRepository repo = new DeviceRepository();
            IDeviceDomainSerivce domainSve = new DeviceDomainService();
            IDeviceManager manager = new DeviceManager(repo);
            IDeviceAppService appSvc = new DeviceAppService(domainSve,manager);

            IDeviceTreeView treeView = new DeviceTreeUC();
            IDeviceDatailView detailView = new CameraDateilUC();


            var treePresenter = new DeviceTressPresenter(treeView, appSvc);
            var detailPresenter = new DeviceDetailPresenter(appSvc);
/*
            split_outer.Panel1.Controls.Add(treeView as DeviceTreeUC);*/
           split_Devicetree.Panel1.Controls.Add(treeView as DeviceTreeUC);
            split_Devicetree.Panel2.Controls.Add(new DeviceInfoUC());

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
           
        }
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
                    var vision=view as UCVisionView;
                    vision.ExposureValueChanged += Vision_ExposureChanged;
                    
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
