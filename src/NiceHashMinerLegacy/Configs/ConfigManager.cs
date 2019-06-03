using System;
using System.Linq;
using System.IO.Compression;
using MinerPluginToolkitV1.Configs;
using NiceHashMiner.Configs.Data;
using NiceHashMiner.Devices;
using NiceHashMiner.Utils;
using NiceHashMinerLegacy.Common;
using System.Collections.Generic;
using System.IO;

namespace NiceHashMiner.Configs
{
    public static class ConfigManager
    {
        private const string Tag = "ConfigManager";
        public static GeneralConfig GeneralConfig = new GeneralConfig();

        private static string GeneralConfigPath => Paths.ConfigsPath("General.json");

        private static object _lock = new object();
        // helper variables
        private static bool _isGeneralConfigFileInit;

        // backups
        private static GeneralConfig _generalConfigBackup = new GeneralConfig();
        private static Dictionary<string, DeviceConfig> _benchmarkConfigsBackup = new Dictionary<string, DeviceConfig>();

        private static void CreateBackupArchive(Version backupVersion)
        {
            try
            {
                var backupZipPath = Paths.RootPath("backups", $"configs_{backupVersion.ToString()}.zip");
                var dirPath = Path.GetDirectoryName(backupZipPath);
                if (Directory.Exists(dirPath) == false)
                {
                    Directory.CreateDirectory(dirPath);
                }
                ZipFile.CreateFromDirectory(Paths.ConfigsPath(), backupZipPath);
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"Error while creating backup archive: {e.Message}");
            }
        }

        private static void TryMigrate()
        {
            try
            {
                var dirPath = Paths.ConfigsPath();
                var benchmarks = Directory.GetFiles(dirPath, "*.json", SearchOption.TopDirectoryOnly).Where(path => path.Contains("benchmark_") && !path.Contains("_OLD"));
                foreach (var benchFile in benchmarks)
                {
                    File.Move(benchFile, benchFile.Replace("benchmark_", "device_settings_"));
                }
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"Error while trying to migrate: {e.Message}");
            }
        }

        public static void InitializeConfig()
        {
            TryMigrate();
            // init defaults
            GeneralConfig.SetDefaults();
            GeneralConfig.hwid = WindowsManagementObjectSearcher.GetCpuID();
            // load file if it exist
            var fromFile = InternalConfigs.ReadFileSettings<GeneralConfig>(GeneralConfigPath);
            if (fromFile != null)
            {
                var asmVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                if (fromFile.ConfigFileVersion != null && asmVersion.CompareTo(fromFile.ConfigFileVersion) != 0)
                {
                    Logger.Info(Tag, "Config file is differs from version of NiceHashMiner... Creating backup archive");
                    CreateBackupArchive(fromFile.ConfigFileVersion);
                }
                if (fromFile.ConfigFileVersion != null)
                {
                    // set config loaded from file
                    _isGeneralConfigFileInit = true;
                    GeneralConfig = fromFile;
                    GeneralConfig.FixSettingBounds();
                }
                else
                {
                    Logger.Info(Tag, "Loaded Config file no version detected falling back to defaults.");
                }
            }
            else
            {
                GeneralConfigFileCommit();
            }
        }

        public static bool GeneralConfigIsFileExist()
        {
            return _isGeneralConfigFileInit;
        }

        public static void CreateBackup()
        {
            _generalConfigBackup = MemoryHelper.DeepClone(GeneralConfig);
            _benchmarkConfigsBackup = new Dictionary<string, DeviceConfig>();
            foreach (var cDev in AvailableDevices.Devices)
            {
                _benchmarkConfigsBackup[cDev.Uuid] = cDev.GetDeviceConfig();
            }
        }

        public static bool IsRestartNeeded()
        {
            return GeneralConfig.DebugConsole != _generalConfigBackup.DebugConsole
                   || GeneralConfig.NVIDIAP0State != _generalConfigBackup.NVIDIAP0State
                   || GeneralConfig.LogToFile != _generalConfigBackup.LogToFile
                   || GeneralConfig.DisableWindowsErrorReporting != _generalConfigBackup.DisableWindowsErrorReporting
                   || GeneralConfig.Use3rdPartyMiners != _generalConfigBackup.Use3rdPartyMiners;
        }

        public static void GeneralConfigFileCommit()
        {
            CommitBenchmarks();
            InternalConfigs.WriteFileSettings(GeneralConfigPath, GeneralConfig);
        }

        private static string GetDeviceSettingsPath(string uuid)
        {
            return Paths.ConfigsPath($"device_settings_{uuid}.json");
        }

        public static void CommitBenchmarks()
        {
            foreach (var cDev in AvailableDevices.Devices)
            {
                CommitBenchmarksForDevice(cDev);
            }
        }

        public static void CommitBenchmarksForDevice(ComputeDevice device)
        {
            // since we have multitrheaded benchmarks
            lock (_lock)
            lock (device)
            {
                var devSettingsPath = GetDeviceSettingsPath(device.Uuid);
                var configs = device.GetDeviceConfig();
                InternalConfigs.WriteFileSettings(devSettingsPath, configs);
            }
        }

        // TODO this should be obsolete
        public static void AfterDeviceQueryInitialization()
        {
            // extra check (probably will never happen but just in case)
            AvailableDevices.RemoveInvalidDevs();
            // create/init device configs
            foreach (var device in AvailableDevices.Devices)
            {
                var devSettingsPath = GetDeviceSettingsPath(device.Uuid);
                var currentConfig = InternalConfigs.ReadFileSettings<DeviceConfig>(devSettingsPath);
                if (currentConfig != null)
                {
                    device.SetDeviceConfig(currentConfig);
                }
            }
            // save settings
            GeneralConfigFileCommit();
        }
    }
}
