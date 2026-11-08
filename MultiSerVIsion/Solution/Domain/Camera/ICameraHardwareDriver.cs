using MultiSerVIsion.Solution.Domain.Camera;
using MultiSerVIsion.Solution.Domain.Device;
using MultiSerVIsion.Solution.Domain.Enums;
using MultiSerVIsion.Solution.Infrastructure.Camera.HikVision;
using MultiSerVIsion.Solution.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Camera
{
    /// <summary>
    /// 相机硬件驱动统一接口
    /// 【封装原则】绝不暴露任何 SDK 原生类型、错误码、句柄对象；
    /// 上层仅通过「序列号 + 业务配置 + 统一帧回调」与驱动交互。
    /// </summary>
    public interface ICameraHardwareDriver : IDisposable
    {
        /// <summary>/// 扫描所有在线相机/// </summary>
        /// <returns>扫描到的相机设备列表</returns>
        Task<OperationResult<List<CameraDeviceDto>>> ScanAsync();

        /// <summary> /// 测试连接（登录后立即登出，不占用句柄）
        /// <param name="config">连接参数（至少含序列号）</param>
        Task<OperationResult> TestConnectAsync(CameraConnectConfig config);

        /// <summary>/// 正式连接相机（登录并缓存句柄） /// </summary>
        /// <param name="config">连接参数（至少含序列号）</param>
        Task<OperationResult> ConnectAsync(CameraConnectConfig config);

        /// <summary>/// 断开相机（停流、登出并释放句柄缓存）
        /// </summary> /// <param name="serialNumber">相机序列号</param>
        Task<OperationResult> DisconnectAsync(string serialNumber);

        /// <summary>  /// 开启采流（主动取流模式） /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="frameCallback">帧数据回调（在采集线程触发，参数为统一业务帧）</param>
        Task<OperationResult> StartStreamAsync(string serialNumber, Action<CameraFrame> frameCallback);

        /// <summary> /// 停止采流/// </summary>
        /// <param name="serialNumber">相机序列号</param>
        Task<OperationResult> StopStreamAsync(string serialNumber);

        /// <summary>
        /// 按采集模式显式设置触发源与触发模式（连续=关闭触发，软触发=开启触发）。
        /// 【互斥】采集过程中不允许直接切换，需先停止采流，否则 SDK 配置不生效。
        /// </summary>
        /// <param name="serialNumber">相机序列号。</param>
        /// <param name="mode">目标采集模式。</param>
        /// <returns>操作结果。</returns>
        Task<OperationResult> SetAcquisitionModeAsync(string serialNumber, AcquisitionMode mode);

        /// <summary>
        /// 软件触发一次（仅在软触发模式下有效）。
        /// </summary>
        /// <param name="serialNumber">相机序列号。</param>
        /// <returns>操作结果；帧通过 IFrameSource 回调输出。</returns>
        Task<OperationResult> SoftwareTriggerOnceAsync(string serialNumber);

        /// <summary>
        /// 获取指定序列号的图源（多相机各自独立）。
        /// </summary>
        /// <param name="serialNumber">相机序列号。</param>
        /// <returns>图源实例；未连接返回 null。</returns>
        IFrameSource GetFrameSource(string serialNumber);
        /// <summary>读取参数当前值</summary>
        Task<OperationResult<object>> GetParamAsync(string serialNumber, string paramName);

        /// <summary>写入参数</summary>
        Task<OperationResult> SetParamAsync(string serialNumber, string paramName, object value);

        /// <summary>查询参数元信息（范围/可写性/枚举项）</summary>
        Task<OperationResult<HikDescriptor>> GetParamDescriptorAsync(string serialNumber, string paramName);

        /// <summary>
        /// 读取参数：一次性返回「当前值 + 完整规格（范围 / 步长 / 可选项 / 可写性）」。
        /// 【说明】旧接口只返回当前值并丢弃范围与可选项，无法支撑界面限幅与实体校验，故新增本方法。
        /// </summary>
        /// <param name="serialNumber">相机序列号</param>
        /// <param name="paramName">参数节点名</param>
        /// <returns>成功时 Data 为已回填规格且含 CurrentValue 的描述符</returns>
        Task<OperationResult<HikDescriptor>> ReadParamAsync(string serialNumber, string paramName);
    }
}
