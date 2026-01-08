using NHM.Common;
using NHM.Common.Configs;
using NHMCore.ApplicationState;
using NHMCore.Configs.Data;
using NHMCore.Mining;
using NHMCore.Mining.Plugins;
using NHMCore.Notifications;
using NHMCore.Schedules;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NHMCore.Configs
{
    public static class ConfigManager
    {
        private const string Tag = "ConfigManager";

        public static bool IsVersionChanged { get; private set; } = false;

        // TODO set to internal and refactor external usage
        private static GeneralConfig GeneralConfig { get; set; } = new GeneralConfig();

        private static string GeneralConfigPath => Paths.ConfigsPath("General.json");
        private static string ScheduleConfigPath => Paths.ConfigsPath("Schedule.json");

        private static object _lock = new object();

        // backups
        private static GeneralConfigBackup _generalConfigBackup = null;
        private static Dictionary<string, DeviceConfig> _benchmarkConfigsBackup = new Dictionary<string, DeviceConfig>();

        public static EventHandler<bool> ShowRestartRequired;

        private static void CreateBackupArchive(Version backupVersion)
        {
            try
            {
                var backupZipPath = Paths.RootPath("backups", $"configs_{backupVersion}.zip");
                var dirPath = Path.GetDirectoryName(backupZipPath);
                Paths.EnsureDirectoryPath(dirPath);
                ZipFile.CreateFromDirectory(Paths.ConfigsPath(), backupZipPath);
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"Error while creating backup archive: {e.Message}");
            }
        }

        private static bool RestoreBackupArchive(Version backupVersion)
        {
            try
            {
                var backupZipPath = Paths.RootPath("backups", $"configs_{backupVersion}.zip");
                if (File.Exists(backupZipPath))
                {
                    Directory.Delete(Paths.ConfigsPath(), true);
                    ZipFile.ExtractToDirectory(backupZipPath, Paths.ConfigsPath());
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(Tag, $"Error while creating backup archive: {e.Message}");
            }
            return false;
        }

        public static void InitializeConfig()
        {
            // init defaults
            GeneralConfig.SetDefaults();
            ToSSetings.Instance.Hwid = ApplicationStateManager.RigID();

            var asmVersion = new Version(NHMApplication.ProductVersion);

            // load file if it exist
            var fromFile = InternalConfigs.ReadFileSettings<GeneralConfig>(GeneralConfigPath);
            if (fromFile != null)
            {
                if (fromFile.ConfigFileVersion != null && asmVersion.CompareTo(fromFile.ConfigFileVersion) != 0)
                {
                    IsVersionChanged = true;
                    Logger.Info(Tag, "Config file differs from version of NiceHashMiner... Creating backup archive");
                    CreateBackupArchive(fromFile.ConfigFileVersion);
                    if (RestoreBackupArchive(asmVersion))//check if we have backup version
                    {
                        fromFile = InternalConfigs.ReadFileSettings<GeneralConfig>(GeneralConfigPath);
                    }
                }
                if (fromFile?.ConfigFileVersion != null)
                {
                    GeneralConfig = fromFile;
                    GeneralConfig.FixSettingBounds();
                    // TODO temp 
                    GeneralConfig.PropertyChanged += BalanceAndExchangeRates.Instance.GeneralConfig_PropertyChanged;
                }
                else
                {
                    Logger.Info(Tag, "Loaded Config file no version detected falling back to defaults.");
                }
            }
            else
            {
                var fromFileInfo = InternalConfigs.ReadFileSettings<GeneralConfigOld>(GeneralConfigPath);
                if(fromFileInfo != null)
                {
                    GeneralConfig.SetValues(fromFileInfo);
                    GeneralConfig.FixSettingBounds();
                    GeneralConfig.PropertyChanged += BalanceAndExchangeRates.Instance.GeneralConfig_PropertyChanged;
                }
                
                GeneralConfigFileCommit();
            }
        }

        public static void CreateBackup()
        {
            _generalConfigBackup = new GeneralConfigBackup
            {
                DebugConsole = LoggingDebugConsoleSettings.Instance.DebugConsole,
                LogToFile = LoggingDebugConsoleSettings.Instance.LogToFile,
                LogMaxFileSize = LoggingDebugConsoleSettings.Instance.LogMaxFileSize,
                DisableWindowsErrorReporting = WarningSettings.Instance.DisableWindowsErrorReporting,
                AllowMultipleInstances = MiscSettings.Instance.AllowMultipleInstances,
            };
            _benchmarkConfigsBackup = new Dictionary<string, DeviceConfig>();
            foreach (var cDev in AvailableDevices.Devices)
            {
                _benchmarkConfigsBackup[cDev.Uuid] = cDev.GetDeviceConfig();
            }
        }

        public static bool IsRestartNeeded()
        {
            if (_generalConfigBackup == null) return false;
            return LoggingDebugConsoleSettings.Instance.DebugConsole != _generalConfigBackup.DebugConsole
                   || LoggingDebugConsoleSettings.Instance.LogToFile != _generalConfigBackup.LogToFile
                   || LoggingDebugConsoleSettings.Instance.LogMaxFileSize != _generalConfigBackup.LogMaxFileSize
                   || WarningSettings.Instance.DisableWindowsErrorReporting != _generalConfigBackup.DisableWindowsErrorReporting
                   || MiscSettings.Instance.AllowMultipleInstances != _generalConfigBackup.AllowMultipleInstances;
        }

        public static void GeneralConfigFileCommit()
        {
            ApplicationStateManager.App.Dispatcher.Invoke(() =>
            {
                var res = InternalConfigs.WriteFileSettings(GeneralConfigPath, GeneralConfig);
                if (!res)
                {
                    try
                    {
                        EventManager.Instance.AddEventGeneralCfg();
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("ConfigManager", ex.Message);
                    }
                }
                ShowRestartRequired?.Invoke(null, IsRestartNeeded());
            });
        }

        public static void ScheduleConfigFileCommit()
        {
            ApplicationStateManager.App.Dispatcher.Invoke(() =>
            {
                InternalConfigs.WriteFileSettings(ScheduleConfigPath, SchedulesManager.Instance.Schedules);
            });
        }

        public static void DeviceConfigFileCommit(ComputeDevice device)
        {
            var devSettingsPath = GetDeviceSettingsPath(device.Uuid);
            var configs = device.GetDeviceConfig();
            InternalConfigs.WriteFileSettings(devSettingsPath, configs);
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
                if (MinerPluginsManager.PluginsForEulaConfirm.Any() && Launcher.IsUpdated)
                {
                    foreach (var plugin in MinerPluginsManager.PluginsForEulaConfirm)
                    {
                        foreach (var algo in currentConfig.PluginAlgorithmSettings)
                        {
                            if (algo.PluginUUID == plugin.PluginUUID)
                            {
                                algo.Enabled = false;
                            }
                        }
                    }
                }
                if (currentConfig != null)
                {
                    device.SetDeviceConfig(currentConfig);
                }
            }
            // save settings
            CommitBenchmarks();
        }

        private class GeneralConfigBackup
        {
            public bool DebugConsole { get; set; }
            public bool LogToFile { get; set; }
            public long LogMaxFileSize { get; set; }
            public bool DisableWindowsErrorReporting { get; set; }
            public bool DisableDevicePowerModeSettings { get; set; }
            public bool AllowMultipleInstances { get; set; }
        }

        public static void SetDefaults()
        {
            GeneralConfig.SetDefaults();
        }

        public static void FixSettingBounds()
        {
            GeneralConfig.FixSettingBounds();
        }
    }
}
