using NiceHashMiner.Configs;
using NiceHashMiner.Configs.Data;
using System.Collections.Generic;
using System.Linq;
using NHM.Common.Enums;
using NHM.Common.Device;
using NHM.UUID;
using NHM.DeviceMonitoring;
using NHM.DeviceMonitoring.TDP;

namespace NiceHashMiner.Mining
{
    public class ComputeDevice
    {
        // migrate ComputeDevice to BaseDevice
        public BaseDevice BaseDevice { get; private set; }

        public int ID => BaseDevice?.ID ?? -1;
        // CPU, NVIDIA, AMD
        public DeviceType DeviceType => BaseDevice?.DeviceType ?? (DeviceType)(-1);
        // UUID now used for saving
        public string Uuid => BaseDevice?.UUID ?? "-1";
        // to identify equality;
        public string Name => BaseDevice?.Name ?? "-1";

        public int Index { get; private set; } // For socket control, unique

        // name count is the short name for displaying in moning groups
        public string NameCount { get; private set; }
        public bool Enabled { get; protected set; }

        // disabled state check
        public bool IsDisabled => (!Enabled || State == DeviceState.Disabled);

        private DeviceState _state = DeviceState.Stopped;
        public DeviceState State
        {
            get => _state;
            set
            {
                _state = value;
                MiningState.Instance.CalculateDevicesStateChange();
            }
        }

        public string B64Uuid
        {
            get
            {
                //UUIDs
                //RIG - 0
                //CPU - 1
                //GPU - 2 // NVIDIA
                //AMD - 3
                // types 

                int type = 1; // assume type is CPU
                if (DeviceType == DeviceType.NVIDIA)
                {
                    type = 2;
                }
                else if (DeviceType == DeviceType.AMD)
                {
                    type = 3;
                }
                var b64Web = UUID.GetB64UUID(Uuid);
                return $"{type}-{b64Web}";
            }
        }

        public List<AlgorithmContainer> AlgorithmSettings { get; protected set; } = new List<AlgorithmContainer>();

        public List<PluginAlgorithmConfig> PluginAlgorithmSettings { get; protected set; } = new List<PluginAlgorithmConfig>();

        public double MinimumProfit { get; set; }

        public string BenchmarkCopyUuid { get; set; }

        #region DeviceMonitor
        public void SetDeviceMonitor(DeviceMonitor deviceMonitor)
        {
            DeviceMonitor = deviceMonitor;
        }
        public DeviceMonitor DeviceMonitor { get; private set; }

        #region Getters

        public uint PowerTarget
        {
            get
            {
                if (!ConfigManager.GeneralConfig.DisableDevicePowerModeSettings && DeviceMonitor != null && DeviceMonitor is IPowerTarget get) return get.PowerTarget;
                //throw new NotSupportedException($"Device with {Uuid} doesn't support PowerTarget");
                return 0;
            }
        }

#warning "This property requires change of protocol. Currently it is disabled on the backend."
        public TDPSimpleType TDPSimple
        {
            get
            {
                //if (ConfigManager.GeneralConfig.DisableDevicePowerModeSettings) return TDPSimple.Disabled;
                //if (DeviceMonitor != null && DeviceMonitor is ITDP get) return get.TDPSimple;
                //return TDPSimple.Unsupported;
                return (TDPSimpleType)(-1);
            }
        }

        public float Load
        {
            get
            {
                if (!ConfigManager.GeneralConfig.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ILoad get) return get.Load;
                return -1;
            }
        }
        public float Temp
        {
            get
            {
                if (!ConfigManager.GeneralConfig.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ITemp get) return get.Temp;
                return -1;
            }
        }
        public int FanSpeed
        {
            get
            {
                if (!ConfigManager.GeneralConfig.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IFanSpeedRPM get) return get.FanSpeedRPM;
                return -1;
            }
        }
        public double PowerUsage
        {
            get
            {
                if (!ConfigManager.GeneralConfig.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IPowerUsage get) return get.PowerUsage;
                return -1;
            }
        }

        public bool CanSetPowerMode
        {
            get
            {
                var canSet = !ConfigManager.GeneralConfig.DisableDevicePowerModeSettings && DeviceMonitor != null && DeviceMonitor is ITDP;
                return canSet;
            }
        }
        #endregion Getters

        #region Setters

        public bool SetPowerMode(TDPSimpleType level)
        {
            if (!ConfigManager.GeneralConfig.DisableDevicePowerModeSettings && DeviceMonitor != null && DeviceMonitor is ITDP set)
            {
                return set.SetTDPSimple(level);
            }
            return false;
        }

        #endregion

        #endregion DeviceMonitor


        // constructor
        public ComputeDevice(BaseDevice baseDevice, int index, string nameCount)
        {
            BaseDevice = baseDevice;
            Index = index;
            NameCount = nameCount;
            SetEnabled(true);
        }

        public void SetEnabled(bool isEnabled)
        {
            Enabled = isEnabled;
            State = isEnabled ? DeviceState.Stopped : DeviceState.Disabled;
        }

        // combines long and short name
        public string GetFullName()
        {
            if (ConfigManager.GeneralConfig.ShowGPUPCIeBusIDs && BaseDevice is IGpuDevice gpu)
            {
                return $"{NameCount} {Name} (pcie {gpu.PCIeBusID})";
            }
            return $"{NameCount} {Name}";
        }

        public void RemovePluginAlgorithms(string pluginUUID)
        {
            // TODO save removed algorithm configs
            var toRemove = AlgorithmSettings.Where(algo => algo.Algorithm.MinerID == pluginUUID);
            if (toRemove.Count() == 0) return;
            var newList = AlgorithmSettings.Where(algo => toRemove.Contains(algo) == false).ToList();
            AlgorithmSettings = newList;
        }

        public void RemovePluginAlgorithms(IEnumerable<AlgorithmContainer> algos)
        {
            foreach (var algo in algos)
            {
                AlgorithmSettings.Remove(algo);
            }
        }

        public void AddPluginAlgorithms(IEnumerable<AlgorithmContainer> algos)
        {
            if (algos.Count() > 0) AlgorithmSettings.AddRange(algos);
        }

        public void CopyBenchmarkSettingsFrom(ComputeDevice copyBenchCDev)
        {
            foreach (var copyFromAlgo in copyBenchCDev.AlgorithmSettings)
            {
                var setAlgo = AlgorithmSettings.Where(a => a.AlgorithmStringID == copyFromAlgo.AlgorithmStringID).FirstOrDefault();
                if (setAlgo != null)
                {
                    setAlgo.BenchmarkSpeed = copyFromAlgo.BenchmarkSpeed;
                    setAlgo.ExtraLaunchParameters = copyFromAlgo.ExtraLaunchParameters;
                    setAlgo.PowerUsage = copyFromAlgo.PowerUsage;
                }
            }
        }

        public AlgorithmContainer GetAlgorithm(string minerUUID, params AlgorithmType[] ids)
        {
            return AlgorithmSettings.Where(a => a.MinerUUID == minerUUID && a.IDs.Except(ids).Count() == 0).FirstOrDefault();
        }

        #region Config Setters/Getters

        public void SetDeviceConfig(DeviceConfig config)
        {
            if (config == null || config.DeviceUUID != Uuid) return;
            // set device settings
            //Enabled = config.Enabled;
            SetEnabled(config.Enabled);
            MinimumProfit = config.MinimumProfit;

#if TESTNET || TESTNETDEV || PRODUCTION_NEW
            if (!DeviceMonitorManager.DisableDevicePowerModeSettings)
            {
                var tdpSimpleDefault = TDPSimpleType.HIGH;
                var tdpSettings = config.TDPSettings;
                if (tdpSettings != null && DeviceMonitor is ITDP tdp)
                {
                    tdp.SettingType = config.TDPSettings.SettingType;
                    switch (config.TDPSettings.SettingType)
                    {
                        case TDPSettingType.PERCENTAGE:
                            if (config.TDPSettings.Percentage.HasValue)
                            {
                                // config values are from 0.0% to 100.0%
                                tdp.SetTDPPercentage(config.TDPSettings.Percentage.Value / 100);
                            }
                            else
                            {
                                tdp.SetTDPSimple(tdpSimpleDefault); // fallback
                            }
                            break;
                        case TDPSettingType.RAW:
                            if (config.TDPSettings.Raw.HasValue)
                            {
                                tdp.SetTDPRaw(config.TDPSettings.Raw.Value);
                            }
                            else
                            {
                                tdp.SetTDPSimple(tdpSimpleDefault); // fallback
                            }
                            break;
                        // here we decide to not allow per GPU disable state, default fallback is SIMPLE setting
                        case TDPSettingType.UNSUPPORTED:
                        case TDPSettingType.DISABLED:
                        case TDPSettingType.SIMPLE:
                        default:
                            tdp.SettingType = TDPSettingType.SIMPLE;
                            if (config.TDPSettings.Simple.HasValue)
                            {
                                tdp.SetTDPSimple(config.TDPSettings.Simple.Value);
                            }
                            else
                            {
                                tdp.SetTDPSimple(tdpSimpleDefault); // fallback
                            }
                            break;
                    }
                }
                else if (DeviceMonitor is ITDP tdpDefault)
                {
                    tdpDefault.SetTDPSimple(tdpSimpleDefault); // set default high
                }
            }
#endif

            if (config.PluginAlgorithmSettings == null) return;
            PluginAlgorithmSettings = config.PluginAlgorithmSettings;
            // plugin algorithms
            foreach (var pluginConf in config.PluginAlgorithmSettings)
            {
                var pluginConfAlgorithmIDs = pluginConf.GetAlgorithmIDs();
                var pluginAlgo = AlgorithmSettings
                    .Where(pAlgo => pluginConf.PluginUUID == pAlgo.Algorithm.MinerID && pluginConfAlgorithmIDs.Except(pAlgo.Algorithm.IDs).Count() == 0)
                    .FirstOrDefault();
                if (pluginAlgo == null) continue;
                // set plugin algo
                pluginAlgo.Speeds = pluginConf.Speeds;
                pluginAlgo.Enabled = pluginConf.Enabled;
                pluginAlgo.ExtraLaunchParameters = pluginConf.ExtraLaunchParameters;
                pluginAlgo.PowerUsage = pluginConf.PowerUsage;
                pluginAlgo.ConfigVersion = pluginConf.GetVersion();
            }
        }

        public DeviceConfig GetDeviceConfig()
        {
            var TDPSettings = new DeviceTDPSettings { SettingType = TDPSettingType.UNSUPPORTED };
            if(DeviceMonitor is ITDP tdp)
            {
                if (DeviceMonitorManager.DisableDevicePowerModeSettings)
                {
                    TDPSettings.SettingType = TDPSettingType.DISABLED;
                }
                else
                {
                    TDPSettings.SettingType = tdp.SettingType;
                    if (TDPSettings.SettingType == TDPSettingType.SIMPLE)
                    {
                        TDPSettings.Simple = tdp.TDPSimple;
                    }
                    if (TDPSettings.SettingType == TDPSettingType.PERCENTAGE)
                    {
                        TDPSettings.Percentage = tdp.TDPPercentage * 100;
                    }
                    if (TDPSettings.SettingType == TDPSettingType.RAW)
                    {
                        TDPSettings.Raw = tdp.TDPRaw;
                    }
                }
            }
            var ret = new DeviceConfig
            {
                DeviceName = Name,
                DeviceUUID = Uuid,
                Enabled = Enabled,
                MinimumProfit = MinimumProfit,
                TDPSettings = TDPSettings,
            };
            // init algo settings
            foreach (var algo in AlgorithmSettings)
            {
                var pluginConf = new PluginAlgorithmConfig
                {
                    Name = algo.PluginName,
                    PluginUUID = algo.Algorithm.MinerID,
                    AlgorithmIDs = string.Join("-", algo.Algorithm.IDs.Select(id => id.ToString())),
                    Enabled = algo.Enabled,
                    ExtraLaunchParameters = algo.ExtraLaunchParameters,
                    PluginVersion = $"{algo.PluginVersion.Major}.{algo.PluginVersion.Minor}",
                    PowerUsage = algo.PowerUsage,
                    Speeds = algo.Speeds
                };
                ret.PluginAlgorithmSettings.Add(pluginConf);
            }
            // add old algo configs

            return ret;
        }

        #endregion Config Setters/Getters

        #region Checker
        public bool AllEnabledAlgorithmsWithoutBenchmarks()
        {
            var allEnabledAlgorithms = AlgorithmSettings.Where(algo => algo.Enabled);
            var allEnabledAlgorithmsWithoutBenchmarks = allEnabledAlgorithms.Where(algo => algo.BenchmarkNeeded);
            return allEnabledAlgorithms.Count() == allEnabledAlgorithmsWithoutBenchmarks.Count();
        }

        public bool HasEnabledAlgorithmsWithReBenchmark()
        {
            var reBenchmarks = AlgorithmSettings.Where(algo => algo.Enabled && algo.IsReBenchmark && !algo.BenchmarkNeeded);
            return reBenchmarks.Count() > 0;
        }

        public bool AnyAlgorithmEnabled()
        {
            var anyEnabled = AlgorithmSettings.Any(a => a.Enabled);
            return anyEnabled;
        }

        public bool AllEnabledAlgorithmsZeroPaying()
        {
            var isAllZeroPayingState = AlgorithmSettings.Where(a => a.Enabled).Select(a => a.CurPayingRate == 0d);
            var ret = isAllZeroPayingState.All(t => t);
            return ret;
        }
        #endregion Checker
    }
}
