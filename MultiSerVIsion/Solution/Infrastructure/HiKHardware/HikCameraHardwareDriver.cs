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
using System.Text;
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

                // 主动取流：开启 SDK 采集并启动取流线程（与参数配置的恢复逻辑共用 BeginGrab）
                return BeginGrab(ctx, serialNumber);
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

            // 单色 → Mono8（1 字节/像素）；彩色 → BGR8（3 字节/像素，与 GDI+ 24bpp 内存顺序一致）
            bool isMono = IsMonoPixel(srcPixel);
            var dstPixel = isMono
                ? MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8
                : MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;
            int bytesPerPixel = isMono ? 1 : 3;
            int dstSize = width * height * bytesPerPixel;

            EnsureConvertBuffer(ctx, dstSize);

            // 构造像素格式转换参数（源缓冲必须显式指定，否则转换失败）
            var convertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
            convertParam.nWidth = frameInfo.stFrameInfo.nWidth;
            convertParam.nHeight = frameInfo.stFrameInfo.nHeight;
            // 源缓冲必须显式指定，否则 MV_CC_ConvertPixelType_NET 必然失败（此前被注释，导致转换始终失败并回退原始数据）
            convertParam.pSrcData = frameInfo.pBufAddr;
            convertParam.nSrcDataLen = frameInfo.stFrameInfo.nFrameLen;
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
                PixelFormat = isMono ? PixelFormatEnum.Mono8 : PixelFormatEnum.BGR24,
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

        // ==================== 参数配置（通用参数读写：覆盖高频与基础参数） ====================

        /// <summary>
        /// 读取指定参数当前值。
        /// 【说明】已登记参数按描述符类型读取；未登记参数按「浮点 → 整型 → 枚举 → 布尔」顺序探测。
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="paramName">参数节点名（如 ExposureTime）</param>
        /// <returns>成功时 Data 为当前值（double / long / string / bool）</returns>
        public async Task<OperationResult<object>> GetParamAsync(string serialNumber, string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                return OperationResult<object>.Fail("参数名不能为空");
            if (!TryGetContext(serialNumber, out var ctx))
                return OperationResult<object>.Fail("设备未连接，请先连接后再读取参数");

            return await Task.Run(() =>
            {
                var cam = ctx.CameraObj;
                var descriptor = HikDescriptor.Find(paramName);

                if (descriptor != null)
                {
                    // 已登记参数：严格按描述符类型读取，避免误判节点类型；顺带回填范围/步长/可选项
                    return TryReadParam(cam, descriptor, out var registeredValue)
                        ? OperationResult<object>.Succes(registeredValue)
                        : OperationResult<object>.Fail($"读取参数失败：{paramName}");
                }

                // 未登记参数：依次探测，返回首个读取成功的类型
                if (TryReadFloat(cam, paramName, out var probeFloat, out _, out _))
                    return OperationResult<object>.Succes((object)probeFloat);
                if (TryReadInt(cam, paramName, out var probeInt, out _, out _, out _))
                    return OperationResult<object>.Succes((object)probeInt);
                if (TryReadEnumSymbolic(cam, paramName, out var probeSymbol))
                    return OperationResult<object>.Succes((object)probeSymbol);
                if (TryReadBool(cam, paramName, out var probeBool))
                    return OperationResult<object>.Succes((object)probeBool);

                return OperationResult<object>.Fail($"不支持的参数：{paramName}");
            });
        }

        /// <summary>
        /// 写入指定参数。
        /// 【说明】按值的运行期类型选择 SDK 写入方式；基础参数节点要求停止采集，故内部自动暂停并恢复。
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="paramName">参数节点名</param>
        /// <param name="value">待写入值（double/float、整型、bool 或枚举符号字符串）</param>
        public async Task<OperationResult> SetParamAsync(string serialNumber, string paramName, object value)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                return OperationResult.Fail("参数名不能为空");
            if (value == null)
                return OperationResult.Fail("参数值不能为空");
            if (!TryGetContext(serialNumber, out var ctx))
                return OperationResult.Fail("设备未连接，请先连接后再配置参数");

            return await Task.Run(() =>
            {
                var cam = ctx.CameraObj;
                var descriptor = HikDescriptor.Find(paramName);

                // 基础参数多数节点要求停止采集后才可写，故先暂停、写完恢复
                bool needPause = descriptor != null && descriptor.Group == HikParamGroup.Basic;
                bool paused = needPause && PauseGrab(ctx);
                try
                {
                    int ret = WriteNode(cam, paramName, descriptor, value);
                    return ret == MV_OK
                        ? OperationResult.Succes()
                        : OperationResult.Fail($"参数 {paramName} 写入失败：{GetErrorMessage(ret)}");
                }
                finally
                {
                    if (paused) ResumeGrab(ctx, serialNumber);
                }
            });
        }

        /// <summary>
        /// 查询指定参数的元信息（取值范围、可写性、枚举项），供界面生成可调节项。
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="paramName">参数节点名</param>
        /// <returns>成功时返回已回填范围与枚举项的描述符</returns>
        public async Task<OperationResult<HikDescriptor>> GetParamDescriptorAsync(string serialNumber, string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                return OperationResult<HikDescriptor>.Fail("参数名不能为空");
            if (!TryGetContext(serialNumber, out var ctx))
                return OperationResult<HikDescriptor>.Fail("设备未连接，请先连接后再查询参数");

            return await Task.Run(() =>
            {
                var cam = ctx.CameraObj;
                // 未登记参数按浮点型描述符占位，便于上层先展示再探测
                var descriptor = HikDescriptor.Find(paramName)
                                 ?? new HikDescriptor(paramName, HikParamGroup.Basic,
                                        HikParamValueType.Float, paramName);

                // 从相机回填取值范围与枚举项；节点不存在时保持默认，不影响描述符可用性
                switch (descriptor.ValueType)
                {
                    case HikParamValueType.Float:
                        if (TryReadFloat(cam, paramName, out _, out var fmin, out var fmax))
                        {
                            descriptor.Min = fmin;
                            descriptor.Max = fmax;
                        }
                        break;

                    case HikParamValueType.Int:
                        if (TryReadInt(cam, paramName, out _, out var imin, out var imax, out var istep))
                        {
                            descriptor.Min = imin;
                            descriptor.Max = imax;
                            descriptor.Step = istep;
                        }
                        break;

                    case HikParamValueType.Enum:
                        descriptor.EnumEntries = ReadEnumEntries(cam, paramName);
                        break;
                }

                return OperationResult<HikDescriptor>.Succes(descriptor);
            });
        }

        /// <summary>获取已连接相机的采集上下文</summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="ctx">输出：采集上下文</param>
        /// <returns>设备已连接时返回 true</returns>
        private bool TryGetContext(string serialNumber, out CameraGrabContext ctx)
        {
            ctx = null;
            if (string.IsNullOrWhiteSpace(serialNumber)) return false;
            return _cameraContexts.TryGetValue(serialNumber, out ctx);
        }

        /// <summary>按值的运行期类型选择 SDK 写入方式</summary>
        /// <returns>SDK 返回码（MV_OK 表示成功）</returns>
        private static int WriteNode(MyCamera cam, string nodeName, HikDescriptor descriptor, object value)
        {
            // 枚举符号字符串：用符号名写入，规避各机型枚举值不一致的问题
            if (value is string symbol)
                return cam.MV_CC_SetEnumValueByString_NET(nodeName, symbol);

            if (value is bool boolValue)
                return cam.MV_CC_SetBoolValue_NET(nodeName, boolValue);

            if (value is float || value is double || value is decimal)
            {
                // 帧率节点受使能开关控制，写入前先使能
                if (descriptor != null
                    && string.Equals(descriptor.ParamName, HikParamName.AcquisitionFrameRate, StringComparison.Ordinal))
                {
                    cam.MV_CC_SetBoolValue_NET(HikParamName.AcquisitionFrameRateEnable, true);
                }
                return cam.MV_CC_SetFloatValue_NET(nodeName, Convert.ToSingle(value));
            }

            // 整型：枚举型按枚举值写入，其余按整型写入
            uint intValue = Convert.ToUInt32(value);
            return descriptor != null && descriptor.ValueType == HikParamValueType.Enum
                ? cam.MV_CC_SetEnumValue_NET(nodeName, intValue)
                : cam.MV_CC_SetIntValue_NET(nodeName, intValue);
        }

        /// <summary>读取浮点节点当前值与允许范围</summary>
        private static bool TryReadFloat(MyCamera cam, string nodeName,
            out double value, out double min, out double max)
        {
            value = 0;
            min = 0;
            max = 0;
            if (cam == null) return false;

            // 定长数组字段必须按 MarshalAs(SizeConst) 预分配，否则非托管封送会失败
            var stValue = new MyCamera.MVCC_FLOATVALUE { nReserved = new uint[4] };
            if (cam.MV_CC_GetFloatValue_NET(nodeName, ref stValue) != MV_OK) return false;

            value = stValue.fCurValue;
            min = stValue.fMin;
            max = stValue.fMax;
            return true;
        }

        /// <summary>读取整型节点当前值、允许范围与最小步长</summary>
        private static bool TryReadInt(MyCamera cam, string nodeName,
            out long value, out double min, out double max, out double step)
        {
            value = 0;
            min = 0;
            max = 0;
            step = 0;
            if (cam == null) return false;

            var stValue = new MyCamera.MVCC_INTVALUE { nReserved = new uint[4] };
            if (cam.MV_CC_GetIntValue_NET(nodeName, ref stValue) != MV_OK) return false;

            value = stValue.nCurValue;
            min = stValue.nMin;
            max = stValue.nMax;
            // nInc 为相机声明的最小步长，是界面步进与实体校验的共同依据
            step = stValue.nInc;
            return true;
        }

        /// <summary>读取布尔节点当前值</summary>
        private static bool TryReadBool(MyCamera cam, string nodeName, out bool value)
        {
            value = false;
            if (cam == null) return false;

            bool result = false;
            if (cam.MV_CC_GetBoolValue_NET(nodeName, ref result) != MV_OK) return false;

            value = result;
            return true;
        }

        /// <summary>读取枚举节点当前的符号名（如 Mono8 / Off / Software）</summary>
        private static bool TryReadEnumSymbolic(MyCamera cam, string nodeName, out string symbol)
        {
            symbol = null;
            if (cam == null) return false;

            if (!TryGetEnumValue(cam, nodeName, out var stEnum)) return false;

            var stEntry = new MyCamera.MVCC_ENUMENTRY
            {
                nValue = stEnum.nCurValue,
                chSymbolic = new byte[64],
                nReserved = new uint[4]
            };
            if (cam.MV_CC_GetEnumEntrySymbolic_NET(nodeName, ref stEntry) != MV_OK) return false;

            symbol = DecodeSymbolic(stEntry.chSymbolic);
            return !string.IsNullOrEmpty(symbol);
        }

        /// <summary>读取枚举节点支持的全部符号名（供界面生成下拉项）</summary>
        private static List<string> ReadEnumEntries(MyCamera cam, string nodeName)
        {
            var entries = new List<string>();
            if (!TryGetEnumValue(cam, nodeName, out var stEnum)) return entries;

            // 相机上报的支持个数可能超过缓冲上限，需按 SizeConst 截断
            uint count = Math.Min(stEnum.nSupportedNum, (uint)stEnum.nSupportValue.Length);
            for (uint i = 0; i < count; i++)
            {
                var stEntry = new MyCamera.MVCC_ENUMENTRY
                {
                    nValue = stEnum.nSupportValue[i],
                    chSymbolic = new byte[64],
                    nReserved = new uint[4]
                };
                if (cam.MV_CC_GetEnumEntrySymbolic_NET(nodeName, ref stEntry) != MV_OK) continue;

                var symbol = DecodeSymbolic(stEntry.chSymbolic);
                if (!string.IsNullOrEmpty(symbol)) entries.Add(symbol);
            }
            return entries;
        }

        /// <summary>读取枚举节点的原始枚举值</summary>
        private static bool TryGetEnumValue(MyCamera cam, string nodeName, out MyCamera.MVCC_ENUMVALUE stEnum)
        {
            stEnum = new MyCamera.MVCC_ENUMVALUE
            {
                nSupportValue = new uint[64],
                nReserved = new uint[4]
            };
            if (cam == null) return false;
            return cam.MV_CC_GetEnumValue_NET(nodeName, ref stEnum) == MV_OK;
        }

        /// <summary>按 C 字符串（'\0' 结尾）解码 SDK 返回的定长字节数组</summary>
        private static string DecodeSymbolic(byte[] raw)
        {
            if (raw == null || raw.Length == 0) return null;

            int length = Array.IndexOf(raw, (byte)0);
            if (length < 0) length = raw.Length;
            return Encoding.ASCII.GetString(raw, 0, length).Trim();
        }

        /// <summary>暂停取流（仅在实际采流时执行）</summary>
        /// <returns>确实执行了暂停时返回 true，用于决定是否需要恢复</returns>
        private static bool PauseGrab(CameraGrabContext ctx)
        {
            if (ctx == null || !ctx.IsGrabbing) return false;

            ctx.IsGrabbing = false;
            JoinGrabThread(ctx);
            ctx.CameraObj?.MV_CC_StopGrabbing_NET();
            return true;
        }

        /// <summary>恢复取流：仅在存在帧回调时执行（参数配置完成后调用）</summary>
        private void ResumeGrab(CameraGrabContext ctx, string serialNumber)
        {
            if (ctx?.FrameCallback == null) return;
            BeginGrab(ctx, serialNumber);
        }

        /// <summary>开启 SDK 采集并启动取流线程（StartStreamAsync 与参数配置恢复共用）</summary>
        private OperationResult BeginGrab(CameraGrabContext ctx, string serialNumber)
        {
            int ret = ctx.CameraObj.MV_CC_StartGrabbing_NET();
            if (ret != MV_OK)
                return OperationResult.Fail($"开启采集失败：{GetErrorMessage(ret)}");

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

        /// <summary>
        /// 读取参数：一次性返回「当前值 + 完整规格（范围 / 步长 / 可选项 / 可写性）」。
        /// 【设计】描述符一律先复制再回填，避免多相机/多次读取互相覆盖全局参数目录。
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="paramName">参数节点名</param>
        /// <returns>成功时 Data 为已回填规格且含 CurrentValue 的描述符</returns>
        public async Task<OperationResult<HikDescriptor>> ReadParamAsync(string serialNumber, string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                return OperationResult<HikDescriptor>.Fail("参数名不能为空");
            if (!TryGetContext(serialNumber, out var ctx))
                return OperationResult<HikDescriptor>.Fail("设备未连接，请先连接后再读取参数");

            return await Task.Run(() =>
            {
                var cam = ctx.CameraObj;

                // Find 返回副本，未登记参数再按类型探测，保证不会污染全局目录
                var descriptor = ResolveDescriptor(cam, paramName);
                if (descriptor == null)
                    return OperationResult<HikDescriptor>.Fail($"不支持的参数：{paramName}");

                if (!TryReadParam(cam, descriptor, out var currentValue))
                    return OperationResult<HikDescriptor>.Fail($"读取参数失败：{paramName}");

                descriptor.CurrentValue = currentValue;
                return OperationResult<HikDescriptor>.Succes(descriptor);
            });
        }

        /// <summary>
        /// 取得参数描述符：已登记参数用登记类型；未登记参数按「浮点 → 整型 → 枚举 → 布尔」探测真实类型，
        /// 避免用错误的 SDK API 读取节点而误判。
        /// </summary>
        /// <param name="cam">相机句柄</param>
        /// <param name="paramName">参数节点名</param>
        /// <returns>可安全回填的描述符副本；节点不存在时返回 null</returns>
        private static HikDescriptor ResolveDescriptor(MyCamera cam, string paramName)
        {
            var registered = HikDescriptor.Find(paramName);
            if (registered != null) return registered;

            var probe = new HikDescriptor(paramName, HikParamGroup.Basic, HikParamValueType.Float, paramName);

            if (TryReadFloat(cam, paramName, out _, out _, out _)) return probe;

            if (TryReadInt(cam, paramName, out _, out _, out _, out _))
            {
                probe.ValueType = HikParamValueType.Int;
                return probe;
            }

            if (TryReadEnumSymbolic(cam, paramName, out _))
            {
                probe.ValueType = HikParamValueType.Enum;
                return probe;
            }

            if (TryReadBool(cam, paramName, out _))
            {
                probe.ValueType = HikParamValueType.Bool;
                return probe;
            }

            return null;
        }

        /// <summary>
        /// 按描述符类型读取当前值，并把范围 / 步长 / 可选项回填到描述符。
        /// </summary>
        /// <param name="cam">相机句柄</param>
        /// <param name="descriptor">待回填的描述符</param>
        /// <param name="currentValue">输出：当前值（double / long / string / bool）</param>
        /// <returns>读取成功时返回 true</returns>
        private static bool TryReadParam(MyCamera cam, HikDescriptor descriptor, out object currentValue)
        {
            currentValue = null;
            if (cam == null || descriptor == null) return false;

            switch (descriptor.ValueType)
            {
                case HikParamValueType.Float:
                    if (!TryReadFloat(cam, descriptor.ParamName, out var fvalue, out var fmin, out var fmax))
                        return false;
                    descriptor.Min = fmin;
                    descriptor.Max = fmax;
                    // SDK 未提供浮点节点步长，Step 保持 0 表示无步长约束
                    currentValue = fvalue;
                    return true;

                case HikParamValueType.Int:
                    if (!TryReadInt(cam, descriptor.ParamName, out var ivalue, out var imin, out var imax, out var istep))
                        return false;
                    descriptor.Min = imin;
                    descriptor.Max = imax;
                    descriptor.Step = istep;
                    currentValue = ivalue;
                    return true;

                case HikParamValueType.Enum:
                    if (!TryReadEnumSymbolic(cam, descriptor.ParamName, out var symbol))
                        return false;
                    descriptor.EnumEntries = ReadEnumEntries(cam, descriptor.ParamName);
                    currentValue = symbol;
                    return true;

                case HikParamValueType.Bool:
                    if (!TryReadBool(cam, descriptor.ParamName, out var bvalue))
                        return false;
                    currentValue = bvalue;
                    return true;
            }

            return false;
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
