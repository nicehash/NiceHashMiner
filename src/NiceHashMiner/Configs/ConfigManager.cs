using System;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using MinerPluginToolkitV1.Configs;
using NiceHashMiner.Configs.Data;
using NiceHashMiner.Devices;
using NiceHashMiner.Utils;
using NHM.Common;
using NHM.Common.Enums;

namespace NiceHashMiner.Configs
{
    public static class ConfigManager
    {
        private const string Tag = "ConfigManager";

        public static GeneralConfig GeneralConfig = new GeneralConfig();
        
        // extra composed settings
        public static RunAtStartup RunAtStartup = RunAtStartup.Instance;
        public static IdleMiningSettings IdleMiningSettings = IdleMiningSettings.Instance;
        public static TranslationsSettings TranslationsSettings = TranslationsSettings.Instance;
        public static CredentialsSettings CredentialsSettings = CredentialsSettings.Instance;

        private static string GeneralConfigPath => Paths.ConfigsPath("General.json");

        private static object _lock = new object();
        // helper variables
        private static bool _isGeneralConfigFileInit;

        public static bool IsMiningRegardlesOfProfit => GeneralConfig.MineRegardlessOfProfit;

        // backups
        private static GeneralConfigBackup _generalConfigBackup = new GeneralConfigBackup();
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
            GeneralConfig.hwid = ApplicationStateManager.RigID;
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
            _generalConfigBackup = new GeneralConfigBackup
            {
                DebugConsole = GeneralConfig.DebugConsole,
                NVIDIAP0State = GeneralConfig.NVIDIAP0State,
                LogToFile = GeneralConfig.LogToFile,
                DisableWindowsErrorReporting = GeneralConfig.DisableWindowsErrorReporting,
                Use3rdPartyMiners = GeneralConfig.Use3rdPartyMiners,
                GUIWindowsAlwaysOnTop = GeneralConfig.GUIWindowsAlwaysOnTop,
                DisableDeviceStatusMonitoring = GeneralConfig.DisableDeviceStatusMonitoring,
                DisableDevicePowerModeSettings = GeneralConfig.DisableDevicePowerModeSettings,
            };
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
                   || GeneralConfig.Use3rdPartyMiners != _generalConfigBackup.Use3rdPartyMiners
                   || GeneralConfig.GUIWindowsAlwaysOnTop != _generalConfigBackup.GUIWindowsAlwaysOnTop
                   || GeneralConfig.DisableDeviceStatusMonitoring != _generalConfigBackup.DisableDeviceStatusMonitoring
                   || GeneralConfig.DisableDevicePowerModeSettings != _generalConfigBackup.DisableDevicePowerModeSettings;
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

        public static void InitDeviceSettings()
        {
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

        private class GeneralConfigBackup
        {
            public bool DebugConsole { get; set; }
            public bool NVIDIAP0State { get; set; }
            public bool LogToFile { get; set; }
            public bool DisableWindowsErrorReporting { get; set; }
            public Use3rdPartyMiners Use3rdPartyMiners { get; set; }
            public bool GUIWindowsAlwaysOnTop { get; set; }
            public bool DisableDeviceStatusMonitoring { get; set; }
            public bool DisableDevicePowerModeSettings { get; set; }
        }
    }
}
