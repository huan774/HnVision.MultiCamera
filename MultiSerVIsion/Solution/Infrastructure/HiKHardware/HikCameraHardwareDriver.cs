using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Shared.Models;
using MvCamCtrl.NET;
using MvCameraControl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Infrastructure.HiKHardware
{
    /// <summary>
    /// 海康工业相机硬件驱动封装
    /// 【取流模式】统一采用主动取流（StartGrabbing + GetImageBuffer 轮询），
    /// 不使用 SDK 回调取流，二者互斥。
    /// 【多相机隔离】每台相机持有独立采集上下文，避免实例级共享字段互相覆盖。
    /// 【封装原则】SDK 句柄、原始设备信息与错误码全部封装在类内部，按「序列号」索引。
    /// </summary>
    public class HikCameraHardwareDriver : ICameraHardwareDriver
    {
        private const int MV_OK = 0;

        // 内部错误码（仅类内流转，绝不外泄）
        private const int ERR_PARAM_NULL = -1001;
        private const int ERR_EXCEPTION = -9999;

        // SDK 初始化状态（线程安全）
        private readonly object _sdkLock = new object();
        private bool _sdkInited = false;

        // 扫描枚举结果缓存：序列号 → 设备原始结构体（连接时按序列号取用）
        private static readonly Dictionary<string, MyCamera.MV_CC_DEVICE_INFO> _scannedDeviceInfoCache
            = new Dictionary<string, MyCamera.MV_CC_DEVICE_INFO>();

        private static readonly object _cacheLock = new object();

        // 多相机采集上下文缓存：序列号 → 独立上下文
        private readonly ConcurrentDictionary<string, CameraGrabContext> _cameraContexts
            = new ConcurrentDictionary<string, CameraGrabContext>();

        /// <summary>SDK 错误码映射表（按需补充）</summary>
        private readonly Dictionary<int, string> _errorCodeMap = new Dictionary<int, string>
        {
            { 0, "操作成功" },
            { -1001, "设备不存在或网络不通" },
            { -1002, "设备已被占用" },
            { -1003, "参数错误" },
            { -1005, "网络包大小不匹配" }
        };

        /// <summary>
        /// 单台相机的独立采集上下文
        /// 【多相机隔离】每台相机各自持有线程、缓冲与状态，互不干扰
        /// </summary>
        private sealed class CameraGrabContext
        {
            public MyCamera CameraObj;
            public Action<CameraFrame> FrameCallback;
            public Thread GrabThread;
            public volatile bool IsGrabbing;

            // 像素格式转换输出缓冲（非托管）
            public IntPtr ConvertBuffer = IntPtr.Zero;
            public uint ConvertBufferSize = 0;
        }

        /// <summary>
        /// 相机帧事件参数：携带设备序列号与统一帧数据，供上层订阅使用。
        /// 【封装原则】仅暴露统一业务帧，不暴露任何 SDK 原生类型。
        /// </summary>
        public class CameraFrameEventArgs : EventArgs
        {
            /// <summary>设备序列号</summary>
            public string DeviceId { get; }

            /// <summary>统一帧数据</summary>
            public CameraFrame Frame { get; }

            /// <summary>
            /// 构造帧事件参数
            /// </summary>
            /// <param name="deviceId">设备序列号</param>
            /// <param name="frame">统一帧数据</param>
            public CameraFrameEventArgs(string deviceId, CameraFrame frame)
            {
                DeviceId = deviceId;
                Frame = frame;
            }
        }

        /// <summary>硬件层异常（含 SDK 错误码，仅类内部使用）</summary>
        public class HardwareException : Exception
        {
            public int ErrorCode { get; }
            public HardwareException(string msg, int errorCode) : base(msg)
            {
                ErrorCode = errorCode;
            }
        }

        // ==================== 扫描枚举（已写好，保持不变） ====================

        /// <summary>扫描所有在线相机，转换为业务 DTO</summary>
        public async Task<OperationResult<List<CameraDeviceDto>>> ScanAsync()
        {
            try
            {
                var rawList = await ScanAllCameraAsync();
                if (rawList == null || rawList.Count == 0)
                    return OperationResult<List<CameraDeviceDto>>.Succes(new List<CameraDeviceDto>());

                var result = rawList.Select(raw => new CameraDeviceDto
                {
                    SerialNumber = raw.SerialNumber,
                    IpAddress = raw.IpAddress,
                    DeviceName = raw.DeviceName,
                    Model = raw.Model,
                    Manufacturer = raw.Manufacturer,
                    InterfaceType = raw.InterfaceType
                }).ToList();

                return OperationResult<List<CameraDeviceDto>>.Succes(result);
            }
            catch (Exception ex)
            {
                return OperationResult<List<CameraDeviceDto>>.Fail($"扫描相机异常：{ex.Message}");
            }
        }

        /// <summary>扫描所有在线相机（返回底层原始 DTO）</summary>
        public async Task<List<CameraHardwareRawDto>> ScanAllCameraAsync()
        {
            return await Task.Run(() =>
            {
                InitSdkIfNot();
                var result = new List<CameraHardwareRawDto>();

                lock (_cacheLock)
                {
                    _scannedDeviceInfoCache.Clear();

                    var devList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                    int ret = MyCamera.MV_CC_EnumDevices_NET(
                        MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref devList);

                    if (ret != MV_OK)
                        throw new HardwareException($"枚举相机失败，错误码{ret}", ret);

                    for (uint i = 0; i < devList.nDeviceNum; i++)
                    {
                        IntPtr pDeviceInfo = devList.pDeviceInfo[i];
                        var stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                            pDeviceInfo, typeof(MyCamera.MV_CC_DEVICE_INFO));

                        var dto = ConverRawToDomainModel(stDevInfo);
                        result.Add(dto);

                        if (!string.IsNullOrWhiteSpace(dto.SerialNumber))
                            _scannedDeviceInfoCache[dto.SerialNumber] = stDevInfo;
                    }
                }
                return result;
            });
        }

        /// <summary>SDK 原始设备结构体 → 业务原始 DTO</summary>
        private CameraHardwareRawDto ConverRawToDomainModel(MyCamera.MV_CC_DEVICE_INFO raw)
        {
            var info = new CameraHardwareRawDto();

            if (raw.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                var gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO_EX)MyCamera.ByteToStruct(
                    raw.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO_EX));

                info.InterfaceType = "GigE";
                info.IpAddress = gigeInfo.nCurrentIp.ToString();
                info.SerialNumber = gigeInfo.chSerialNumber;
                info.Model = gigeInfo.chModelName;
                info.DeviceName = gigeInfo.chUserDefinedName.ToString();
                info.Manufacturer = gigeInfo.chManufacturerName;
            }
            else if (raw.nTLayerType == MyCamera.MV_USB_DEVICE)
            {
                var usbInfo = (MyCamera.MV_USB3_DEVICE_INFO_EX)MyCamera.ByteToStruct(
                    raw.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO_EX));

                info.InterfaceType = "USB";
                info.SerialNumber = usbInfo.chSerialNumber.ToString();
                info.Model = usbInfo.chModelName;
                info.IpAddress = string.Empty;
                info.DeviceName = usbInfo.chUserDefinedName.ToString();
                info.Manufacturer = usbInfo.chManufacturerName;
            }
            return info;
        }

        // ==================== 连接 / 断开 ====================

        /// <summary>测试连接：登录成功后立即释放，不占用句柄</summary>
        public async Task<OperationResult> TestConnectAsync(CameraConnectConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.SerialNumber))
                return OperationResult.Fail("连接参数无效，缺少序列号");

            try
            {
                var (errorCode, cameraObj) = await LoginAsync(config);
                if (errorCode != MV_OK || cameraObj == null)
                    return OperationResult.Fail(GetErrorMessage(errorCode));

                try
                {
                    cameraObj.MV_CC_CloseDevice_NET();
                    cameraObj.MV_CC_DestroyDevice_NET();
                }
                catch
                {
                    // 释放阶段忽略异常
                }

                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"测试连接异常：{ex.Message}");
            }
        }

        /// <summary>正式连接：登录并缓存句柄</summary>
        public async Task<OperationResult> ConnectAsync(CameraConnectConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.SerialNumber))
                return OperationResult.Fail("连接参数无效，缺少序列号");

            if (_cameraContexts.ContainsKey(config.SerialNumber))
                return OperationResult.Succes("设备已连接");

            try
            {
                var (errorCode, cameraObj) = await LoginAsync(config);
                if (errorCode != MV_OK || cameraObj == null)
                    return OperationResult.Fail($"连接失败：{GetErrorMessage(errorCode)}");

                _cameraContexts[config.SerialNumber] = new CameraGrabContext { CameraObj = cameraObj };
                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"连接异常：{ex.Message}");
            }
        }

        /// <summary>断开：停流 → 释放缓冲 → 关闭并销毁设备</summary>
        public async Task<OperationResult> DisconnectAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("序列号不能为空");

            if (!_cameraContexts.TryRemove(serialNumber, out var ctx))
                return OperationResult.Succes("设备未连接，无需断开");

            try
            {
                // 1. 先停止采流（幂等）
                if (ctx.IsGrabbing)
                {
                    ctx.IsGrabbing = false;
                    JoinGrabThread(ctx);
                    ctx.CameraObj.MV_CC_StopGrabbing_NET();
                }

                // 2. 释放取流缓冲
                ReleaseGrabBuffer(ctx);

                // 3. 关闭并销毁设备
                ctx.CameraObj.MV_CC_CloseDevice_NET();
                ctx.CameraObj.MV_CC_DestroyDevice_NET();

                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                // 兜底销毁，避免句柄泄漏
                TryDestroyDevice(ctx.CameraObj);
                return OperationResult.Fail($"断开异常：{ex.Message}");
            }
        }

        // ==================== 主动取流 / 停止 ====================

        /// <summary>开启采流（主动取流）：StartGrabbing + 取流线程轮询</summary>
        public async Task<OperationResult> StartStreamAsync(string serialNumber, Action<CameraFrame> frameCallback)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("序列号不能为空");
            if (frameCallback == null)
                return OperationResult.Fail("帧回调不能为空");

            if (!_cameraContexts.TryGetValue(serialNumber, out var ctx))
                return OperationResult.Fail("设备未连接，请先连接后再开启采流");

            if (ctx.IsGrabbing)
                return OperationResult.Succes("设备已在采流中");

            try
            {
                ctx.FrameCallback = frameCallback;

                // 主动取流：开启 SDK 采集
                int ret = ctx.CameraObj.MV_CC_StartGrabbing_NET();
                if (ret != MV_OK)
                    return OperationResult.Fail($"开启采集失败：{GetErrorMessage(ret)}");

                // 启动主动取流线程（GetImageBuffer 轮询）
                ctx.IsGrabbing = true;
                ctx.GrabThread = new Thread(GrabThreadProcess)
                {
                    IsBackground = true,
                    Name = $"CameraGrab_{serialNumber}",
                    Priority = ThreadPriority.AboveNormal
                };
                ctx.GrabThread.Start(ctx);

                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                ctx.IsGrabbing = false;
                return OperationResult.Fail($"开启采流异常：{ex.Message}");
            }
        }

        /// <summary>停止采流：结束取流线程 → StopGrabbing → 释放缓冲</summary>
        public async Task<OperationResult> StopStreamAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("序列号不能为空");

            if (!_cameraContexts.TryGetValue(serialNumber, out var ctx))
                return OperationResult.Succes("设备未连接，无需停止采流");

            if (!ctx.IsGrabbing)
                return OperationResult.Succes("设备未在采流中");

            try
            {
                ctx.IsGrabbing = false;
                JoinGrabThread(ctx);

                int ret = ctx.CameraObj.MV_CC_StopGrabbing_NET();
                ReleaseGrabBuffer(ctx);
                ctx.FrameCallback = null;

                return ret == MV_OK
                    ? OperationResult.Succes()
                    : OperationResult.Fail($"停止采流失败：{GetErrorMessage(ret)}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"停止采流异常：{ex.Message}");
            }
        }

        // ==================== 取流线程 ====================

        /// <summary>
        /// 主动取流线程：轮询 GetImageBuffer 拉取帧，转换为统一帧后回调
        /// 【运行线程】独立后台线程
        /// </summary>
        private void GrabThreadProcess(object state)
        {
            var ctx = (CameraGrabContext)state;
            var camera = ctx.CameraObj;
            var frameInfo = new MyCamera.MV_FRAME_OUT();

            while (ctx.IsGrabbing)
            {
                try
                {
                    // 超时 80ms 取一帧，避免线程卡死
                    int ret = camera.MV_CC_GetImageBuffer_NET(ref frameInfo, 80);
                    if (ret != MV_OK)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    // 转换为统一帧格式并回调
                    var frame = ConvertToCameraFrame(camera, ref frameInfo, ctx);
                    ctx.FrameCallback?.Invoke(frame);

                    // 必须释放 SDK 图像缓冲，否则后续取流失败
                    camera.MV_CC_FreeImageBuffer_NET(ref frameInfo);
                }
                catch
                {
                    // 采集异常不崩溃，稍作等待继续下一帧
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>将 SDK 帧转换为统一业务帧（托管内存独立拷贝，脱离 SDK 生命周期）</summary>
        private CameraFrame ConvertToCameraFrame(MyCamera camera, ref MyCamera.MV_FRAME_OUT frameInfo, CameraGrabContext ctx)
        {
            int width = (int)frameInfo.stFrameInfo.nWidth;
            int height = (int)frameInfo.stFrameInfo.nHeight;
            var srcPixel = frameInfo.stFrameInfo.enPixelType;

            // 单色 → Mono8（1 字节/像素），彩色 → RGB24（3 字节/像素）
            bool isMono = IsMonoPixel(srcPixel);
            var dstPixel = isMono
                ? MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8
                : MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            int bytesPerPixel = isMono ? 1 : 3;
            int dstSize = width * height * bytesPerPixel;

            EnsureConvertBuffer(ctx, dstSize);

            // 构造像素格式转换参数（源缓冲必须显式指定，否则转换失败）
            var convertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
            convertParam.nWidth = frameInfo.stFrameInfo.nWidth;
            convertParam.nHeight = frameInfo.stFrameInfo.nHeight;
           /* convertParam.pSrcBuffer = frameInfo.pBufAddr;
            convertParam.nSrcBufferLen = frameInfo.stFrameInfo.nFrameLen;*/
            convertParam.enSrcPixelType = srcPixel;
            convertParam.enDstPixelType = dstPixel;
            convertParam.pDstBuffer = ctx.ConvertBuffer;
            convertParam.nDstBufferSize = (uint)dstSize;

            byte[] data;
            int convertRet = camera.MV_CC_ConvertPixelType_NET(ref convertParam);
            if (convertRet == MV_OK)
            {
                // 转换成功：从转换缓冲拷贝到独立托管数组
                data = new byte[dstSize];
                Marshal.Copy(ctx.ConvertBuffer, data, 0, dstSize);
            }
            else
            {
                // 转换失败：退回直接拷贝原始数据（保底可用）
                int rawLen = (int)frameInfo.stFrameInfo.nFrameLen;
                data = new byte[rawLen];
                Marshal.Copy(frameInfo.pBufAddr, data, 0, rawLen);
            }

            // 海康时间戳：高 32 位 + 低 32 位组合成 64 位 tick（纳秒）
            long timestamp = (long)(((ulong)frameInfo.stFrameInfo.nDevTimeStampHigh << 32)
                                    | frameInfo.stFrameInfo.nDevTimeStampLow);

            return new CameraFrame
            {
                Width = width,
                Height = height,
                PixelFormat = isMono ? PixelFormatEnum.Mono8 : PixelFormatEnum.RGB24,
                Data = data,
                Timestamp = timestamp,
                FrameId = frameInfo.stFrameInfo.nFrameNum
            };
        }

        /// <summary>判断是否为单色（灰度）像素格式</summary>
        private bool IsMonoPixel(MyCamera.MvGvspPixelType pixelType)
        {
            return pixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8
                || pixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10
                || pixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12;
        }

        // ==================== 登录（私有，不暴露 SDK 句柄） ====================

        /// <summary>异步登录：创建并打开设备，返回 SDK 句柄与错误码（仅类内部使用）</summary>
        private Task<(int errorCode, MyCamera cameraObj)> LoginAsync(CameraConnectConfig connectParam)
        {
            return Task.Run(() =>
            {
                int ret = Login(connectParam, out var cam);
                return (ret, cam);
            });
        }

        /// <summary>同步登录：创建设备 → 打开设备 → 配置默认采集参数</summary>
        private int Login(CameraConnectConfig connectParam, out MyCamera cameraObj)
        {
            cameraObj = null;
            if (connectParam == null)
                return ERR_PARAM_NULL;

            try
            {
                MyCamera.MV_CC_DEVICE_INFO deviceInfo;

                // 优先从扫描缓存取完整结构体（官方标准路径，成功率最高）
                lock (_cacheLock)
                {
                    if (!string.IsNullOrWhiteSpace(connectParam.SerialNumber)
                        && _scannedDeviceInfoCache.TryGetValue(connectParam.SerialNumber, out var cached))
                    {
                        deviceInfo = cached;
                    }
                    else
                    {
                        deviceInfo = new MyCamera.MV_CC_DEVICE_INFO();
                    }
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

                // GigE 相机设置最佳包大小
                if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    int packetSize = cameraObj.MV_CC_GetOptimalPacketSize_NET();
                    if (packetSize > 0)
                    {
                        cameraObj.MV_CC_SetIntValueEx_NET("GevSCPSPacketSize", (uint)packetSize);
                    }
                }

                // 默认连续采集、关闭触发
                cameraObj.MV_CC_SetEnumValue_NET("AcquisitionMode",
                    (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                cameraObj.MV_CC_SetEnumValue_NET("TriggerMode",
                    (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);

                return MV_OK;
            }
            catch
            {
                // 异常兜底：确保句柄一定释放
                if (cameraObj != null)
                {
                    try { cameraObj.MV_CC_DestroyDevice_NET(); } catch { }
                    cameraObj = null;
                }
                return ERR_EXCEPTION;
            }
        }

        // ==================== 缓冲管理 ====================

        /// <summary>按需分配非托管转换缓冲（不够大才重新分配）</summary>
        private void EnsureConvertBuffer(CameraGrabContext ctx, int size)
        {
            if (ctx.ConvertBuffer != IntPtr.Zero && ctx.ConvertBufferSize >= size)
                return;

            if (ctx.ConvertBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ctx.ConvertBuffer);
                ctx.ConvertBuffer = IntPtr.Zero;
            }

            ctx.ConvertBuffer = Marshal.AllocHGlobal(size);
            ctx.ConvertBufferSize = (uint)size;
        }

        /// <summary>释放非托管转换缓冲</summary>
        private void ReleaseGrabBuffer(CameraGrabContext ctx)
        {
            if (ctx.ConvertBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ctx.ConvertBuffer);
                ctx.ConvertBuffer = IntPtr.Zero;
            }
            ctx.ConvertBufferSize = 0;
        }

        // ==================== SDK 生命周期 / 工具 ====================

        /// <summary>SDK 初始化（线程安全，幂等）</summary>
        private void InitSdkIfNot()
        {
            if (_sdkInited) return;

            lock (_sdkLock)
            {
                if (_sdkInited) return;
                int code = MyCamera.MV_CC_Initialize_NET();
                if (code != MV_OK)
                    throw new HardwareException($"海康SDK初始化失败，错误码{code}", code);
                _sdkInited = true;
            }
        }

        /// <summary>错误码转可读信息</summary>
        private string GetErrorMessage(int errorCode)
        {
            return _errorCodeMap.TryGetValue(errorCode, out var msg)
                ? msg
                : $"未知错误码：{errorCode}";
        }

        /// <summary>等待取流线程退出（上限 2 秒，避免死等）</summary>
        private static void JoinGrabThread(CameraGrabContext ctx)
        {
            var thread = ctx.GrabThread;
            if (thread != null && thread.IsAlive && thread != Thread.CurrentThread)
            {
                thread.Join(2000);
            }
            ctx.GrabThread = null;
        }

        /// <summary>销毁设备（异常兜底，避免句柄泄漏）</summary>
        private static void TryDestroyDevice(MyCamera cameraObj)
        {
            if (cameraObj == null) return;
            try { cameraObj.MV_CC_DestroyDevice_NET(); } catch { }
        }

        /// <summary>释放所有相机会话并反初始化 SDK</summary>
        public void Dispose()
        {
            foreach (var serial in _cameraContexts.Keys.ToList())
            {
                if (_cameraContexts.TryRemove(serial, out var ctx))
                {
                    try
                    {
                        if (ctx.IsGrabbing)
                        {
                            ctx.IsGrabbing = false;
                            JoinGrabThread(ctx);
                            ctx.CameraObj.MV_CC_StopGrabbing_NET();
                        }
                        ReleaseGrabBuffer(ctx);
                        ctx.CameraObj.MV_CC_CloseDevice_NET();
                        ctx.CameraObj.MV_CC_DestroyDevice_NET();
                    }
                    catch
                    {
                        // 释放阶段忽略异常，保证尽量释放资源
                    }
                }
            }

            // 反初始化 SDK
            if (_sdkInited)
            {
                lock (_sdkLock)
                {
                    if (_sdkInited)
                    {
                        MyCamera.MV_CC_Finalize_NET();
                        _sdkInited = false;
                    }
                }
            }
        }
    }
}
