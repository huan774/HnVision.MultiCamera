using MultiSerVIsion.Solution.Application;
using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Presentation.Presenter;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Factor
{
    public interface IDeviceDetailPresenterFactory
    {
        DeviceDetailPresenter Create(IDeviceDatailView view);
    }
    public class DeviceDetailPresenterFactory : IDeviceDetailPresenterFactory
    {
       
        private readonly IEventBus _eventBus;
        private readonly ICameraAppService _cameraAppService;
        private readonly IDeviceDatailView _datailView;
        public DeviceDetailPresenterFactory(
            IEventBus eventBus,
            ICameraAppService cameraAppService,
            IDeviceDatailView datailView)
        {
            _eventBus = eventBus;
            _cameraAppService = cameraAppService;
            _datailView = datailView;
        }
        public DeviceDetailPresenter Create(IDeviceDatailView view)
        {
            var presenter = new DeviceDetailPresenter(_cameraAppService, view, _eventBus);
            presenter.Init();
            return presenter;
        }
    }
}
