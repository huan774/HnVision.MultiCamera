using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Entities
{
    [JsonDerivedType(typeof(CameraEntity), typeDiscriminator: "Camera")]
    /*[JsonDerivedType(typeof(MotionDeviceEntity), typeDiscriminator: "MotionCard")]*/
    public abstract  class DeviceEntity
    {
        public string DeviceId {  get; set; }=string.Empty;
        public string DeviceName { get; set; } = string.Empty;

        public string GroupTage { get; set; } = string.Empty;
        public string IpAddress {  get; set; }= string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public bool IsEnable { get; set; } = true;

        [JsonIgnore]
        public int Handle { get;  set; } = -1;

        /// <summary>
        /// 通用连接状态（设备基类层面，所有设备通用）。
        /// 【职责】由应用层在连接/断开时统一维护；仅内存态，不序列化到 JSON。
        /// 与 DeviceContext 会话状态、CameraEntity.DetailStatus 相机详细状态分工不同：
        /// 此处描述「设备是否已连接」的通用标志，不承载 UI 会话上下文。
        /// </summary>
        [JsonIgnore]
        public DeviceConnectionStatue ConnectionStatus { get; set; } = DeviceConnectionStatue.Disconnected;

        public abstract DeviceEntity ShallowClone();

        protected void CopyBaseFieldTo(DeviceEntity entity)
        {
            entity.DeviceId = this.DeviceId;
            entity.DeviceName = this.DeviceName;
            entity.GroupTage = this.GroupTage;
            entity.IpAddress = this.IpAddress;
            entity.DeviceType = this.DeviceType;
            entity.IsEnable = this.IsEnable;
        }

        /// <summary>
        /// 设备自校验：串行执行「基础规则 → 子类专属规则」的完整校验。
        /// 【调用约定】只允许「用户表单输入」入口调用一次（如应用层新增/编辑设备），
        /// 严禁在仓储或设备管理器内部再次调用，否则同一实体将被重复校验；
        /// 扫描导入等来源于硬件真值的设备不属于表单输入，应跳过本校验。
        /// </summary>
        /// <returns>校验结果；失败时 Message 为具体原因</returns>
        public Shared.Models.ValidationResult SelfValidate()
        {
            // 先校验所有设备通用的基础规则；不通过直接短路，避免无谓的子类校验
            var baseCheck = ValidateRule();
            if (!baseCheck.IsValid)
                return baseCheck;

            // 基础规则通过后，再执行子类专属规则
            return ValidateDeviceSpecialRule();
        }

        // 基础规则：设备名、通信地址、所属分组。仅供 SelfValidate 单次调用，外部不得直接调用
        private Shared.Models.ValidationResult ValidateRule()
        {
            if (string.IsNullOrWhiteSpace(DeviceName) || DeviceName.Length > 64)
                return Shared.Models.ValidationResult.Failure("设备长度不能为空且长度≤64");
            if (!CheckIpFormat(IpAddress))
                return Shared.Models.ValidationResult.Failure("IP格式非法");
            if (string.IsNullOrWhiteSpace(GroupTage))
                return Shared.Models.ValidationResult.Failure("必须选择设备分组");
            return Shared.Models.ValidationResult.Success();
        }

        /// <summary>子类专属规则校验：由各设备类型实现（如相机校验曝光参数）</summary>
        /// <returns>校验结果；失败时 Message 为具体原因</returns>
        protected abstract Shared.Models.ValidationResult ValidateDeviceSpecialRule();

        private bool CheckIpFormat(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;
            var arr = ip.Split('.');
            if (arr.Length != 4) return false;
            return arr.All(x => byte.TryParse(x, out var num) && num <= 255);
        }
    }
}
