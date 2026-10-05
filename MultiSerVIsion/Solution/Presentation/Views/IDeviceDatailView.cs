using MultiSerVIsion.Solution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Views
{
    //参数配置更改事件
    public class ParamChangedEventArgs : EventArgs
    {
        public string NodeName { get; }
        public object Value { get; }
        public ParamChangedEventArgs(string nodeName,object value)
        {
            NodeName = nodeName;
            Value = value;
        }
    }
    public interface IDeviceDatailView
    {
        //参数属性
      /*  decimal ExposureTime { get; set; }
        decimal Gain { get; set; }
        decimal WhiteBalance { get; set; }
        string TriggerMode { get; set; }
        decimal AcquisitionFrameRate { get; set; }
        string Roi { get; set; }
        string PixelFormat { get; set; }
        decimal ReconnectCount { get; set; }
        bool AutoExposure { get; set; }
*/
        //事件
        event EventHandler<ParamChangedEventArgs> ParamChanged;
        event Action SaveConfigRequest;
       

        //方法
        
        void ShowMessage(string msg);
      

    }
}
