using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MultiSerVIsion.Solution.Presentation.UserControls
{
    public partial class UCVisionView : BaseViewUc,IVisionView
    {
        public event Action StartGrabRequested;
        public event Action StopGrabRequested;

        /// <summary>取流按钮点击 → 请求连续采集</summary>
        public event Action OnStartStreamClicked;
        /// <summary>停止按钮点击 → 请求停止取流</summary>
        public event Action OnStopStreamClicked;

        /// <summary>单步（软触发）按钮（代码创建，不依赖设计器控件命名）</summary>
        private ToolStripButton _btnSoftTrigger;

        /// <summary>当前设备回填的参数规格（供输入限幅使用）</summary>
        private IReadOnlyList<CameraParameterSpec> _parameterSpecs = new List<CameraParameterSpec>();

        /// <summary>抓拍图片保存目录（应用目录下 Capture）</summary>
        private static readonly string CaptureFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Capture");

        public UCVisionView()
        {
            InitializeComponent();
            InitializeStreamControls();
            SetGrabButtonsEnabled(canStart: true, canStop: false);
        }

        /// <summary>
        /// 初始化采集模式下拉与单步按钮。
        /// 【说明】采用代码方式创建/绑定而非设计器，避免手工改动 Designer 生成代码后被覆盖。
        /// </summary>
        private void InitializeStreamControls()
        {
            // 导航栏保持简单：取流默认连续采集，单步按钮控制软触发
            _btnSoftTrigger = new ToolStripButton
            {
                Name = "btn_Capture",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Text = "抓拍",
                Enabled = true
            };
            _btnSoftTrigger.Click += btn_SoftTrigger_Click;
            toolStrip1.Items.Add(_btnSoftTrigger);

            // 三按钮文案：启动 / 暂停 / 抓拍
            btn_StartGrab.Text = "启动";
            btn_StopGrab.Text = "暂停";
        }
        
        public override void OnViewShow()
        {
            base.OnViewShow();
        }
        public override void SetUIPlaceholder()
        {
          

        }
        // 最新待渲染帧与排队标志：同一时刻只保留 1 帧排队，实现「丢帧保实时」
        private Bitmap _pendingFrame;
        private int _renderQueued;

        public void UpdateFrame(Bitmap frame)
        {
            if (frame == null) return;

            // 控件句柄未创建时直接丢弃帧，避免 Invoke/BeginInvoke 抛异常（控件尚未显示时会出现）
            if (!PitCamera1.IsHandleCreated)
            {
                frame.Dispose();
                return;
            }

            // 用最新帧替换尚未渲染的旧帧，避免 UI 线程排队堆积造成显示延迟与频率抖动
            var stale = System.Threading.Interlocked.Exchange(ref _pendingFrame, frame);
            stale?.Dispose();

            // 已有渲染任务排队时直接返回，UI 线程会取走最新帧
            if (System.Threading.Interlocked.CompareExchange(ref _renderQueued, 1, 0) != 0)
                return;

            PitCamera1.BeginInvoke(new Action(RenderPendingFrame));
        }

        /// <summary>在 UI 线程渲染最新待显示帧（由 UpdateFrame 排队触发）</summary>
        private void RenderPendingFrame()
        {
            // 先放开排队标志，保证后续帧能继续投递
            System.Threading.Interlocked.Exchange(ref _renderQueued, 0);

            var frame = System.Threading.Interlocked.Exchange(ref _pendingFrame, null);
            if (frame == null) return;

            var oldImg = PitCamera1.Image;
            PitCamera1.Image = frame;
            oldImg?.Dispose();
        }
        public void UpdateRunInfo(string info)
        {
            if (!IsHandleCreated) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateRunInfo(info)));
                return;
            }
            // 帧率/分辨率等信息显示在底部状态条首个标签
            toolStripLabel1.Text = info;
        }
        public void SetGrabButtonsEnabled(bool canStart, bool canStop)
        {
            btn_StartGrab.Enabled = canStart;
            btn_StopGrab.Enabled = canStop;
        }
        public void ShowMessage(string message, bool isError = false)
        {
            MessageBox.Show(message, isError ? "错误" : "提示",
                MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }
        public void ClearDisplay()
        {
            // 句柄未创建时无需清空，直接返回避免异常
            if (!PitCamera1.IsHandleCreated) return;

            if (PitCamera1.InvokeRequired)
            {
                PitCamera1.BeginInvoke(new Action(ClearDisplay));
                return;
            }

            // 丢弃尚未渲染的帧，避免清空后又被旧帧覆盖
            System.Threading.Interlocked.Exchange(ref _pendingFrame, null)?.Dispose();

            PitCamera1.Image?.Dispose();
            PitCamera1.Image = null;
           /* lbl_RunInfo.Text = "未采集";*/
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            StopGrabRequested?.Invoke();
            base.OnHandleDestroyed(e);
        }

        // ==================== 采集模式 / 三按钮联动 ====================

        /// <summary>
        /// 抓拍按钮点击：把当前画面保存为图片文件，不再做软触发。
        /// 【说明】保存动作留在视图层，因为它依赖当前显示内容，不涉及业务规则。
        /// </summary>
        private void btn_SoftTrigger_Click(object sender, EventArgs e)
        {
            var image = PitCamera1.Image as Bitmap;
            if (image == null)
            {
                ShowMessage("当前没有可抓拍的图像", isError: true);
                return;
            }

            try
            {
                if (!Directory.Exists(CaptureFolder))
                    Directory.CreateDirectory(CaptureFolder);

                // 时间戳命名，避免重名覆盖；PNG 无损便于后续算法读取
                string fileName = $"CAP_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
                string fullPath = Path.Combine(CaptureFolder, fileName);

                // 复制一份再保存，避免与渲染线程争用同一张位图
                using (var snapshot = new Bitmap(image))
                {
                    snapshot.Save(fullPath, ImageFormat.Png);
                }

                ShowMessage($"已保存：{fileName}");
            }
            catch (Exception ex)
            {
                ShowMessage($"抓拍保存失败：{ex.Message}", isError: true);
            }
        }

        /// <summary>
        /// 按参数规格刷新参数配置区：缓存规格供输入限幅与提示使用。
        /// </summary>
        /// <param name="specs">参数规格集合</param>
        public void ShowParameterSpecs(IReadOnlyList<CameraParameterSpec> specs)
        {
            _parameterSpecs = specs ?? new List<CameraParameterSpec>();
        }

        /// <summary>
        /// 按规格对输入值做限幅（数值型按 Min/Max/Step 对齐），供文本输入 Validating 兼底调用。
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="rawValue">原始输入</param>
        /// <returns>限幅后的安全值；无对应规格或非数值时原样返回</returns>
        public string ClampParameterInput(string parameterName, string rawValue)
        {
            if (_parameterSpecs == null) return rawValue;

            foreach (var spec in _parameterSpecs)
            {
                if (!string.Equals(spec.Name, parameterName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!spec.IsNumeric) return rawValue;
                if (!double.TryParse(rawValue, out var numeric)) return rawValue;

                double result = numeric;
                if (spec.Min.HasValue && result < spec.Min.Value) result = spec.Min.Value;
                if (spec.Max.HasValue && result > spec.Max.Value) result = spec.Max.Value;

                if (spec.Step.HasValue && spec.Step.Value > 0)
                {
                    double baseValue = spec.Min ?? 0;
                    result = baseValue + Math.Round((result - baseValue) / spec.Step.Value) * spec.Step.Value;

                    // 对齐后可能又越界，二次夹取保证落在合法区间
                    if (spec.Min.HasValue && result < spec.Min.Value) result = spec.Min.Value;
                    if (spec.Max.HasValue && result > spec.Max.Value) result = spec.Max.Value;
                }

                return spec.ValueType == ParameterValueType.Int
                    ? Math.Round(result).ToString()
                    : result.ToString();
            }

            return rawValue;
        }

        private void btn_StopGrab_Click(object sender, EventArgs e)
        {
            StopGrabRequested?.Invoke();
            OnStopStreamClicked?.Invoke();
        }

        private void btn_StartGrab_Click(object sender, EventArgs e)
        {
            StartGrabRequested?.Invoke();
            OnStartStreamClicked?.Invoke();
        }
    }
}
