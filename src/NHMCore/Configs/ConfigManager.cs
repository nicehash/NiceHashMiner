using MinerPluginToolkitV1.Configs;
using NHM.Common;
using NHMCore.Configs.Data;
using NHMCore.Mining;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace NHMCore.Configs
{
    public static class ConfigManager
    {
        private const string Tag = "ConfigManager";

        public static bool IsVersionChanged { get; private set; } = false;

        public static GeneralConfig GeneralConfig { get; private set; } = new GeneralConfig();
        
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

        private static bool RestoreBackupArchive(Version backupVersion)
        {
            try
            {
                var backupZipPath = Paths.RootPath("backups", $"configs_{backupVersion.ToString()}.zip");
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
            GeneralConfig.hwid = ApplicationStateManager.RigID;

            var asmVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

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
#warning "DELETE this after 1.9.2.18"
            #region MIGRATE plugin UUIDs
            var findReplace = new Dictionary<string, string>
            {
                { "\"PluginUUID\": \"BMiner\",", "\"PluginUUID\": \"e5fbd330-7235-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"CCMinerTpruvot\",", "\"PluginUUID\": \"2257f160-7236-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"ClaymoreDual\",", "\"PluginUUID\": \"70984aa0-7236-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"cpuminer-opt\",", "\"PluginUUID\": \"92fceb00-7236-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"CryptoDredge\",", "\"PluginUUID\": \"d9c2e620-7236-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"Ethlargement\",", "\"PluginUUID\": \"efd40691-618c-491a-b328-e7e020bda7a3\"," },
                { "\"PluginUUID\": \"Ewbf\",", "\"PluginUUID\": \"f7d5dfa0-7236-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"ExamplePlugin\",", "\"PluginUUID\": \"455c4d98-a45d-45d6-98ca-499ce866b2c7\"," },
                { "\"PluginUUID\": \"GMinerCuda9.0+\",", "\"PluginUUID\": \"1b7019d0-7237-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"LolMinerBeam\",", "\"PluginUUID\": \"435f0820-7237-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"MiniZ\",", "\"PluginUUID\": \"59bba2c0-b1ef-11e9-8e4e-bb1e2c6e76b4\"," },
                { "\"PluginUUID\": \"NanoMiner\",", "\"PluginUUID\": \"a841b4b0-ae17-11e9-8e4e-bb1e2c6e76b4\"," },
                { "\"PluginUUID\": \"NBMiner\",", "\"PluginUUID\": \"6c07f7a0-7237-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"Phoenix\",", "\"PluginUUID\": \"f5d4a470-e360-11e9-a914-497feefbdfc8\"," },
                { "\"PluginUUID\": \"SGminerAvemore\",", "\"PluginUUID\": \"bc95fd70-e361-11e9-a914-497feefbdfc8\"," },
                { "\"PluginUUID\": \"SRBMiner\",", "\"PluginUUID\": \"85f507c0-b2ba-11e9-8e4e-bb1e2c6e76b4\"," },
                { "\"PluginUUID\": \"TeamRedMiner\",", "\"PluginUUID\": \"abc3e2a0-7237-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"TRex\",", "\"PluginUUID\": \"d47d9b00-7237-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"TTMiner\",", "\"PluginUUID\": \"f1945a30-7237-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"WildRig\",", "\"PluginUUID\": \"2edd8080-9cb6-11e9-a6b8-09e27549d5bb\"," },
                { "\"PluginUUID\": \"XMRig\",", "\"PluginUUID\": \"1046ea50-c261-11e9-8e4e-bb1e2c6e76b4\"," },
                { "\"PluginUUID\": \"XmrStak\",", "\"PluginUUID\": \"3d4e56b0-7238-11e9-b20c-f9f12eb6d835\"," },
                { "\"PluginUUID\": \"ZEnemy\",", "\"PluginUUID\": \"5532d300-7238-11e9-b20c-f9f12eb6d835\"," },
            };
            try
            {
                var deviceConfigs = Directory.GetFiles(Paths.ConfigsPath(), "device_settings_*.json", SearchOption.TopDirectoryOnly);
                foreach (var devConfigPath in deviceConfigs)
                {
                    try
                    {
                        var content = File.ReadAllText(devConfigPath);
                        var containsNameUUIDs = false;
                        foreach (var key in findReplace.Keys)
                        {
                            if (content.Contains(key))
                            {
                                containsNameUUIDs = true;
                                break;
                            }
                        }
                        if (!containsNameUUIDs) continue;
                        foreach (var kvp in findReplace)
                        {
                            content = content.Replace(kvp.Key, kvp.Value);
                        }
                        File.WriteAllText(devConfigPath, content);
                    }
                    catch
                    {}
                }
            }
            catch
            {}
            #endregion MIGRATE plugin UUIDs
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
            public bool GUIWindowsAlwaysOnTop { get; set; }
            public bool DisableDeviceStatusMonitoring { get; set; }
            public bool DisableDevicePowerModeSettings { get; set; }
        }
    }
}
