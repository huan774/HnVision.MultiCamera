using MultiSerVIsion.Solution.Presentation.Presenter;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Factor
{
    public interface IDeviceInfoPresenterFactory
    {
        /// <summary>
        /// 创建设备信息Presenter
        /// </summary>
        /// <param name="view">设备信息视图实例（UI层传入）</param>
        /// <returns>装配完成的Presenter</returns>
        DeviceInfoPresenter Create(IDeviceInfoParamView view);
    }
}
