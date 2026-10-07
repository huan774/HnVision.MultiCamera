using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Entities
{
    public class CameraEntity : DeviceEntity
    {
        public CameraAllConfig CameraAllConfig { get; set; }= new CameraAllConfig();

     
        /// <summary>
        /// 相机详细运行状态（相机特有：Idle/Connected/Streaming 等）。
        /// 【职责】由应用层（CameraAppService）在连接/断开/采流时更新，是相机领域状态的权威来源；
        /// UI 展示直接读取此字段。仅内存态，不序列化到 JSON。
        /// 与 DeviceContext 会话状态分工不同：此处是领域层真实状态，DeviceContext 只是当前选中设备的会话快照。
        /// </summary>
        [JsonIgnore]
        public CameraStatus DetailStatus { get; set; } = CameraStatus.Idle;
        public DateTime LastConnectTime {  get; set; }
        public override DeviceEntity ShallowClone()
        {
            var newCam=new CameraEntity();
            CopyBaseFieldTo(newCam);

            // 深拷贝配置，避免副本与原实体共享同一配置对象（改副本会连带改原实体）
            newCam.CameraAllConfig = this.CameraAllConfig.Clone();

            newCam.Handle = -1;
            newCam.ConnectionStatus = DeviceConnectionStatue.Disconnected;
            newCam.DetailStatus = CameraStatus.Idle;
            return newCam;
        }
        
        /// <summary>
        /// 参数权威校验：按实体缓存的参数规格校验取值是否合法。
        /// 【双层校验之一】与界面限幅复用同一份规格，避免两处规则分叉。
        /// </summary>
        /// <param name="paramName">参数节点名</param>
        /// <param name="value">待校验的取值</param>
        /// <returns>校验结果；非法时 Message 说明原因</returns>
        public ValidationResult ValidateParam(string paramName, object value)
        {
            if (CameraAllConfig == null) return ValidationResult.Success();
            return CameraAllConfig.ValidateParam(paramName, value);
        }

        protected override ValidationResult ValidateDeviceSpecialRule()
        {
            var paramConfig = CameraAllConfig.ParamConfig;
            if (paramConfig == null)
                return ValidationResult.Failure("相机参数配置不能为空");

            // 曝光时间：有缓存规格时按相机真实范围/步长校验；否则沿用最小可用性检查
            if (CameraAllConfig.FindSpec(CameraParamNames.ExposureTime) != null)
            {
                var exposureCheck = CameraAllConfig.ValidateParam(
                    CameraParamNames.ExposureTime, paramConfig.ExposureTime);
                if (!exposureCheck.IsValid) return exposureCheck;
            }
            else if (paramConfig.ExposureTime <= 0)
            {
                return ValidationResult.Failure("曝光需大于0");
            }

            return ValidationResult.Success();
        }
        public static CameraEntity CreateFormScanResult(CameraDeviceDto scanResult)
        {
            var cam=new CameraEntity();
            cam.DeviceId = Guid.NewGuid().ToString("N");
            cam.DeviceType = "Camera";
            cam.DeviceName = $"相机_{scanResult.SerialNumber}";
            cam.IpAddress = scanResult.IpAddress;
            cam.GroupTage = "相机分组";
            cam.IsEnable = true;

            cam.CameraAllConfig.ConnectConfig.SerialNumber = scanResult.SerialNumber;
            cam.CameraAllConfig.ConnectConfig.InterfaceType = scanResult.InterfaceType;
            
            return cam;
        }
    }
    public class CameraAllConfig
    {
        public CameraConnectConfig ConnectConfig { get;  set; }=new CameraConnectConfig();

        public CameraParamConfig ParamConfig { get; set; } = new CameraParamConfig();

        /// <summary>
        /// 参数规格缓存（范围/步长/可选项）。连接成功后由硬件层从相机回读并填充，
        /// 随实体一并持久化，使界面限幅与实体校验在未连接相机时仍然可用。
        /// </summary>
        public List<ParamSpec> ParamSpecs { get; set; } = new List<ParamSpec>();

        /// <summary>
        /// 按参数名查找已缓存的参数规格
        /// </summary>
        /// <param name="paramName">相机参数节点名</param>
        /// <returns>命中时返回规格，未缓存时返回 null</returns>
        public ParamSpec FindSpec(string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName)) return null;
            if (ParamSpecs == null) return null;

            return ParamSpecs.FirstOrDefault(
                s => string.Equals(s.ParamName, paramName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 新增或更新参数规格（按参数名匹配）
        /// </summary>
        /// <param name="spec">待写入的规格；为空时忽略</param>
        public void UpsertSpec(ParamSpec spec)
        {
            if (spec == null || string.IsNullOrWhiteSpace(spec.ParamName)) return;

            if (ParamSpecs == null) ParamSpecs = new List<ParamSpec>();

            int index = ParamSpecs.FindIndex(
                s => string.Equals(s.ParamName, spec.ParamName, StringComparison.OrdinalIgnoreCase));

            if (index >= 0) ParamSpecs[index] = spec;
            else ParamSpecs.Add(spec);
        }

        /// <summary>
        /// 按缓存的参数规格做权威校验（可写性 / 范围 / 步长 / 可选项）。
        /// 【说明】规格由连接成功后回读并随实体持久化；未缓存规格时放行（fail-open），
        /// 由相机节点自身的范围最终兜底，避免离线状态下误拦合法参数。
        /// </summary>
        /// <param name="paramName">参数节点名</param>
        /// <param name="value">待校验的取值</param>
        /// <returns>校验结果；非法时 Message 说明原因</returns>
        public ValidationResult ValidateParam(string paramName, object value)
        {
            var spec = FindSpec(paramName);
            if (spec == null) return ValidationResult.Success();

            var display = string.IsNullOrEmpty(spec.DisplayName) ? spec.ParamName : spec.DisplayName;

            if (!spec.IsWritable)
                return ValidationResult.Failure($"参数 {display} 为只读，不允许修改");

            if (spec.IsNumeric)
            {
                double numeric;
                try
                {
                    numeric = Convert.ToDouble(value);
                }
                catch
                {
                    return ValidationResult.Failure($"参数 {display} 需要数值");
                }

                if (!spec.IsInRange(numeric))
                    return ValidationResult.Failure($"参数 {display} 需在 {spec.Min}~{spec.Max}{spec.Unit} 范围内");

                if (!spec.IsOnStep(numeric))
                    return ValidationResult.Failure($"参数 {display} 需为步长 {spec.Step}{spec.Unit} 的整数倍");

                return ValidationResult.Success();
            }

            if (spec.ValueType == ParamValueType.Enum && spec.Options != null && spec.Options.Count > 0)
            {
                var text = value?.ToString();
                if (string.IsNullOrEmpty(text) || !spec.Options.Contains(text))
                    return ValidationResult.Failure($"参数 {display} 不支持取值 {text}");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// 深拷贝配置（连接参数 / 参数值 / 参数规格均为独立副本）
        /// </summary>
        /// <returns>独立副本</returns>
        public CameraAllConfig Clone()
        {
            return new CameraAllConfig
            {
                ConnectConfig = CloneConnectConfig(ConnectConfig),
                ParamConfig = CloneParamConfig(ParamConfig),
                ParamSpecs = ParamSpecs == null
                    ? new List<ParamSpec>()
                    : ParamSpecs.Select(s => s.Clone()).ToList()
            };
        }

        /// <summary>拷贝连接参数</summary>
        private static CameraConnectConfig CloneConnectConfig(CameraConnectConfig source)
        {
            if (source == null) return new CameraConnectConfig();

            return new CameraConnectConfig
            {
                SerialNumber = source.SerialNumber,
                IpAddress = source.IpAddress,
                Port = source.Port,
                InterfaceType = source.InterfaceType,
                Model = source.Model,
                ConnectTimeoutMs = source.ConnectTimeoutMs,
                UserName = source.UserName,
                Password = source.Password
            };
        }

        /// <summary>拷贝参数值</summary>
        private static CameraParamConfig CloneParamConfig(CameraParamConfig source)
        {
            if (source == null) return new CameraParamConfig();

            return new CameraParamConfig
            {
                ExposureTime = source.ExposureTime,
                Gain = source.Gain,
                PixelFormat = source.PixelFormat,
                FrameRate = source.FrameRate,
                TriggerMode = source.TriggerMode,
                TriggerSource = source.TriggerSource
            };
        }
    }
}
