using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Presentation.Views;
using MvCameraControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    public partial class CameraDateilUC : BaseViewUc,IDeviceDatailView
    {
        private bool _suppress;
        public event EventHandler<ParamChangedEventArgs> ParamChanged;
      
        
        public CameraDateilUC()
        {
            InitializeComponent();
            BindInternalEvents();
        }
        public decimal ExposureTime 
        {
            get => nud_ExposureTime.Value;
            set => SetValue(() => nud_ExposureTime.Value = Clamp(value,
                nud_ExposureTime.Minimum, nud_ExposureTime.Maximum));
        }
        public decimal Gain
        {
              get => nud_Gain.Value;
              set => SetValue(() => nud_Gain.Value = Clamp(value,
                  nud_Gain.Minimum, nud_Gain.Maximum));

        }
        public decimal AcquisitionFrameRate
        {
            get => nud_AcquisitionFrameRate.Value;
            set => SetValue(() => nud_AcquisitionFrameRate.Value = Clamp(value,
                nud_AcquisitionFrameRate.Minimum, nud_AcquisitionFrameRate.Maximum));
        }
        /*    public decimal WhiteBalance
            {
                get => num_AcquisitionRate.Value;
                set => SetValue(() => num_AcquisitionRate.Value = Clamp(value,
                    num_AcquisitionRate.Minimum, num_AcquisitionRate.Maximum));
            }*/
        public string TriggerMode
        {
            get => cmb_TriggerMode.SelectedItem?.ToString();
            set => SetValue(() => SelectComboText(cmb_TriggerMode, value));
        }
         
        public string TriggerSource
        {
            get => cmb_TriggerSource.SelectedItem?.ToString();
            set => SetValue(() => SelectComboText(cmb_TriggerSource, value));
        }
      /*  public string Roi
        {
            get => txt_Rio.Text;
            set => SetValue(() => txt_Rio.Text = value);
        }*/
        public string PixelFormat
        {
            get => cmb_PixelFormat.SelectedItem?.ToString();
            set => SetValue(() => SelectComboText(cmb_PixelFormat, value));
        }
    
        /*  public decimal ReconnectCount
          {
              get => nud_ReConnection.Value;
              set => SetValue(() => nud_ReConnection.Value = Clamp(value,
                  nud_ReConnection.Minimum, nud_ReConnection.Maximum));
          }
          public bool AutoExposure
          {
              get => chk_AutoExposureDefault.Checked;
              set => SetValue(() => chk_AutoExposureDefault.Checked = value);
          }*/
        //统一控件值变化绑定事件
        private void BindInternalEvents()
        {
            nud_ExposureTime.ValueChanged += (s, e) => 
            RaiseParam(nameof(ExposureTime), ExposureTime);

            nud_Gain.ValueChanged += (s, e) => 
            RaiseParam(nameof(Gain), Gain);

            nud_AcquisitionFrameRate.ValueChanged += (s, e) =>
            RaiseParam(nameof(AcquisitionFrameRate), AcquisitionFrameRate);

            /*  num_AcquisitionRate.ValueChanged += (s, e) =>
              RaiseParam(nameof(WhiteBalance), WhiteBalance);*/

            cmb_TriggerMode.SelectedIndexChanged += (s, e) =>
            RaiseParam(nameof(TriggerMode), TriggerMode);

            cmb_TriggerSource.SelectedIndexChanged += (s, e) =>
            RaiseParam(nameof(TriggerSource), TriggerSource);
/*
            txt_Rio.TextChanged += (s, e) =>
            RaiseParam(nameof(Roi), Roi);*/

            cmb_PixelFormat.SelectedIndexChanged += (s, e) =>
            RaiseParam(nameof(PixelFormat), PixelFormat);

          /*  nud_ReConnection.ValueChanged += (s, e) =>
            RaiseParam(nameof(ReconnectCount), ReconnectCount);*/
        }

        
        // ==================== 参数规格应用（范围 / 步长 / 可选项） ====================

        /// <summary>
        /// 本面板支持调节的参数节点名。
        /// 【职责】由视图声明自己关心哪些参数，上层据此回读规格与当前值，避免两侧各自硬编码清单。
        /// </summary>
        public IReadOnlyList<string> SupportedParamNames { get; } = new List<string>
        {
            "ExposureTime",
            "Gain",
            "AcquisitionFrameRate",
            "PixelFormat",
            "TriggerMode",
            "TriggerSource"
        };

        /// <summary>
        /// 应用参数规格：把相机上报的范围/步长/可选项落实到对应控件，实现界面限幅。
        /// 【职责】视图只负责控件呈现，规格来源与合法性规则由上层的实体缓存提供。
        /// </summary>
        /// <param name="spec">参数规格；为空时忽略</param>
        public void ApplyParamSpec(ParamSpec spec)
        {
            if (spec == null || string.IsNullOrEmpty(spec.ParamName)) return;
            if (!IsHandleCreated) return;

            // 规格可能来自后台回读线程，控件属性修改必须回到 UI 线程
            if (InvokeRequired)
            {
                Invoke(new Action(() => ApplyParamSpec(spec)));
                return;
            }

            switch (spec.ParamName)
            {
                case "ExposureTime":
                    ApplyNumericSpec(nud_ExposureTime, spec);
                    break;
                case "Gain":
                    ApplyNumericSpec(nud_Gain, spec);
                    break;
                case "AcquisitionFrameRate":
                    ApplyNumericSpec(nud_AcquisitionFrameRate, spec);
                    break;
                case "PixelFormat":
                    ApplyComboSpec(cmb_PixelFormat, spec);
                    break;
                case "TriggerMode":
                    ApplyComboSpec(cmb_TriggerMode, spec);
                    break;
                case "TriggerSource":
                    ApplyComboSpec(cmb_TriggerSource, spec);
                    break;
            }
        }

        /// <summary>
        /// 按参数节点名回填当前值（走控件范围限幅，且不触发改参事件）
        /// </summary>
        /// <param name="paramName">参数节点名</param>
        /// <param name="value">参数当前值</param>
        public void ApplyParamValue(string paramName, object value)
        {
            if (string.IsNullOrEmpty(paramName) || value == null) return;

            switch (paramName)
            {
                case "ExposureTime":
                    ExposureTime = ToDecimal(value, ExposureTime);
                    break;
                case "Gain":
                    Gain = ToDecimal(value, Gain);
                    break;
                case "AcquisitionFrameRate":
                    AcquisitionFrameRate = ToDecimal(value, AcquisitionFrameRate);
                    break;
                case "PixelFormat":
                    PixelFormat = value.ToString();
                    break;
                case "TriggerMode":
                    TriggerMode = value.ToString();
                    break;
                case "TriggerSource":
                    TriggerSource = value.ToString();
                    break;
            }
        }

        /// <summary>按规格设置数值控件的范围与步长</summary>
        private static void ApplyNumericSpec(NumericUpDown control, ParamSpec spec)
        {
            if (control == null || !spec.IsNumeric || !spec.HasRange) return;

            var newMin = (decimal)spec.Min;
            var newMax = (decimal)spec.Max;
            if (newMin < 0 || newMin > newMax) return;

            // 先放宽再收紧，避免中间态出现 Min > Max 而抛异常
            control.Minimum = Math.Min(control.Minimum, newMin);
            control.Maximum = Math.Max(control.Maximum, newMax);
            control.Minimum = newMin;
            control.Maximum = newMax;

            if (control.Value < control.Minimum) control.Value = control.Minimum;
            else if (control.Value > control.Maximum) control.Value = control.Maximum;

            // 相机声明的步长才是界面步进的依据；步长不得超过取值范围
            if (spec.Step > 0 && (decimal)spec.Step <= newMax - newMin)
                control.Increment = (decimal)spec.Step;
        }

        /// <summary>按规格填充下拉框可选项</summary>
        private static void ApplyComboSpec(ComboBox combo, ParamSpec spec)
        {
            if (combo == null || spec.Options == null || spec.Options.Count == 0) return;

            combo.Items.Clear();
            foreach (var option in spec.Options)
            {
                combo.Items.Add(option);
            }
        }

        /// <summary>把参数值转换为 decimal；无法转换时沿用兜底值，避免回填中断</summary>
        private static decimal ToDecimal(object value, decimal fallback)
        {
            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                return fallback;
            }
        }

        public void ShowMessage(string message) { MessageBox.Show(this,message); }
      

        //设置控件启用状态,线程安全
        public void SetEnableState(bool enable)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetEnableState(enable)));
                return;
            }
            foreach (Control control in this.Controls)
            {
                control.Enabled = enable;
            }
        }
        //触发参数更改事件
        private void RaiseParam(string nodeName,object value)
        {
            if (_suppress) return;
            ParamChanged?.Invoke(this, new ParamChangedEventArgs(nodeName, value));
        }
        //设置控件值时，避免触发事件
        private void SetValue(Action action)
        {
            // 参数回读可能发生在连接/取流的后台线程，必须切回 UI 线程才能真实刷新界面
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetValue(action)));
                return;
            }

            _suppress = true;
            try
            {
                action();

            }
            finally
            {
                _suppress = false;
            }
        }
        //把相机回读的枚举符号名填入下拉框：项目缺失时先补齐再选中，避免赋值被静默丢弃
        private static void SelectComboText(ComboBox combo, string text)
        {
            if (combo == null) return;

            if (string.IsNullOrEmpty(text))
            {
                combo.SelectedIndex = -1;
                return;
            }

            int index = combo.FindStringExact(text);
            if (index < 0)
            {
                // 相机支持的枚举项不一定在设计器预置，动态补齐以保证回填可见
                index = combo.Items.Add(text);
            }
            combo.SelectedIndex = index;
        }
        //限制数值范围
        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
