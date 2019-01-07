using NiceHashMiner.Configs.ConfigJsonFile;
using NiceHashMiner.Configs.Data;
using NiceHashMiner.Devices;
using System.Collections.Generic;

namespace NiceHashMiner.Configs
{
    public static class ConfigManager
    {
        private const string Tag = "ConfigManager";
        public static GeneralConfig GeneralConfig = new GeneralConfig();

        // helper variables
        private static bool _isGeneralConfigFileInit;

        private static bool _isNewVersion;

        // for loading and saving
        private static readonly GeneralConfigFile GeneralConfigFile = new GeneralConfigFile();

        private static readonly Dictionary<string, DeviceBenchmarkConfigFile> BenchmarkConfigFiles =
            new Dictionary<string, DeviceBenchmarkConfigFile>();

        // backups
        private static GeneralConfig _generalConfigBackup = new GeneralConfig();

        private static Dictionary<string, DeviceBenchmarkConfig> _benchmarkConfigsBackup =
            new Dictionary<string, DeviceBenchmarkConfig>();

        public static void InitializeConfig()
        {
            // init defaults
            GeneralConfig.SetDefaults();
            GeneralConfig.hwid = Helpers.GetCpuID();
            // if exists load file
            GeneralConfig fromFile = null;
            if (GeneralConfigFile.IsFileExists())
            {
                fromFile = GeneralConfigFile.ReadFile();
            }
            // just in case
            if (fromFile != null)
            {
                // set config loaded from file
                _isGeneralConfigFileInit = true;
                GeneralConfig = fromFile;
                if (GeneralConfig.ConfigFileVersion == null
                    || GeneralConfig.ConfigFileVersion.CompareTo(System.Reflection.Assembly.GetExecutingAssembly()
                        .GetName().Version) != 0)
                {
                    if (GeneralConfig.ConfigFileVersion == null)
                    {
                        Helpers.ConsolePrint(Tag, "Loaded Config file no version detected falling back to defaults.");
                        GeneralConfig.SetDefaults();
                    }
                    Helpers.ConsolePrint(Tag, "Config file is from an older version of NiceHashMiner..");
                    _isNewVersion = true;
                    GeneralConfigFile.CreateBackup();
                }
                GeneralConfig.FixSettingBounds();
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
            _benchmarkConfigsBackup = new Dictionary<string, DeviceBenchmarkConfig>();
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                _benchmarkConfigsBackup[cDev.Uuid] = cDev.GetAlgorithmDeviceConfig();
            }
        }

        public static void RestoreBackup()
        {
            // restore general
            GeneralConfig = _generalConfigBackup;
            if (GeneralConfig.LastDevicesSettup != null)
            {
                foreach (var cDev in ComputeDeviceManager.Available.Devices)
                {
                    foreach (var conf in GeneralConfig.LastDevicesSettup)
                    {
                        cDev.SetFromComputeDeviceConfig(conf);
                    }
                }
            }
            // restore benchmarks
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                if (_benchmarkConfigsBackup != null && _benchmarkConfigsBackup.ContainsKey(cDev.Uuid))
                {
                    cDev.SetAlgorithmDeviceConfig(_benchmarkConfigsBackup[cDev.Uuid]);
                }
            }
        }

        public static bool IsRestartNeeded()
        {
            return GeneralConfig.DebugConsole != _generalConfigBackup.DebugConsole
                   || GeneralConfig.NVIDIAP0State != _generalConfigBackup.NVIDIAP0State
                   || GeneralConfig.LogToFile != _generalConfigBackup.LogToFile
                   || GeneralConfig.SwitchMinSecondsFixed != _generalConfigBackup.SwitchMinSecondsFixed
                   || GeneralConfig.SwitchMinSecondsAMD != _generalConfigBackup.SwitchMinSecondsAMD
                   || GeneralConfig.SwitchMinSecondsDynamic != _generalConfigBackup.SwitchMinSecondsDynamic
                   || GeneralConfig.MinerAPIQueryInterval != _generalConfigBackup.MinerAPIQueryInterval
                   || GeneralConfig.DisableWindowsErrorReporting != _generalConfigBackup.DisableWindowsErrorReporting;
        }

        public static void GeneralConfigFileCommit()
        {
            GeneralConfig.LastDevicesSettup.Clear();
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                GeneralConfig.LastDevicesSettup.Add(cDev.GetComputeDeviceConfig());
            }
            GeneralConfigFile.Commit(GeneralConfig);
        }

        public static void CommitBenchmarks()
        {
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                var devUuid = cDev.Uuid;
                if (BenchmarkConfigFiles.ContainsKey(devUuid))
                {
                    BenchmarkConfigFiles[devUuid].Commit(cDev.GetAlgorithmDeviceConfig());
                }
                else
                {
                    BenchmarkConfigFiles[devUuid] = new DeviceBenchmarkConfigFile(devUuid);
                    BenchmarkConfigFiles[devUuid].Commit(cDev.GetAlgorithmDeviceConfig());
                }
            }
        }

        public static void AfterDeviceQueryInitialization()
        {
            // extra check (probably will never happen but just in case)
            {
                var invalidDevices = new List<ComputeDevice>();
                foreach (var cDev in ComputeDeviceManager.Available.Devices)
                {
                    if (cDev.IsAlgorithmSettingsInitialized() == false)
                    {
                        Helpers.ConsolePrint(Tag,
                            "CRITICAL ISSUE!!! Device has AlgorithmSettings == null. Will remove");
                        invalidDevices.Add(cDev);
                    }
                }
                // remove invalids
                foreach (var invalid in invalidDevices)
                {
                    ComputeDeviceManager.Available.Devices.Remove(invalid);
                }
            }
            // set enabled/disabled devs
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                foreach (var devConf in GeneralConfig.LastDevicesSettup)
                {
                    cDev.SetFromComputeDeviceConfig(devConf);
                }
            }
            // create/init device benchmark configs files and configs
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                var keyUuid = cDev.Uuid;
                BenchmarkConfigFiles[keyUuid] = new DeviceBenchmarkConfigFile(keyUuid);
                // init 
                {
                    DeviceBenchmarkConfig currentConfig = null;
                    if (BenchmarkConfigFiles[keyUuid].IsFileExists())
                    {
                        currentConfig = BenchmarkConfigFiles[keyUuid].ReadFile();
                    }
                    // config exists and file load success set from file
                    if (currentConfig != null)
                    {
                        cDev.SetAlgorithmDeviceConfig(currentConfig);
                        // if new version create backup
                        if (_isNewVersion)
                        {
                            BenchmarkConfigFiles[keyUuid].CreateBackup();
                        }
                    }
                    else
                    {
                        // no config file or not loaded, create new
                        BenchmarkConfigFiles[keyUuid].Commit(cDev.GetAlgorithmDeviceConfig());
                    }
                }
            }
            // save settings
            GeneralConfigFileCommit();
        }
    }
}
