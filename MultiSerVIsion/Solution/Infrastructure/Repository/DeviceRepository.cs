using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Shared.Exceptions;
using MultiSerVIsion.Solution.Shared.Helpers;
using MvCameraControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace MultiSerVIsion.Solution.Infrastructure.Repository
{
    public class DeviceRepository : IDeviceRepository
    {

        private readonly string _storagePath;

        public DeviceRepository()
        {
            _storagePath = Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
           Shared.GlobalConst.AppDataFolder,
           Shared.GlobalConst.DeviceJsonFIleName
           );
            LogHelper.Info($"当前设备存储文件路径：{_storagePath}");

            string folder = Path.GetDirectoryName(_storagePath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
/*
            if (!File.Exists(_storagePath))
            {
                WriteAll(new List<DeviceEntity>());
            }*/
        }
        public List<DeviceEntity> LoadAll()
        {
            if (!File.Exists(_storagePath))
                return new List<DeviceEntity>();

            var json = File.ReadAllText(_storagePath);
            try
            {
                return JsonSerializer.Deserialize<List<DeviceEntity>>(json, JsonConfigHelper.Default)
                       ?? new List<DeviceEntity>();
            }
            catch (JsonException)
            {
                // 配置文件损坏时兜底返回空，避免程序崩溃，可按需补充日志
                return new List<DeviceEntity>();
            }
        }

        public void SaveAll(List<DeviceEntity> devices)
        {
            // 原子写入：先写临时文件，再替换原文件，避免断电/崩溃导致配置半写损坏
            var tempPath = _storagePath + ".tmp";
            var json = JsonSerializer.Serialize(devices, typeof(List<DeviceEntity>), JsonConfigHelper.Default);
            File.WriteAllText(tempPath, json);

            if (File.Exists(_storagePath))
                File.Replace(tempPath, _storagePath, null);
            else
                File.Move(tempPath, _storagePath);
        }

       /* public DeviceEntity GetById(string devId)
        {
            return LoadAll().FirstOrDefault(d => d.DeviceId == devId);
        }
*/
        public void Add(DeviceEntity device)
        {
            var all = LoadAll();
            all.Add(device);
            SaveAll(all);
        }

        public bool Remove(string devId)
        {
            var all = LoadAll();
            var target = all.FirstOrDefault(d => d.DeviceId == devId);
            if (target == null) return false;
            all.Remove(target);
            SaveAll(all);
            return true;
        }

        public void Update(DeviceEntity device)
        {
            var all = LoadAll();
            var index = all.FindIndex(d => d.DeviceId == device.DeviceId);
            if (index < 0) return;
            all[index] = device;
            SaveAll(all);
        }

        /*    private List<DeviceEntity> ReadAll()
            {

                if (!File.Exists(_storagePath))
                {
                    return new List<DeviceEntity>();
                }

                return FileloHelper.ReadJsonFile<List<DeviceEntity>>(_storagePath, JsonConfigHelper.Default);
            }

            private void WriteAll(List<DeviceEntity> list)
            {
                FileloHelper.WriteJsonFile(_storagePath, list, JsonConfigHelper.Default);
            }

            public DeviceEntity GetById(string devId)
            {
                var list = ReadAll();
                return list.FirstOrDefault(d => d.DeviceId == devId);
            }

            public void Update(DeviceEntity deviceEntity)
            {
                try
                {
                    var list = ReadAll();
                    var index = list.FindIndex(d => d.DeviceId == deviceEntity.DeviceId);
                    if (index == -1)
                        throw new KeyNotFoundException($"未找到设备{deviceEntity.DeviceId},无法更新");

                    list[index] = deviceEntity;
                    WriteAll(list);
                    LogHelper.Info($"更新设备配置：{deviceEntity.DeviceName}({deviceEntity.DeviceId})");
                }
                catch (Exception ex)
                {
                    LogHelper.Info($"仓储Add写入失败{ex}");
                    throw;
                }
            }
            public void Add(DeviceEntity deviceEntity)
            {
                var list = ReadAll();
                if (list.Any(d => d.DeviceId == deviceEntity.DeviceId))
                    throw new InvalidOperationException($"设备ID{deviceEntity.DeviceId}");

                list.Add(deviceEntity);
                WriteAll(list);
                LogHelper.Info($"新增设备成功：{deviceEntity.DeviceName}({deviceEntity.DeviceId})");

            }
            public bool Remove(string deviceEntity)
            {
                var list = ReadAll();
                var target = list.FirstOrDefault(d => d.DeviceId == deviceEntity);
                if (target == null) return false;

                list.Remove(target);
                WriteAll(list);
                LogHelper.Info($"删除设备成功：{target.DeviceName}({deviceEntity})");

                return true;
            }
            public List<DeviceEntity> LoadAll()
            {
                try
                {
                    return ReadAll();
                }
                catch (StorageIoException ex)
                {

                    LogHelper.Error($"读取设备配置文件失败，禁止生成空数据覆盖原有文件：{ex.Message}", ex);
                    throw;
                }
            }
            public void SaveAll(List<DeviceEntity> devices)
            {
                var json = JsonSerializer.Serialize(devices, typeof(List<DeviceEntity>), new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_storagePath, json);
            }

            public string GetGroupTag(string devId)
            {
                var dev = GetById(devId);
                return dev.GroupTage ?? string.Empty;
            }
            public List<DeviceEntity> GetByGroup(string groupId)
            {
                var list = ReadAll();
                return list.Where(d => d.GroupTage == groupId).ToList();
            }
          */
    }
}
