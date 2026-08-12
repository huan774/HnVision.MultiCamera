using MultiSerVIsion.Solution.Presentation.Presenter;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Factor
{
    public interface IDeviceTreePresenterFactory
    {
        /// <summary>
        /// 创建设备树Presenter
        /// </summary>
        /// <param name="view">设备树视图实例（UI层传入）</param>
        /// <returns>装配完成的Presenter</returns>
        DeviceTressPresenter Create(IDeviceTreeView view);
    }
}
