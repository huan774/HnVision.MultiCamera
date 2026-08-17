using MultiSerVIsion.Solution.Domain.Entities;
using MultiSerVIsion.Solution.Domain.Repositories;
using MultiSerVIsion.Solution.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MultiSerVIsion.Solution.Infrastructure.Repository
{
    /// <summary>
    /// 设备仓储：唯一职责是工程文件（JSON）的全量读写持久化。
    /// 【原则】不缓存、不校验、不做业务决策，仅提供 LoadAll / SaveAll 全量快照操作；
    /// 内存聚合、业务校验与变更协调由 DeviceManager 统一负责（各司其职）。
    /// </summary>
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
        }

        /// <summary>从磁盘全量加载设备列表（文件不存在或损坏时返回空列表，避免程序崩溃）</summary>
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
                // 配置文件损坏时兜底返回空，避免程序崩溃
                return new List<DeviceEntity>();
            }
        }

        /// <summary>全量覆盖保存设备列表（原子写入：先写临时文件再替换，避免断电导致配置半写损坏）</summary>
        public void SaveAll(List<DeviceEntity> devices)
        {
            var tempPath = _storagePath + ".tmp";
            var json = JsonSerializer.Serialize(devices, typeof(List<DeviceEntity>), JsonConfigHelper.Default);
            File.WriteAllText(tempPath, json);

            if (File.Exists(_storagePath))
                File.Replace(tempPath, _storagePath, null);
            else
                File.Move(tempPath, _storagePath);
        }
    }
}
