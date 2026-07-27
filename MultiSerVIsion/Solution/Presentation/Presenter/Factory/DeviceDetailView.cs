using MultiSerVIsion.Solution.Presentation.UserControls;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Presenter.Factory
{
    public static class DeviceDetailView
    {
        public static IDeviceDatailView Create(string groupTag)
        {
            if(string.IsNullOrEmpty(groupTag))
                return new CameraDateilUC();

            /*if (groupTag.Contains("PLC"))
                return new PlcDetailUC();*/
            if (groupTag.Contains("Camera"))
                return new CameraDateilUC();
            /*if (groupTag.Contains("Motion"))
                return new CardDetailUC();*/

            return new CameraDateilUC();

        }
    }
}
