using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;

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
        public int Handle { get; /*protected*/ set; } = -1;

        [JsonIgnore]
        public DeviceConnectionStatue ConnectionStatus { get; /*protected*/ set; } = DeviceConnectionStatue.Disconnected;

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

        public Shared.Models.ValidationResult SelfValidate()
        {
            var baseCheck = ValidateRule();
            if(!baseCheck.IsValid)
                return baseCheck;

            var chilCheck = ValidateDeviceSpecialRule();
            return chilCheck;
        }
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
