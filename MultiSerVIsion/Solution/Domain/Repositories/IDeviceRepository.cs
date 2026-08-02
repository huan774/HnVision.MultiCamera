using MultiSerVIsion.Solution.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Domain.Repositories
{
    public interface IDeviceRepository
    {
        List<DeviceEntity> LoadAll();

        /// <summary>全量覆盖保存所有设备到工程文件</summary>
        void SaveAll(List<DeviceEntity> devices);


        // ========== 可选：单条操作（兼容旧调用，过渡阶段使用） ==========
        /// <summary>根据ID查询单个设备（全量加载后匹配，不单独读文件）</summary>
        DeviceEntity GetById(string devId);

        /// <summary>新增单个设备（追加到全量集合后重写文件）</summary>
        void Add(DeviceEntity device);

        /// <summary>移除单个设备</summary>
        bool Remove(string devId);

        /// <summary>更新单个设备</summary>
        void Update(DeviceEntity device); 
    }
}
