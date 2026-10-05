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

            newCam.CameraAllConfig = this.CameraAllConfig;

            newCam.Handle = -1;
            newCam.ConnectionStatus = DeviceConnectionStatue.Disconnected;
            newCam.DetailStatus = CameraStatus.Idle;
            return newCam;
        }
        
        protected override ValidationResult ValidateDeviceSpecialRule()
        {
            var connectParam=CameraAllConfig.ParamConfig;

            if (connectParam.ExposureTime <= 0)
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
    }
}
