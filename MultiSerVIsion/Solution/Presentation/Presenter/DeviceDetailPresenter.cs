using MultiSerVIsion.Solution.Application.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Presentation.Views;
using System;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Presentation.Presenter
{
    /// <summary>
    /// 相机参数面板 Presenter：把「用户改参」与「选中设备回填」两条链路接到应用服务。
    /// 【职责边界】
    ///   1. 视图只负责采集输入并抛出 ParamChanged，业务动作一律由本 Presenter 发起；
    ///   2. 参数的实际读写由 <see cref="ICameraAppService"/> 下发到硬件层，本类不感知 SDK 细节；
    ///   3. 回填通过视图的规格应用与取值接口完成，视图内部抑制事件，天然避免「回填又触发下发」的循环。
    /// 【参数名约定】视图抛出的 NodeName 即相机参数节点名（如 ExposureTime / Gain / PixelFormat），
    /// 因此可直接透传给应用服务，无需在表现层再做一层名称映射。
    /// </summary>
    public class DeviceDetailPresenter : BasePresenter
    {
        private readonly IEventBus _eventBus;
        private readonly IDeviceDatailView _datailView;
        private readonly ICameraAppService _cameraAppService;

        /// <summary>当前面板所对应的组态设备 ID（由选中事件驱动）</summary>
        private string _currentDeviceId;

        /// <summary>回填过程中置位，避免回填动作被误判为用户改参</summary>
        private bool _isRefreshing;

        /// <summary>
        /// 构造参数面板 Presenter
        /// </summary>
        /// <param name="cameraAppService">相机应用服务（参数读写入口）</param>
        /// <param name="detailView">参数面板视图</param>
        /// <param name="eventBus">全局事件总线</param>
        public DeviceDetailPresenter(
            ICameraAppService cameraAppService,
            IDeviceDatailView detailView,
            IEventBus eventBus)
        {
            _cameraAppService = cameraAppService;
            _datailView = detailView;
            _eventBus = eventBus;
        }

        /// <summary>
        /// 初始化：挂接视图改参事件，并订阅设备选中/清空事件以驱动参数回填。
        /// </summary>
        public override void Init()
        {
            _datailView.ParamChanged += OnParamChanged;
            _eventBus.Subscribe<ConfigDeviceSelectedEvent>(OnConfigDeviceSelected);
            _eventBus.Subscribe<DeviceSelectionClearedEvent>(OnSelectionCleared);
        }

        /// <summary>
        /// 用户改参：把新值下发到相机。
        /// 【说明】写入以「改参即生效」为原则；失败时提示但保留用户输入，便于其修正后重试。
        /// </summary>
        private async void OnParamChanged(object sender, ParamChangedEventArgs e)
        {
            if (_isRefreshing) return;
            if (e == null || string.IsNullOrWhiteSpace(e.NodeName)) return;
            if (string.IsNullOrEmpty(_currentDeviceId))
            {
                _datailView.ShowMessage("未选中设备，参数未下发");
                return;
            }

            var value = ToDriverValue(e.Value);
            if (value == null)
            {
                _datailView.ShowMessage($"参数 {e.NodeName} 的值类型不支持下发");
                return;
            }

            try
            {
                var result = await _cameraAppService.SetParamAsync(_currentDeviceId, e.NodeName, value);
                if (!result.Success)
                {
                    // 参数不被相机支持或超出节点范围时，把相机返回的原因如实呈现
                    _datailView.ShowMessage($"参数下发失败：{result.Message}");
                }
            }
            catch (Exception ex)
            {
                _datailView.ShowMessage($"参数下发异常：{ex.Message}");
            }
        }

        /// <summary>选中组态设备：切换当前设备并回填其参数</summary>
        private async void OnConfigDeviceSelected(ConfigDeviceSelectedEvent e)
        {
            _currentDeviceId = e?.DeviceId;
            if (string.IsNullOrEmpty(_currentDeviceId)) return;

            await LoadDeviceParamsAsync(_currentDeviceId);
        }

        /// <summary>清空选中：断开与本设备的绑定，避免参数下发到已取消选中的设备</summary>
        private void OnSelectionCleared(DeviceSelectionClearedEvent e)
        {
            _currentDeviceId = null;
        }

        /// <summary>
        /// 读取参数规格与当前值并回填到视图。
        /// 【顺序】先应用规格（范围/步长/可选项）再回填当前值，
        /// 这样取值时即可被控件范围限幅，避免「值超出控件范围」而回填失败。
        /// 【参数清单】由视图自行声明支持的参数，避免表现层硬编码参数节点名。
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public async Task LoadDeviceParamsAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return;

            _isRefreshing = true;
            try
            {
                foreach (var paramName in _datailView.SupportedParamNames)
                {
                    var readResult = await _cameraAppService.ReadParamAsync(deviceId, paramName);
                    if (!readResult.Success || readResult.Data == null) continue;

                    // 单个参数读取失败不影响其余参数，继续回填
                    _datailView.ApplyParamSpec(readResult.Data.Spec);
                    if (readResult.Data.Value != null)
                        _datailView.ApplyParamValue(paramName, readResult.Data.Value);
                }
            }
            catch (Exception ex)
            {
                _datailView.ShowMessage($"参数回填异常：{ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        /// <summary>
        /// 把视图抛出的值转换为硬件层可接收的类型。
        /// 【说明】视图数值控件统一产出 decimal，需转成 double 才能走 SDK 浮点写入；
        /// 空字符串（下拉框未选中）视为无效值，避免把空值写进相机。
        /// </summary>
        /// <param name="rawValue">视图抛出的原始值</param>
        /// <returns>可下发的值；无法下发时返回 null</returns>
        private static object ToDriverValue(object rawValue)
        {
            if (rawValue == null) return null;

            if (rawValue is bool boolValue) return boolValue;

            if (rawValue is decimal decimalValue) return (double)decimalValue;

            if (rawValue is string text)
                return string.IsNullOrWhiteSpace(text) ? null : text;

            // 其余类型（如 int / double）可直接下发
            return rawValue;
        }

        /// <summary>释放：退订视图与事件总线订阅，避免事件持有 Presenter</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _datailView.ParamChanged -= OnParamChanged;
                _eventBus.Unsubscribe<ConfigDeviceSelectedEvent>(OnConfigDeviceSelected);
                _eventBus.Unsubscribe<DeviceSelectionClearedEvent>(OnSelectionCleared);
            }
            base.Dispose(disposing);
        }
    }
}
