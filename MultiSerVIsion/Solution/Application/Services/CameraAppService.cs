using MultiSerVIsion.Solution.Application.Dtos;
using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Entities.Configs;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Domain.Models;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Domain.Services;
using MultiSerVIsion.Solution.Infrastructure.Events;
using MultiSerVIsion.Solution.Infrastructure.HiKHardware;
using MultiSerVIsion.Solution.Presentation.Events;
using MultiSerVIsion.Solution.Shared.Extensions;
using MultiSerVIsion.Solution.Shared.Helpers;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiSerVIsion.Solution.Infrastructure.HiKHardware.HikCameraHardwareDriver;

namespace MultiSerVIsion.Solution.Application.Services
{
    /// <summary>
    /// 相机应用服务：编排相机的扫描、组态、连接、断开、采流、参数配置等用例。
    /// 【职责】调用领域服务做业务校验、调用硬件驱动执行实际操作、发布领域事件。
    /// 【原则】不直接依赖 SDK，仅通过 ICameraHardwareDriver 接口与硬件交互，驱动内部按序列号管理多相机。
    /// </summary>
    public class CameraApplicationoService : ICameraAppService
    {
        private readonly ICameraDeviceService _cameraDomainService;   // 相机领域服务（业务规则校验）
        private readonly IDeviceManager _deviceManager;               // 设备内存管理器（组态设备）
        private readonly ICameraHardwareDriver _driver;               // 硬件驱动（内部按序列号管理多相机）
        private readonly IEventBus _eventBus;                         // 全局事件总线

        public CameraApplicationoService(
            ICameraDeviceService cameraDomainService,
            IDeviceManager cameraDeviceManager,
            ICameraHardwareDriver driver,
            IEventBus eventBus)
        {
            _cameraDomainService = cameraDomainService;
            _deviceManager = cameraDeviceManager;
            _driver = driver;
            _eventBus = eventBus;
        }

        /// <summary>相机帧到达事件：驱动采集到统一帧后向上层透传</summary>
        public event EventHandler<CameraFrameEventArgs> FrameReceived;

        /// <summary>
        /// 搜索所有在线相机（驱动内部已转换为统一 DTO 列表）
        /// </summary>
        public async Task<OperationResult<List<CameraDeviceDto>>> SearchOnlineCamera()
        {
            try
            {
                // 直接调用驱动扫描，ScanAsync 返回的已是 OperationResult<List<CameraDeviceDto>>
                return await _driver.ScanAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Error("搜索在线相机异常", ex);
                return OperationResult<List<CameraDeviceDto>>.Fail($"搜索失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 将扫描到的在线相机一键导入组态（落盘 JSON）
        /// </summary>
        /// <param name="scannedCamera">扫描得到的相机 DTO</param>
        public OperationResult<DeviceEntity> AddScannedCameraToConfig(CameraDeviceDto scannedCamera)
        {
            try
            {
                // 1. 防重复：按序列号判断是否已添加过
                var exist = _deviceManager.GetDevices<CameraEntity>()
                    .FirstOrDefault(c => c.CameraAllConfig.ConnectConfig.SerialNumber
                                         == scannedCamera.SerialNumber);
                if (exist != null)
                    return OperationResult<DeviceEntity>.Fail("该相机已在组态中，请勿重复添加");

                // 2. 扫描 DTO → 持久化实体（调用映射扩展方法）
                var cameraEntity = scannedCamera.ToNewCameraEntity();

                // 3. 加入内存管理器
                // 【说明】扫描结果来源于硬件真值（序列号/型号/接口类型均由设备提供），
                //          不是“用户表单输入”，因此跳过 ValidateRule 表单校验，
                //          仅由 DeviceManager 做 ID 去重与完整性检查，避免 IP 等表单规则误拦。
                bool addOk = _deviceManager.AddDevice(cameraEntity);
                if (!addOk)
                    return OperationResult<DeviceEntity>.Fail("添加失败：设备ID重复或数据不完整");

                return OperationResult<DeviceEntity>.Succes(cameraEntity);
            }
            catch (Exception ex)
            {
                LogHelper.Error("添加扫描相机异常", ex);
                return OperationResult<DeviceEntity>.Fail($"添加失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 测试连接（短连接，登录后立即释放，不占用句柄）
        /// 【用途】保存组态前验证参数可达性，验证完即释放，不建立持久连接。
        /// </summary>
        /// <param name="connectConfig">连接参数</param>
        public async Task<OperationResult> TestConnectAsync(CameraConnectConfig connectConfig)
        {
            if (connectConfig == null)
                return OperationResult.Fail("连接参数不能为空");

            try
            {
                // 直接委托驱动执行短连接测试，驱动内部登录成功后立即释放资源
                return await _driver.TestConnectAsync(connectConfig);
            }
            catch (Exception ex)
            {
                LogHelper.Error("测试连接异常", ex);
                return OperationResult.Fail($"测试连接异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 连接已组态相机（长连接，句柄由驱动内部按序列号缓存）
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public async Task<OperationResult> ConnectCamera(string deviceId)
        {
            // 1. 取设备实体
            var device = _deviceManager.GetDeviceById(deviceId) as CameraEntity;
            if (device == null)
                return OperationResult.Fail("设备不存在");

            // 2. 业务规则校验：交给领域服务判断能否连接
            var validateResult = _cameraDomainService.ValidateCanConnect(device);
            if (!validateResult.Success)
                return validateResult;

            // 3. 组装硬件连接参数
            var connectConfig = new CameraConnectConfig
            {
                SerialNumber = device.CameraAllConfig.ConnectConfig.SerialNumber,
                IpAddress = device.IpAddress,
                InterfaceType = device.CameraAllConfig.ConnectConfig.InterfaceType
            };

            try
            {
                // 4. 调用驱动正式连接
                var connectResult = await _driver.ConnectAsync(connectConfig);
                if (!connectResult.Success)
                    return connectResult;

                // 5. 更新领域对象状态
                _cameraDomainService.ApplyConnectionStatus(device, true);

                // 6. 参数同步：首次连接以相机为准回读并落盘；已有持久化配置则以实体（JSON）值为准下发，
                //    避免每次连接都被 SDK 默认值覆盖，使持久化真正生效
                if (HasPersistedParamConfig(device))
                {
                    // 已配置：仅刷新规格（范围/步长/可选项属设备能力），不覆盖持久化的参数值
                    await RefreshParamsFromDeviceAsync(device, connectConfig.SerialNumber, overwriteValues: false);
                    // 把上次保存的参数值重新下发给相机
                    await ApplyPersistedParamsToDeviceAsync(device, connectConfig.SerialNumber);
                }
                else
                {
                    // 首次配置：以相机为准回读参数值并落盘
                    await RefreshParamsFromDeviceAsync(device, connectConfig.SerialNumber, overwriteValues: true);
                }

                // 7. 发布全局连接事件
                _eventBus.Publish(new DeviceConnectionChangedEveent(deviceId, true));

                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                LogHelper.Error("连接相机异常", ex);
                return OperationResult.Fail($"连接异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 在线扫描设备直连（无实体，跳过实体校验，直接调用驱动正式连接）
        /// </summary>
        /// <param name="config">连接参数</param>
        public async Task<OperationResult> ConnectOnlineCamera(CameraConnectConfig config)
        {
            if (config == null)
                return OperationResult.Fail("连接参数不能为空");

            try
            {
                var result = await _driver.ConnectAsync(config);
                if (result.Success)
                {
                    _eventBus.Publish(new DeviceConnectionChangedEveent(config.SerialNumber, true));
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("在线设备连接异常", ex);
                return OperationResult.Fail($"连接异常：{ex.Message}");
            }
        }

        /// <summary>获取相机实体，非相机类型返回 null</summary>
        private CameraEntity GetCameraEntity(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return null;
            return _deviceManager.GetDeviceById(deviceId) as CameraEntity;
        }

        /// <summary>
        /// 断开相机（驱动内部会先停流、释放缓冲、关闭设备）
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public OperationResult DisconnectCamera(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            var serialNumber = camera.CameraAllConfig.ConnectConfig.SerialNumber;
            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("设备缺少序列号，无法断开");

            try
            {
                // 同步等待异步断开结果（驱动内部为纯同步 SDK 操作，无死锁风险）
                var result = _driver.DisconnectAsync(serialNumber).GetAwaiter().GetResult();

                // 更新领域状态（成功与否都置为断开，避免半连接残留）
                _cameraDomainService.ApplyConnectionStatus(camera, false);
                _eventBus.Publish(new DeviceConnectionChangedEveent(serialNumber, false));

                return result.Success
                    ? OperationResult.Succes()
                    : OperationResult.Fail($"断开失败：{result.Message}");
            }
            catch (Exception ex)
            {
                LogHelper.Error("断开相机异常", ex);
                _cameraDomainService.ApplyConnectionStatus(camera, false);
                return OperationResult.Fail($"相机断开异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 开启连续采流（主动取流模式），帧通过 FrameReceived 事件透传
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public async Task<OperationResult> StartStream(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            // 业务校验：交给领域服务判断能否采流
            var validateResult = _cameraDomainService.ValidateCanStartStream(camera);
            if (!validateResult.Success)
                return validateResult;

            var serialNumber = camera.CameraAllConfig.ConnectConfig.SerialNumber;

            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("设备缺少序列号，无法开启采流");

            try
            {
                // 帧回调：将驱动统一帧包装为带设备 ID 的事件参数，向上层透传
                Action<CameraFrame> frameCallback = frame =>
                {
                    FrameReceived?.Invoke(this, new CameraFrameEventArgs(deviceId, frame));
                };

                // 调用驱动开启采流
                var result = await _driver.StartStreamAsync(serialNumber, frameCallback);
                if (!result.Success)
                    return result;

                camera.DetailStatus = CameraStatus.Streaming;
                return OperationResult.Succes();
            }
            catch (Exception ex)
            {
                LogHelper.Error("开启采流异常", ex);
                return OperationResult.Fail($"开启采流异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 停止采流，回到已连接状态
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        public OperationResult StopStream(string deviceId)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null)
                return OperationResult.Fail("设备不存在或不是相机类型");

            var serialNumber = camera.CameraAllConfig.ConnectConfig.SerialNumber;
            if (string.IsNullOrWhiteSpace(serialNumber))
                return OperationResult.Fail("设备缺少序列号，无法停止采流");

            try
            {
                // 同步等待异步停流结果
                var result = _driver.StopStreamAsync(serialNumber).GetAwaiter().GetResult();

                // 停流成功回到已连接状态
                camera.DetailStatus = CameraStatus.Connected;

                return result.Success
                    ? OperationResult.Succes()
                    : OperationResult.Fail($"停止采流失败：{result.Message}");
            }
            catch (Exception ex)
            {
                LogHelper.Error("停止采流异常", ex);
                camera.DetailStatus = CameraStatus.Connected;
                return OperationResult.Fail($"停止采流异常：{ex.Message}");
            }
        }

        // ==================== 参数配置（对接硬件层：用户可调节的高频与基础参数） ====================

        /// <summary>
        /// 读取相机参数当前值。
        /// 【职责】仅完成「组态设备 ID → 相机序列号」的翻译，读写细节全部由硬件层承担。
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        /// <param name="paramName">参数节点名（如 ExposureTime）</param>
        /// <returns>成功时 Data 为当前值（double / long / string / bool）</returns>
        public async Task<OperationResult<object>> GetParamAsync(string deviceId, string paramName)
        {
            if (!TryResolveSerialNumber(deviceId, out var serialNumber))
                return OperationResult<object>.Fail("设备不存在、不是相机类型或缺少序列号");

            try
            {
                return await _driver.GetParamAsync(serialNumber, paramName);
            }
            catch (Exception ex)
            {
                LogHelper.Error("读取相机参数异常", ex);
                return OperationResult<object>.Fail($"读取参数异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 下发相机参数。
        /// 【职责】参数合法性由实体缓存的规格（范围/步长）与相机节点共同约束，
        /// 应用层负责设备解析、成功后写入实体并落盘、以及异常兜底。
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        /// <param name="paramName">参数节点名</param>
        /// <param name="value">待写入值（double/float、整型、bool 或枚举符号字符串）</param>
        public async Task<OperationResult> SetParamAsync(string deviceId, string paramName, object value)
        {
            var camera = GetCameraEntity(deviceId);
            if (camera == null || string.IsNullOrWhiteSpace(camera.CameraAllConfig.ConnectConfig.SerialNumber))
                return OperationResult.Fail("设备不存在、不是相机类型或缺少序列号");

            string serialNumber = camera.CameraAllConfig.ConnectConfig.SerialNumber;

            // 实体侧权威校验：与界面限幅共用同一份参数规格，避免两处规则分叉；
            // 校验不通过则不下发，保证“参数合理下发”。
            var validate = camera.ValidateParam(paramName, value);
            if (!validate.IsValid)
                return OperationResult.Fail(validate.Message);

            try
            {
                var result = await _driver.SetParamAsync(serialNumber, paramName, value);
                if (result.Success)
                {
                    // 改参成功后立即写入实体并落盘（自动保存），保证重启后参数不丢失
                    PersistParamValue(camera, paramName, value);
                    _deviceManager.Update(camera);
                }
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("下发相机参数异常", ex);
                return OperationResult.Fail($"下发参数异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 查询参数元信息（取值范围、可写性、枚举项），供上层生成可调节项。
        /// 【职责】同上，仅做设备解析与透传。
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        /// <param name="paramName">参数节点名</param>
        /// <returns>成功时返回已回填范围与枚举项的描述符</returns>
        public async Task<OperationResult<HikDescriptor>> GetParamDescriptorAsync(string deviceId, string paramName)
        {
            if (!TryResolveSerialNumber(deviceId, out var serialNumber))
                return OperationResult<HikDescriptor>.Fail("设备不存在、不是相机类型或缺少序列号");

            try
            {
                return await _driver.GetParamDescriptorAsync(serialNumber, paramName);
            }
            catch (Exception ex)
            {
                LogHelper.Error("查询相机参数元信息异常", ex);
                return OperationResult<HikDescriptor>.Fail($"查询参数元信息异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 读取参数：一次性返回「当前值 + 完整规格（范围 / 步长 / 可选项）」，供界面限幅与展示。
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        /// <param name="paramName">参数节点名</param>
        /// <returns>成功时 Data 含已回填的规格与当前值</returns>
        public async Task<OperationResult<ParamReadResult>> ReadParamAsync(string deviceId, string paramName)
        {
            if (!TryResolveSerialNumber(deviceId, out var serialNumber))
                return OperationResult<ParamReadResult>.Fail("设备不存在、不是相机类型或缺少序列号");

            try
            {
                var readResult = await _driver.ReadParamAsync(serialNumber, paramName);
                if (!readResult.Success || readResult.Data == null)
                    return OperationResult<ParamReadResult>.Fail(readResult.Message);

                // 跨层映射：硬件描述符 → 领域规格，避免上层依赖硬件层类型
                return OperationResult<ParamReadResult>.Succes(new ParamReadResult
                {
                    Spec = ToParamSpec(readResult.Data),
                    Value = readResult.Data.CurrentValue
                });
            }
            catch (Exception ex)
            {
                LogHelper.Error("读取相机参数规格异常", ex);
                return OperationResult<ParamReadResult>.Fail($"读取参数规格异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 从相机回读参数规格，并可选地用相机值覆盖实体参数值。
        /// 【意义】规格（范围/步长/可选项）属设备能力，使未连接相机时也能限幅与校验。
        /// 【区分】overwriteValues = true 表示首次配置（以相机为准写入实体）；
        ///          false 表示已有持久化配置（保留实体值，仅刷新规格）。
        /// 【容错】单个参数读取失败不影响其余参数，保证部分可读也能落库。
        /// </summary>
        /// <param name="camera">相机实体</param>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="overwriteValues">是否用相机当前值覆盖实体持久化的参数值</param>
        private async Task RefreshParamsFromDeviceAsync(CameraEntity camera, string serialNumber, bool overwriteValues)
        {
            if (camera == null || string.IsNullOrWhiteSpace(serialNumber)) return;

            foreach (var descriptor in HikDescriptor.All)
            {
                try
                {
                    var readResult = await _driver.ReadParamAsync(serialNumber, descriptor.ParamName);
                    if (!readResult.Success || readResult.Data == null) continue;

                    // 规格随实体持久化，供界面限幅与实体校验共用
                    camera.CameraAllConfig.UpsertSpec(ToParamSpec(readResult.Data));

                    // 首次配置才写入参数值；已有持久化配置时保留 JSON 中的用户设置
                    if (overwriteValues && readResult.Data.CurrentValue != null)
                        PersistParamValue(camera, descriptor.ParamName, readResult.Data.CurrentValue);
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"回读参数 {descriptor.ParamName} 异常", ex);
                }
            }

            // 回读结果统一落盘，保持内存与文件一致
            _deviceManager.Update(camera);
        }

        /// <summary>
        /// 判断实体是否已有持久化的参数配置。
        /// 【依据】参数规格缓存非空，说明上一次运行已完成回读并落盘（JSON 中已有配置），
        /// 此时应改以实体值为准，而不是再次被相机默认值覆盖。
        /// </summary>
        /// <param name="camera">相机实体</param>
        /// <returns>存在持久化参数规格时返回 true</returns>
        private static bool HasPersistedParamConfig(CameraEntity camera)
        {
            var specs = camera?.CameraAllConfig?.ParamSpecs;
            return specs != null && specs.Count > 0;
        }

        /// <summary>
        /// 把实体中持久化的参数值下发给相机。
        /// 【意义】保证用户上次保存的参数在每次连接后依然生效，而不是被 SDK 默认值覆盖。
        /// 【容错】单个参数下发失败不影响其余参数，也不阻断连接流程。
        /// </summary>
        /// <param name="camera">相机实体（持久化参数的来源）</param>
        /// <param name="serialNumber">相机序列号</param>
        private async Task ApplyPersistedParamsToDeviceAsync(CameraEntity camera, string serialNumber)
        {
            if (camera == null || string.IsNullOrWhiteSpace(serialNumber)) return;

            foreach (var descriptor in HikDescriptor.All)
            {
                if (!TryGetPersistedParamValue(camera, descriptor.ParamName, out var value))
                    continue;

                try
                {
                    var result = await _driver.SetParamAsync(serialNumber, descriptor.ParamName, value);
                    if (!result.Success)
                    {
                        // 业务失败无异常对象，LogHelper.Error 已支持 ex 为 null，仅记录失败原因
                        LogHelper.Error($"下发持久化参数 {descriptor.ParamName} 失败：{result.Message}", null);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"下发持久化参数 {descriptor.ParamName} 异常", ex);
                }
            }
        }

        /// <summary>
        /// 从实体读取持久化的参数值
        /// </summary>
        /// <param name="camera">相机实体</param>
        /// <param name="paramName">参数节点名</param>
        /// <param name="value">输出：持久化值</param>
        /// <returns>存在有效持久化值时返回 true；空字符串等无效值返回 false</returns>
        private static bool TryGetPersistedParamValue(CameraEntity camera, string paramName, out object value)
        {
            value = null;

            var paramConfig = camera?.CameraAllConfig?.ParamConfig;
            if (paramConfig == null) return false;

            switch (paramName)
            {
                case HikParamName.ExposureTime:
                    value = paramConfig.ExposureTime;
                    return true;
                case HikParamName.Gain:
                    value = paramConfig.Gain;
                    return true;
                case HikParamName.AcquisitionFrameRate:
                    value = paramConfig.FrameRate;
                    return true;
                case HikParamName.PixelFormat:
                    if (string.IsNullOrWhiteSpace(paramConfig.PixelFormat)) return false;
                    value = paramConfig.PixelFormat;
                    return true;
                case HikParamName.TriggerMode:
                    if (string.IsNullOrWhiteSpace(paramConfig.TriggerMode)) return false;
                    value = paramConfig.TriggerMode;
                    return true;
                case HikParamName.TriggerSource:
                    if (string.IsNullOrWhiteSpace(paramConfig.TriggerSource)) return false;
                    value = paramConfig.TriggerSource;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>硬件层描述符 → 领域参数规格（跨层映射，避免领域层依赖硬件层类型）</summary>
        /// <param name="descriptor">硬件层描述符</param>
        /// <returns>领域层参数规格</returns>
        private static ParamSpec ToParamSpec(HikDescriptor descriptor)
        {
            return new ParamSpec
            {
                ParamName = descriptor.ParamName,
                DisplayName = descriptor.DisplayName,
                Unit = descriptor.Unit,
                Group = descriptor.Group == HikParamGroup.HighFrequency
                    ? ParamGroup.HighFrequency
                    : ParamGroup.Basic,
                ValueType = ToParamValueType(descriptor.ValueType),
                Min = descriptor.Min,
                Max = descriptor.Max,
                Step = descriptor.Step,
                IsWritable = descriptor.IsWritable,
                Options = descriptor.EnumEntries == null
                    ? new List<string>()
                    : new List<string>(descriptor.EnumEntries)
            };
        }

        /// <summary>硬件层值类型 → 领域值类型</summary>
        /// <param name="valueType">硬件层值类型</param>
        /// <returns>领域层值类型</returns>
        private static ParamValueType ToParamValueType(HikParamValueType valueType)
        {
            switch (valueType)
            {
                case HikParamValueType.Int: return ParamValueType.Int;
                case HikParamValueType.Enum: return ParamValueType.Enum;
                case HikParamValueType.Bool: return ParamValueType.Bool;
                default: return ParamValueType.Float;
            }
        }

        /// <summary>
        /// 把参数值写入实体的参数配置
        /// 【说明】按相机节点名分派到对应业务字段，未知参数不写入，避免污染实体。
        /// </summary>
        /// <param name="camera">相机实体</param>
        /// <param name="paramName">参数节点名</param>
        /// <param name="value">参数值</param>
        private static void PersistParamValue(CameraEntity camera, string paramName, object value)
        {
            if (camera == null || value == null) return;

            var paramConfig = camera.CameraAllConfig.ParamConfig;
            if (paramConfig == null) return;

            switch (paramName)
            {
                case HikParamName.ExposureTime:
                    paramConfig.ExposureTime = Convert.ToDouble(value);
                    break;
                case HikParamName.Gain:
                    paramConfig.Gain = Convert.ToDouble(value);
                    break;
                case HikParamName.AcquisitionFrameRate:
                    paramConfig.FrameRate = Convert.ToDouble(value);
                    break;
                case HikParamName.PixelFormat:
                    paramConfig.PixelFormat = value.ToString();
                    break;
                case HikParamName.TriggerMode:
                    paramConfig.TriggerMode = value.ToString();
                    break;
                case HikParamName.TriggerSource:
                    paramConfig.TriggerSource = value.ToString();
                    break;
            }
        }

        /// <summary>
        /// 把组态设备 ID 解析为硬件层所需的相机序列号
        /// </summary>
        /// <param name="deviceId">组态设备 ID</param>
        /// <param name="serialNumber">输出：相机序列号</param>
        /// <returns>设备存在且为相机类型、且序列号非空时返回 true</returns>
        private bool TryResolveSerialNumber(string deviceId, out string serialNumber)
        {
            serialNumber = null;

            var camera = GetCameraEntity(deviceId);
            if (camera == null) return false;

            serialNumber = camera.CameraAllConfig.ConnectConfig.SerialNumber;
            return !string.IsNullOrWhiteSpace(serialNumber);
        }

        /// <summary>
        /// 释放驱动资源（驱动内部会停流、断开所有相机并反初始化 SDK）
        /// </summary>
        public void ReleaseAll()
        {
            try
            {
                _driver.Dispose();
            }
            catch
            {
                // 忽略释放异常，保证清理不中断
            }
        }
    }
}
