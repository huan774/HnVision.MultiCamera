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
        public event Action SaveConfigRequest;
        
        public CameraDateilUC()
        {
            InitializeComponent();
        }

       
        public void ShowMessage(string message) { MessageBox.Show(this,message); }
        public bool ShowConfirmDialog(string  message) { return MessageBox.Show(this,message,"提示",MessageBoxButtons.YesNo)==DialogResult.Yes; }
/*
        private void RaiseParam(string nodeName,object value)
        {
            if (_suppress) return;
            ParamChanged?.Invoke(this, new ParamChangedEventArgs(nodeName, value));
        }
        private void SetValue(Action action)
        {
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
        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

*/
    }
}
