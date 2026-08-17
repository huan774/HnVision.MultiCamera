using MultiSerVIsion.Solution.Domain.Entities;
using System.Collections.Generic;

namespace MultiSerVIsion.Solution.Domain.Repositories
{
    /// <summary>
    /// 设备仓储接口：唯一职责是工程文件（JSON）的全量快照读写。
    /// 【原则】单条增删改由 DeviceManager 在内存中完成后再全量落盘，仓储不提供单条文件操作，各司其职。
    /// </summary>
    public interface IDeviceRepository
    {
        /// <summary>从磁盘全量加载所有设备</summary>
        List<DeviceEntity> LoadAll();

        /// <summary>全量覆盖保存所有设备到工程文件</summary>
        void SaveAll(List<DeviceEntity> devices);
    }
}
