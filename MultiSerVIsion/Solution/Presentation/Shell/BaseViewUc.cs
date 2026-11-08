using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Presentation.Shell
{
    /// <summary>
    /// 所有页面级用户控件的基类：统一承载「视图切换钩子」与「视觉样式规范」。
    /// 【视觉规范】字体分三级、控件统一字体、标题左对齐、容器内边距与间距统一，
    /// 由 <see cref="ApplyVisualStyle"/> 在加载时一次性下发，避免各 Designer 各写一套字号。
    /// </summary>
    public partial class BaseViewUc : UserControl
    {
        /// <summary>全局统一字体族（中文界面优先使用微软雅黑，缺省自动回退）</summary>
        public const string FontFamilyName = "微软雅黑";

        /// <summary>全局内边距（容器内容与边框的距离）</summary>
        public const int GlobalPadding = 8;

        /// <summary>控件间距（同级控件之间）</summary>
        public const int ControlMargin = 6;

        /// <summary>标准行高（表格式布局的行高基准）</summary>
        public const int StandardRowHeight = 28;

        /// <summary>一级标题字体：页面主标题</summary>
        public static readonly Font TitleFont =
            new Font(FontFamilyName, 12F, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>二级标题字体：分组框 / 区块标题</summary>
        public static readonly Font SectionFont =
            new Font(FontFamilyName, 10F, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>正文字体：标签、输入控件、列表等统一使用</summary>
        public static readonly Font BodyFont =
            new Font(FontFamilyName, 9F, FontStyle.Regular, GraphicsUnit.Point);

        [Description("UC切换显示触发")]
        public event EventHandler ViewShow;

        [Description("UC隐藏切换触发")]
        public event EventHandler ViewHide;

        public BaseViewUc()
        {
            InitializeComponent();
            this.Padding = new Padding(GlobalPadding);
            this.SetStyle(ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// 加载完成后统一下发视觉样式。
        /// 【时机说明】派生类的 InitializeComponent 在本基类构造之后执行，
        /// 因此样式必须在 OnLoad（控件树构建完成）时应用，构造函数中应用会漏掉子控件。
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyVisualStyle(this);
        }

        /// <summary>
        /// 递归应用视觉样式：字体分级 + 文本对齐 + 容器间距。
        /// 【幂等】重复调用结果一致，可安全在 OnLoad 多次触发时执行。
        /// </summary>
        /// <param name="root">样式作用范围的根控件</param>
        public static void ApplyVisualStyle(Control root)
        {
            if (root == null) return;

            foreach (Control child in root.Controls)
            {
                ApplyControlStyle(child);

                // 递归下发；容器自身的 Padding/Margin 已在 ApplyControlStyle 内统一
                if (child.HasChildren) ApplyVisualStyle(child);
            }
        }

        /// <summary>
        /// 按控件类型/命名套用字体级别与对齐方式。
        /// 【分级规则】分组框与标题类标签用二级/一级标题字体，其余控件统一正文字体；
        /// 不参与排版的容器（如 SplitContainer 自身）不改字体，避免影响其子控件测量。
        /// </summary>
        /// <param name="control">待样式化的控件</param>
        private static void ApplyControlStyle(Control control)
        {
            if (control == null) return;

            // 容器类：统一内边距与控件间距，保证各页留白一致
            if (control is GroupBox)
            {
                control.Padding = new Padding(GlobalPadding, GlobalPadding, GlobalPadding, GlobalPadding);
                control.Margin = new Padding(ControlMargin);
            }
            else if (control is Panel || control is TableLayoutPanel || control is FlowLayoutPanel)
            {
                control.Margin = new Padding(0);
            }

            // 字体分级
            if (control is GroupBox)
            {
                control.Font = SectionFont;
            }
            else if (control is Label label)
            {
                label.Font = IsTitleLabel(label.Name) ? TitleFont : BodyFont;

                // 自适应尺寸的标签由布局决定宽度，手动设置对齐会失效，故仅处理固定宽度标签
                if (!label.AutoSize)
                    label.TextAlign = ContentAlignment.MiddleLeft;
            }
            else if (control is ToolStrip)
            {
                control.Font = BodyFont;
            }
            else if (control is SplitContainer || control is TabControl || control is SplitterPanel)
            {
                // 纯布局容器：保持设计器字体，避免改变子控件测量基准
            }
            else
            {
                control.Font = BodyFont;
            }
        }

        /// <summary>
        /// 判断标签是否为标题类标签（按命名约定识别，避免逐页列举控件名）
        /// </summary>
        /// <param name="name">控件名</param>
        /// <returns>名称含 title 时视为标题标签</returns>
        private static bool IsTitleLabel(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.IndexOf("title", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>视图切换钩子：切到本页时由宿主调用</summary>
        public virtual void OnViewShow()
        {
            ViewShow?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>视图切换钩子：切走本页时由宿主调用</summary>
        public virtual void OnViewHide()
        {
            ViewHide?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 占位内容初始化钩子：由具体页面覆写以填充演示/空闲态内容。
        /// </summary>
        public virtual void SetUIPlaceholder()
        {
        }
    }
}
