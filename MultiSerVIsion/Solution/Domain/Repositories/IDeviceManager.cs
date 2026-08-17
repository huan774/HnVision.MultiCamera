using MultiSerVIsion.Solution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Repositories
{
    public interface IDeviceManager
    {
        List<DeviceEntity> GetAllDevices();

        /// <summary>根据设备ID获取设备</summary>
        DeviceEntity GetDeviceById(string deviceId);

        /// <summary>泛型获取指定类型设备（核心：替代分类型管理器）</summary>
        List<T> GetDevices<T>() where T : DeviceEntity;

        /// <summary>按分组获取设备（原Repository越界方法，迁移到此处）</summary>
        List<DeviceEntity> GetDevicesByGroup(string groupTag);

        /// <summary>获取设备所属分组（原Repository越界方法，迁移到此处）</summary>
        string GetGroupTag(string deviceId);

        /// <summary>新增设备（自动执行自校验）</summary>
        /// <returns>新增成功返回true，校验失败返回false</returns>
        bool AddDevice(DeviceEntity device);

        /// <summary>根据ID移除设备</summary>
        bool RemoveDevice(string deviceId);

        /// <summary>
        /// 更新单个设备（内存引用更新 + 同步持久化到磁盘）
        /// 【职责】Manager 统一协调内存与仓储，应用层不得直接改实体绕过持久化
        /// </summary>
        void Update(DeviceEntity device);

        /// <summary>清空所有设备</summary>
        void ClearAllDevices();
      

        /// <summary>从磁盘加载全部设备到内存</summary>
        void LoadFromStorage();

        /// <summary>将内存中所有设备持久化到磁盘</summary>
        void SaveToStorage();
      
    }
}
