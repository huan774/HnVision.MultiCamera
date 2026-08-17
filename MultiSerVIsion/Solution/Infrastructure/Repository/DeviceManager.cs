using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiSerVIsion.Solution.Infrastructure.Repository
{
    /// <summary>
    /// 设备管理器：内存聚合根 + 持久化协调器。
    /// 【职责】内存为唯一权威源，负责设备的增删改查与业务校验；
    /// 每次变更后调用仓储 SaveAll 全量落盘，仓储只负责文件快照读写（各司其职）。
    /// </summary>
    public class DeviceManager : IDeviceManager
    {
        private readonly List<DeviceEntity> _inMemoryStore = new List<DeviceEntity>();
        private readonly IDeviceRepository _deviceRepository;

        public DeviceManager(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public List<DeviceEntity> GetAllDevices()
        {
            // 返回副本，避免外部直接修改集合结构；如需高性能可返回AsReadOnly
            return _inMemoryStore.ToList();
        }

        public DeviceEntity GetDeviceById(string deviceId)
        {
            return _inMemoryStore.FirstOrDefault(d => d.DeviceId == deviceId);
        }

        public List<T> GetDevices<T>() where T : DeviceEntity
        {
            return _inMemoryStore.OfType<T>().ToList();
        }

        public List<DeviceEntity> GetDevicesByGroup(string groupTag)
        {
            return _inMemoryStore
                .Where(d => string.Equals(d.GroupTage, groupTag, StringComparison.Ordinal))
                .ToList();
        }

        public string GetGroupTag(string deviceId)
        {
            return GetDeviceById(deviceId)?.GroupTage ?? string.Empty;
        }

        public bool AddDevice(DeviceEntity device)
        {
            // 1. 基础空校验
            if (device == null) return false;
            if (string.IsNullOrWhiteSpace(device.DeviceId)) return false;

            // 2. 防重复：ID已存在则拒绝新增
            if (_inMemoryStore.Any(d => d.DeviceId == device.DeviceId))
                return false;

            // 3. 执行实体自校验，不合法不入库
            var validateResult = device.SelfValidate();
            if (!validateResult.IsValid)
                return false;

            _inMemoryStore.Add(device);
            // 内存为唯一权威源：新增后全量落盘，避免与仓储单条操作产生数据分叉
            _deviceRepository.SaveAll(_inMemoryStore.ToList());
            return true;
        }

        public bool RemoveDevice(string deviceId)
        {
            var target = GetDeviceById(deviceId);
            if (target == null) return false;

            // 内存与磁盘必须同时删除，避免数据不一致
            _inMemoryStore.Remove(target);
            // 内存为唯一权威源：删除后全量落盘，保持内存与文件一致
            _deviceRepository.SaveAll(_inMemoryStore.ToList());
            return true;
        }

        public void ClearAllDevices()
        {
            _inMemoryStore.Clear();
            // 同步清空磁盘存储，保持内存与文件一致
            _deviceRepository.SaveAll(new List<DeviceEntity>());
        }

        public void LoadFromStorage()
        {
            var devices = _deviceRepository.LoadAll();
            _inMemoryStore.Clear();
            _inMemoryStore.AddRange(devices);
        }

        public void SaveToStorage()
        {
            // 传入内存集合的副本，避免序列化过程中集合被修改
            _deviceRepository.SaveAll(_inMemoryStore.ToList());
        }

        public void Update(DeviceEntity device)
        {
            if (device == null) return;

            // 内存中保存的是同一实体引用，字段已由调用方更新；此处只需全量同步落盘
            _deviceRepository.SaveAll(_inMemoryStore.ToList());
        }
    }
}
