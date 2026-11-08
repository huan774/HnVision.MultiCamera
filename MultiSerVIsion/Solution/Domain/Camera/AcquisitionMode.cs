namespace MultiSerVIsion.Solution.Domain.Camera
{
    /// <summary>
    /// 相机采集模式：定义相机的出帧方式。
    /// 【互斥语义】<see cref="Continuous"/> 与 <see cref="SoftTrigger"/> 互斥；
    /// 采集过程中不允许直接切换，必须先停止采集、切换后再重新开始，
    /// 否则 SDK 内部状态与配置会不一致（表现为配置不生效或取流异常）。
    /// </summary>
    public enum AcquisitionMode
    {
        /// <summary>连续采集：TriggerMode=Off，相机按自身帧率自由出帧。</summary>
        Continuous = 0,

        /// <summary>软触发：TriggerMode=On 且 TriggerSource=Software，仅在收到软触发命令时出帧。</summary>
        SoftTrigger = 1
    }
}
