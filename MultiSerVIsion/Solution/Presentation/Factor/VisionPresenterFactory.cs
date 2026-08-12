using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Domain.Contexts;
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
    public class VisionPresenterFactory:IVisonPresenterFactor
    {
        private readonly ICameraAppService _cameraAppService;
        private readonly IEventBus _eventBus;
        private readonly IDeviceContext _deviceContext;
        public VisionPresenterFactory(
            ICameraAppService cameraAppService,
            IDeviceContext deviceContext,
            IEventBus eventBus)
        {
            _cameraAppService = cameraAppService;
            _deviceContext = deviceContext;
            _eventBus = eventBus;
        }
        public VisionPreseter Create(IVisionView vision)
        {
            return new VisionPreseter(vision, _eventBus, _deviceContext, _cameraAppService);
        }
    }
   
}
