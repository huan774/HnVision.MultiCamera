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
    public class DeviceInfoPresenterFactory:IDeviceInfoPresenterFactory
    {
        // 固定依赖：由DI容器自动注入
        private readonly IDeviceManager _deviceManager;
        private readonly IEventBus _eventBus;
        private readonly ICameraAppService _cameraAppService;
        private readonly IDeviceContext _deviceContext;

        public DeviceInfoPresenterFactory(
            IDeviceManager manager,
            IEventBus eventBus,
            ICameraAppService cameraAppService,
            IDeviceContext deviceContext)
        {
            
            _cameraAppService = cameraAppService;
            _deviceContext = deviceContext;
            _deviceManager = manager;
            _eventBus = eventBus;
        }

        public DeviceInfoPresenter Create(IDeviceInfoParamView view)
        {
            var presenter = new DeviceInfoPresenter(view, _deviceManager, _eventBus,_cameraAppService, _deviceContext);
            presenter.Init(); // 统一初始化
            return presenter;
        }
    }
}
