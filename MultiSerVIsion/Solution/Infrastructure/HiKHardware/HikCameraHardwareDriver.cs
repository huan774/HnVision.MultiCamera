using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Presentation.UserControls;
using MvCamCtrl.NET;
using MvCameraControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using static MvCamCtrl.NET.MyCamera;

namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    public class HikCameraHardwareDriver : ICameraHardwareDriver
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private const int MV_OK = 0;

        private const int ERR_PARAM_NULL = -1001;
        private const int ERR_PARAM_EMPTY = -1002;
        private const int ERR_INVALID_HANDLE = -1003;
        private const int ERR_EXCEPTION = -9999;

        // 采集字段
        private Thread _grabThread;
        private volatile bool _isGrabbing = false;
        private IntPtr _convertBuffer = IntPtr.Zero;
        private uint _convertBufferSize = 0;
        private Bitmap _currentBitmap;
        private PixelFormat _pixelFormat;

        private readonly MyCamera m_MyCamera = new MyCamera();
        private bool _sdkInited = false;

        public event EventHandler<CameraFrameEventArgs> FrameReceived;
        
        MyCamera.MV_CC_DEVICE_INFO_LIST devList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        private static readonly Dictionary<string, MV_CC_DEVICE_INFO> _scannedDeviceInfoCache = new Dictionary<string, MV_CC_DEVICE_INFO>();
        private static readonly object _cacheLock = new object();
        public class CameraFrameEventArgs : EventArgs
        {
            public string DeviceId { get; }
            /// <summary>转换后的标准位图（可直接绑定UI显示）</summary>
            public Bitmap Frame { get; }
            /// <summary>图像宽度</summary>
            public int Width { get; }
            /// <summary>图像高度</summary>
            public int Height { get; }
            /// <summary>帧序号</summary>
            public ulong FrameId { get; }
            /// <summary>时间戳（微秒）</summary>
            public ulong Timestamp { get; }
            public CameraFrameEventArgs(string devId,Bitmap frame, int width, int height, ulong frameId, ulong timestamp)
            {
                DeviceId = devId;
                Frame = frame;
                Width = width;
                Height = height;
                FrameId = frameId;
                Timestamp = timestamp;
            }
        }
        private void InitSdkIfNot()
        {
            if (_sdkInited) return;

            var code = MyCamera.MV_CC_Initialize_NET();
            if (code != MyCamera.MV_OK)
                throw new HardwareException($"海康SDK初始化失败，错误码{code}", code);

            _sdkInited = true;
        }

        public void Relese()
        {
            if (_sdkInited) return;
            MyCamera.MV_CC_Finalize_NET();
            _sdkInited = false;
        }

        public async Task<List<CameraHardwareRawDto>> ScanAllCameraAsync()
        {
            return await Task.Run(() =>
            {
                InitSdkIfNot();
                List<CameraHardwareRawDto> result = new List<CameraHardwareRawDto>();

                lock (_cacheLock)
                {
                    _scannedDeviceInfoCache.Clear();

                    int ret = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE |
                        MyCamera.MV_USB_DEVICE, ref devList);

                    if (ret != MyCamera.MV_OK)
                        throw new HardwareException($"枚举相机失败，错误码{ret}", ret);

                    for (uint i = 0; i < devList.nDeviceNum; i++)
                    {
                        IntPtr pDeviceInfo = devList.pDeviceInfo[i];

                        MyCamera.MV_CC_DEVICE_INFO stDevInfo =
                        (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(pDeviceInfo,
                        typeof(MyCamera.MV_CC_DEVICE_INFO));

                     /*   MyCamera cam = new MyCamera();
                        int re = cam.MV_CC_CreateDevice_NET(ref stDevInfo);*/

                        CameraHardwareRawDto domainInfo = ConverRawToDomainModel(stDevInfo);
                        result.Add(domainInfo);

                        _scannedDeviceInfoCache[domainInfo.SerialNumber] = stDevInfo;

                    }
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
                info.Model = gigeInfo.chModelName;
                
            }
            else if (raw.nTLayerType == MyCamera.MV_USB_DEVICE)
            {
                MyCamera.MV_USB3_DEVICE_INFO_EX usbInfo = (
                    MyCamera.MV_USB3_DEVICE_INFO_EX)MyCamera.ByteToStruct(
                        raw.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO_EX));

                info.InterfaceType = "USB";
                info.SerialNumber = usbInfo.chSerialNumber.ToString();
                info.Model = usbInfo.chModelName;
                info.IpAddress = string.Empty;
            }
            return info;
        }
        public class HardwareException : Exception
        {
            public int ErrorCode { get; }
            public HardwareException(string msg, int errorCode) : base(msg)
            {
                ErrorCode = errorCode;
            }
        }

        public int Login(CameraConnectConfig connectParam, out MyCamera cameraObj)
        {
            cameraObj = null;
            // 1. 参数前置校验
            if (connectParam == null) return ERR_PARAM_NULL;

            try
            {
                MV_CC_DEVICE_INFO deviceInfo;
             

                // 1. 优先：从扫描缓存取完整结构体（官方标准路径，成功率100%）
                if (!string.IsNullOrWhiteSpace(connectParam.SerialNumber))
                {
                    lock (_cacheLock)
                    {
                        if (_scannedDeviceInfoCache.TryGetValue(connectParam.SerialNumber, out var cachedInfo))
                        {
                            deviceInfo = cachedInfo;
                          
                        }
                        else
                        {
                            deviceInfo = new MV_CC_DEVICE_INFO();
                        }
/*
                        if (!string.IsNullOrWhiteSpace(connectParam.SerialNumber))
                        {
                            ret = cameraObj.MV_CC_CreateDeviceBySerialNumber_NET(connectParam.SerialNumber);
                        }
                        // 备用：按IP创建
                        else if (!string.IsNullOrWhiteSpace(connectParam.IpAddress))
                        {
                            ret = cameraObj.MV_CC_CreateDeviceByIp_NET(connectParam.IpAddress);
                        }
                        else
                        {
                            return ERR_PARAM_EMPTY;
                        }
*/
                    }
                }
                else
                {
                    deviceInfo = new MV_CC_DEVICE_INFO();
                }
                cameraObj = new MyCamera();
                int createRet = cameraObj.MV_CC_CreateDevice_NET(ref deviceInfo);
                if (createRet != MV_OK)
                {
                    cameraObj = null;
                    return createRet;
                }

                int openRet = cameraObj.MV_CC_OpenDevice_NET(MyCamera.MV_ACCESS_Exclusive, 0);
                if (openRet != MV_OK)
                {
                    cameraObj.MV_CC_DestroyDevice_NET();
                    cameraObj = null;
                    return openRet;
                }

                // GigE相机设置最佳包大小
                if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    int packetSize = cameraObj.MV_CC_GetOptimalPacketSize_NET();
                    if (packetSize > 0)
                    {
                        cameraObj.MV_CC_SetIntValueEx_NET("GevSCPSPacketSize", (uint)packetSize);
                    }
                }

                // 默认参数初始化
                cameraObj.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                cameraObj.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);

                return MV_OK;

            }
            catch (Exception)
            {
                // 异常兜底：确保句柄一定释放
                if (cameraObj != null)
                {
                    try
                    {
                        cameraObj.MV_CC_DestroyDevice_NET();
                    }
                    catch { }
                    cameraObj = null;
                }
                return ERR_EXCEPTION;
            }

        }

        public int OpenStream(MyCamera cameraObj)
        {
            if (cameraObj == null) return ERR_INVALID_HANDLE;
            if (_isGrabbing) return MV_OK; // 幂等：已在采集中直接返回

            try
            {
                // 1. 采集前置配置：读取宽高+像素格式 + 分配转换缓冲区（对应官方 NecessaryOperBeforeGrab）
                int ret = InitGrabBuffer(cameraObj);
                if (ret != MV_OK) return ret;

                // 2. 启动采集线程
                _isGrabbing = true;
                _grabThread = new Thread(GrabThreadProcess)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                _grabThread.Start(cameraObj);

                // 3. 调用SDK开始取流
                ret = cameraObj.MV_CC_StartGrabbing_NET();
                if (ret != MV_OK)
                {
                    _isGrabbing = false;
                    return ret;
                }

                return MV_OK;
            }
            catch (Exception)
            {
                _isGrabbing = false;
                ReleaseGrabBuffer();
                return ERR_EXCEPTION;
            }
        }

        public int CloseStream(MyCamera cameraObj)
        {
            if (cameraObj == null) return ERR_INVALID_HANDLE;
            try
            {
                return cameraObj.MV_CC_StopGrabbing_NET();
            }
            catch (Exception)
            {
                return ERR_EXCEPTION;
            }
        }

        public int Logout(MyCamera cameraObj)
        {
            if (cameraObj == null) return MV_OK;

            try
            {
                // 兜底停止采集，防止漏关流导致资源残留
                cameraObj.MV_CC_StopGrabbing_NET();
                // 关闭并销毁设备
                cameraObj.MV_CC_CloseDevice_NET();
                cameraObj.MV_CC_DestroyDevice_NET();

                return MV_OK;
            }
            catch (Exception)
            {
                return ERR_EXCEPTION;
            }
        }
        private int IpStringToUint(string ipStr,out uint ipUint)
        {
            ipUint = 0;
            if (!IPAddress.TryParse(ipStr, out var iPAdd))
                return ERR_INVALID_HANDLE;

            ipUint = (uint)IPAddress.HostToNetworkOrder((int)iPAdd.AddressFamily);
            return MV_OK;
        }
        private int InitGrabBuffer(MyCamera cameraObj)
        {
            // 1. 读取图像宽度
            MVCC_INTVALUE_EX stWidth = new MVCC_INTVALUE_EX();
            int ret = cameraObj.MV_CC_GetIntValueEx_NET("Width", ref stWidth);
            if (ret != MV_OK) return ret;

            // 2. 读取图像高度
            MVCC_INTVALUE_EX stHeight = new MVCC_INTVALUE_EX();
            ret = cameraObj.MV_CC_GetIntValueEx_NET("Height", ref stHeight);
            if (ret != MV_OK) return ret;

            // 3. 读取像素格式
            MVCC_ENUMVALUE stPixelFormat = new MVCC_ENUMVALUE();
            ret = cameraObj.MV_CC_GetEnumValue_NET("PixelFormat", ref stPixelFormat);
            if (ret != MV_OK) return ret;

            // 4. 像素格式判断 + 分配转换缓冲区（完全复用官方逻辑）
            var pixelType = (Int32)stPixelFormat.nCurValue;
            if (pixelType == (Int32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined)
                return MyCamera.MV_E_UNKNOW;

            // 释放旧缓冲区
            if (_convertBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_convertBuffer);
                _convertBuffer = IntPtr.Zero;
            }

            if (IsMonoPixel(stPixelFormat.nCurValue))
            {
                _pixelFormat = PixelFormat.Format8bppIndexed;
                _convertBufferSize = (uint)(stWidth.nCurValue * stHeight.nCurValue);
            }
            else
            {
                _pixelFormat = PixelFormat.Format24bppRgb;
                _convertBufferSize = (uint)(3 * stWidth.nCurValue * stHeight.nCurValue);
            }

            _convertBuffer = Marshal.AllocHGlobal((int)_convertBufferSize);
            if (_convertBuffer == IntPtr.Zero)
                return MyCamera.MV_E_RESOURCE;

            // 5. 初始化位图对象
            _currentBitmap?.Dispose();
            _currentBitmap = new Bitmap((int)stWidth.nCurValue, (int)stHeight.nCurValue, _pixelFormat);

            // 6. Mono8格式设置灰度调色板（官方逻辑完整保留）
            if (_pixelFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette palette = _currentBitmap.Palette;
                for (int i = 0; i < palette.Entries.Length; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                _currentBitmap.Palette = palette;
            }

            return MV_OK;
        }
        private bool IsMonoPixel(UInt32 pixelType)
        {
            return pixelType == (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8
                || pixelType == (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10
                || pixelType == (UInt32)MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12;
        }
        private void GrabThreadProcess(object obj)
        {
            MyCamera camera = (MyCamera)obj;
            MV_FRAME_OUT frameInfo = new MV_FRAME_OUT();

            while (_isGrabbing)
            {
                try
                {
                    // 超时取一帧（80ms超时，避免线程卡死）
                    int ret = camera.MV_CC_GetImageBuffer_NET(ref frameInfo, 80);
                    if (ret != MV_OK)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    // 像素格式转换（SDK内置转换，输出到我们的托管缓冲区）
                    MyCamera.MV_PIXEL_CONVERT_PARAM convertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                    convertParam.nWidth = frameInfo.stFrameInfo.nWidth;
                    convertParam.nHeight = frameInfo.stFrameInfo.nHeight;
                    /*convertParam.pSrcBuffer = frameInfo.pBufAddr;
                    convertParam.nSrcBufferLen = frameInfo.stFrameInfo.nFrameLen;*/
                    
                    convertParam.enSrcPixelType = frameInfo.stFrameInfo.enPixelType;
                    convertParam.enDstPixelType = GetDstPixelType(_pixelFormat);
                    convertParam.pDstBuffer = _convertBuffer;
                    convertParam.nDstBufferSize = _convertBufferSize;

                    ret = camera.MV_CC_ConvertPixelType_NET(ref convertParam);
                    if (ret == MV_OK)
                    {
                        // 拷贝数据到位图
                        BitmapData bmpData = _currentBitmap.LockBits(
                            new Rectangle(0, 0, _currentBitmap.Width, _currentBitmap.Height),
                            ImageLockMode.WriteOnly,
                            _currentBitmap.PixelFormat);

                        CopyMemory(bmpData.Scan0, _convertBuffer, _convertBufferSize);
                        _currentBitmap.UnlockBits(bmpData);

                        // 触发事件，向上层推送图像
                        FrameReceived?.Invoke(this, new CameraFrameEventArgs(
                            string.Empty,
                            (Bitmap)_currentBitmap.Clone(),
                            (int)frameInfo.stFrameInfo.nWidth,
                            (int)frameInfo.stFrameInfo.nHeight,
                            frameInfo.stFrameInfo.nFrameNum,
                            frameInfo.stFrameInfo.nDevTimeStampHigh));
                    }

                    // 释放SDK图像缓冲区（必须调用，否则会取流失败）
                    camera.MV_CC_FreeImageBuffer_NET(ref frameInfo);
                }
                catch
                {
                    // 采集异常不崩溃，继续下一帧
                    Thread.Sleep(10);
                }
            }
        }
        private MyCamera.MvGvspPixelType GetDstPixelType(PixelFormat format)
        {
            return format == PixelFormat.Format8bppIndexed
                ? MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8
                : MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
        }
       
        private void ReleaseGrabBuffer()
        {
            if (_convertBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_convertBuffer);
                _convertBuffer = IntPtr.Zero;
            }
            _currentBitmap?.Dispose();
            _currentBitmap = null;
            _convertBufferSize = 0;
        }
        public Task<(int errorCode, MyCamera cameraObj)> LoginAsync(CameraConnectConfig connectParam)
        {
            return Task.Run(() =>
            {
                int ret = Login(connectParam, out var cam);
                return (ret, cam);
            });
        }
        public Task<int> OpenStreamAsync(MyCamera cameraObj)
        {
            return Task.Run(() => OpenStream(cameraObj));
        }
        public Task<int> CloseStreamAsync(MyCamera cameraObj)
        {
            return Task.Run(() => CloseStream(cameraObj));
        }

        public Task<int> LogoutAsync(MyCamera cameraObj)
        {
            return Task.Run(() => Logout(cameraObj));
        }
    }
}
