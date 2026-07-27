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
    public partial class BaseViewUc : UserControl
    {

        public const int GlobalPadding = 8;
        public const int ControlMargin = 6;
        public const int StandardRowHeight = 28;

        [Description("UC切换显示触发")]
        public event EventHandler ViewShow;

        [Description("UC隐藏切换触发")]
        public event EventHandler ViewHide;

        [Description("UC销毁释放触发")]
        public event EventHandler ViewDestroy;
        public BaseViewUc()
        {
            InitializeComponent();
            this.Padding=new Padding(1);
            this.SetStyle(ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint|
                ControlStyles.OptimizedDoubleBuffer,true); 
            
        }

        public virtual void OnViewShow()
        {
            ViewShow?.Invoke(this, EventArgs.Empty);
        }
        public virtual void OnViewHide()
        {
           ViewHide?.Invoke(this, EventArgs.Empty);
        }
        public virtual void OnViewDestroy()
        {
            ViewDestroy?.Invoke(this, EventArgs.Empty);
            this.Controls.Clear();
        }
        public virtual void SetUIPlaceholder()
        {

        }
    }
}
