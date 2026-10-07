using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
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
        decimal ExposureTime { get; set; }
        decimal Gain { get; set; }
        decimal AcquisitionFrameRate { get; set; }
        string TriggerMode { get; set; }
    
       /* string Roi { get; set; }*/
        string TriggerSource { get; set; }
        string PixelFormat { get; set; }
        
        /*  decimal ReconnectCount { get; set; }*/
        /*      bool AutoExposure { get; set; }*/

        //事件
        event EventHandler<ParamChangedEventArgs> ParamChanged;

        //方法

        /// <summary>
        /// 本面板支持调节的参数节点名。
        /// 【职责】由视图声明自己关心哪些参数，上层据此回读规格与当前值，避免两侧各自硬编码清单。
        /// </summary>
        IReadOnlyList<string> SupportedParamNames { get; }

        /// <summary>
        /// 应用参数规格：把相机上报的范围/步长/可选项落实到对应控件，实现界面限幅。
        /// </summary>
        /// <param name="spec">参数规格；为空时忽略</param>
        void ApplyParamSpec(ParamSpec spec);

        /// <summary>
        /// 按参数节点名回填当前值（受控件范围限幅，且不触发改参事件）
        /// </summary>
        /// <param name="paramName">参数节点名</param>
        /// <param name="value">参数当前值</param>
        void ApplyParamValue(string paramName, object value);

        void ShowMessage(string msg);
      
    }
}
