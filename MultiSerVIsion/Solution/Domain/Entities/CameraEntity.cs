using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Shared.Models;
using MvCamCtrl.NET;
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

        public MyCamera CameraHandle { get; set; } = null;
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
