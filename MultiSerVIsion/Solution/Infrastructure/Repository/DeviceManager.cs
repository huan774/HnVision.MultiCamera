using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Infrastructure.Repository
{
    public class DeviceManager:IDeviceManager
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
                .Where(d => d.GroupTage.Equals(groupTag, StringComparison.Ordinal))
                .ToList();
        }

        public string GetGroupTag(string deviceId)
        {
            return GetDeviceById(deviceId).GroupTage;
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
            _deviceRepository.Add(device);
            return true;
        }

        public bool RemoveDevice(string deviceId)
        {
            var target = GetDeviceById(deviceId);
            if (target == null) return false;
            return _deviceRepository.Remove(deviceId); 
        }

        public void ClearAllDevices()
        {
            _inMemoryStore.Clear();
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
       /* public void Update(DeviceEntity device)
        {
             _deviceRepository.Update(device);
        }*/
    }
}
