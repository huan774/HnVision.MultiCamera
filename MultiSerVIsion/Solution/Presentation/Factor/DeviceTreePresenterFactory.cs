using MultiSerVIsion.Solution.Application;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Presentation.Presenter;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Presentation.Factor
{
    public class DeviceTreePresenterFactory : IDeviceTreePresenterFactory
    {
        // 固定依赖：由DI容器自动注入，新增依赖只加这里
        private readonly IDeviceAppService _deviceAppService;
        private readonly IEventBus _eventBus;
        private readonly ICameraAppService _cameraAppService;
        private readonly IDeviceContext _deviceContext;
        private readonly IDeviceManager _deviceManager;
        public DeviceTreePresenterFactory(
            IEventBus eventBus,
            IDeviceAppService appService,
            ICameraAppService cameraAppService,
            IDeviceContext deviceContext,
            IDeviceManager deviceManager
            )
        {
            _eventBus = eventBus;
            _deviceAppService = appService;
            _cameraAppService = cameraAppService;
            _deviceContext = deviceContext;
            _deviceManager= deviceManager;
        }

        public DeviceTressPresenter Create(IDeviceTreeView view)
        {
            // 装配：动态参数 + 固定依赖，统一构造Presenter
            var presenter = new DeviceTressPresenter(_eventBus, view,_deviceAppService, _deviceContext, _deviceManager, _cameraAppService);
            presenter.Init(); // 统一执行初始化（订阅事件等）
            return presenter;
        }
    }
}
