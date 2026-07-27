using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Repositories;
using MvCamCtrl.NET;
using MvCameraControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    public class HikCameraHardwareDriver:ICameraHardwareDriver
    {
       /* private readonly MyCamera.MV_CC_DEVICE_INFO_LIST devList = new MyCamera.MV_CC_DEVICE_INFO_LIST();*/
        private readonly MyCamera m_MyCamera = new MyCamera();
        private bool _sdkInited = false;

        private void InitSdkIfNot()
        {
            if (_sdkInited) return;

            var code = MyCamera.MV_CC_Initialize_NET();
            if (code != MyCamera.MV_OK)
                throw new HardwareException($"海康SDK初始化失败，错误码{code}",code);

            _sdkInited = true; 
        }

        public void Relese()
        {
            if( _sdkInited) return;
            MyCamera.MV_CC_Finalize_NET();
            _sdkInited= false;
        }

        public async Task<List<CameraHardwareRawDto>> ScanAllCameraAsync()
        {
            return await Task.Run(() =>
            {
                InitSdkIfNot();
                List<CameraHardwareRawDto> result = new List<CameraHardwareRawDto>();

                MyCamera.MV_CC_DEVICE_INFO_LIST devList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                int ret = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE |
                    MyCamera.MV_USB_DEVICE, ref devList);


                if (ret != MyCamera.MV_OK)
                    throw new HardwareException($"枚举相机失败，错误码{ret}",ret);

                for(uint i = 0; i < devList.nDeviceNum; i++)
                {
                    MyCamera.MV_CC_DEVICE_INFO stDevInfo=
                    (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(devList.pDeviceInfo[i],
                    typeof(MyCamera.MV_CC_DEVICE_INFO));

                    CameraHardwareRawDto domainInfo = ConverRawToDomainModel(stDevInfo);
                    result.Add(domainInfo);
                }
                return result;
            });
        }

        private CameraHardwareRawDto ConverRawToDomainModel(MyCamera.MV_CC_DEVICE_INFO raw)
        {
            CameraHardwareRawDto info = new CameraHardwareRawDto();

            if (raw.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                MyCamera.MV_GIGE_DEVICE_INFO_EX gigeInfo = (
                    MyCamera.MV_GIGE_DEVICE_INFO_EX)MyCamera.ByteToStruct(
                        raw.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO_EX));

                info.InterfaceType = "GigE";
                info.IpAddress = gigeInfo.nCurrentIp.ToString();
                info.SerialNumber = gigeInfo.chSerialNumber;
                info.ModelName = gigeInfo.chModelName;
                
            }
            else if (raw.nTLayerType == MyCamera.MV_USB_DEVICE)
            {
                MyCamera.MV_USB3_DEVICE_INFO_EX usbInfo = (
                    MyCamera.MV_USB3_DEVICE_INFO_EX)MyCamera.ByteToStruct(
                        raw.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO_EX));

                info.InterfaceType = "USB";
                info.SerialNumber = usbInfo.chSerialNumber.ToString();
                info.ModelName = usbInfo.chModelName;
                info.IpAddress = string.Empty;
            }
            return null /*info*/;
        }
        public class HardwareException : Exception
        { 
            public int ErrorCode { get; }
            public HardwareException(string msg, int errorCode) :base(msg)
            {
                ErrorCode = errorCode;
            }
        }


         public int Login(CameraConnectConfig connectParam)
          {
            /*  int handle = HikCamera.Login(connectParam.Ip, connectParam.Port, connectParam.UserName, connectParam.Password);
              if (handle <= 0)
              {
                  int errCode = HikCamera.GetLastError();
                  throw new Exception($"海康登录失败，错误码：{errCode}");
              }
*/
              return 1;
          }

        public void OpenStream(int sdkHandle)
        {
           /* bool success = HikCamera.StartStream(sdkHandle);
            if (!success)
            {
                int errCode = HikCamera.GetLastError();
                throw new Exception($"开启取流失败，错误码：{errCode}");
            }*/
        }

        public void CloseStream(int sdkHandle)
        {
           /* HikCamera.StopStream(sdkHandle);*/
        }

        public void Logout(int sdkHandle)
        {
           /* HikCamera.Logout(sdkHandle);*/
        }
    }
}
