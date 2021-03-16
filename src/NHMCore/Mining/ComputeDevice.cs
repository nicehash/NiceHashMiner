using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.DeviceMonitoring;
using NHM.DeviceMonitoring.TDP;
using NHM.UUID;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Configs.Data;
using NHMCore.Nhmws;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NHMCore.Mining
{
    public class ComputeDevice : NotifyChangedBase
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

        public string FullName => GetFullName();

        public int Index { get; private set; } // For socket control, unique

        // name count is the short name for displaying in moning groups
        public string NameCount { get; private set; }

        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            internal set
            {
                if (value == _enabled) return;
                _enabled = value;
                StartState = false;
                State = value ? DeviceState.Stopped : DeviceState.Disabled;
                OnPropertyChanged();
            }
        }

        // disabled state check
        public bool IsDisabled => (!Enabled || State == DeviceState.Disabled);

        private DeviceState _state = DeviceState.Stopped;
        public DeviceState State
        {
            get => _state;
            internal set
            {
                if (_state == value) return;
                _state = value;
                MiningState.Instance.CalculateDevicesStateChange();
                OnPropertyChanged();
                NHWebSocket.NotifyStateChanged();
            }
        }

        internal bool StartState { get; set; } = false;

        private readonly object _lock = new object();
        private bool _isPendingChange { get; set; } = false;
        public bool IsPendingChange
        {
            get
            {
                lock (_lock)
                {
                    return _isPendingChange;
                }
            }
            internal set
            {
                lock (_lock)
                {
                    if (_isPendingChange == value) return;
                    _isPendingChange = value;
                }
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

        private List<AlgorithmContainer> _algorithmSettings { get; set; } = new List<AlgorithmContainer>();
        public List<AlgorithmContainer> AlgorithmSettings
        {
            get
            {
                lock (_lock)
                {
                    return _algorithmSettings;
                }
            }
            protected set
            {
                lock (_lock)
                {
                    _algorithmSettings = value;
                }
            }
        }

        private List<PluginAlgorithmConfig> PluginAlgorithmSettings { get; set; } = new List<PluginAlgorithmConfig>();

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
                if (!GlobalDeviceSettings.Instance.DisableDevicePowerModeSettings && DeviceMonitor != null && DeviceMonitor is IPowerTarget get) return get.PowerTarget;
                //throw new NotSupportedException($"Device with {Uuid} doesn't support PowerTarget");
                return 0;
            }
        }

        public TDPSimpleType TDPSimple
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ITDP get) return get.TDPSimple;
                return (TDPSimpleType)(-1);
            }
        }

        public float Load
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ILoad get) return get.Load;
                return -1;
            }
        }
        public float Temp
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ITemp get) return get.Temp;
                return -1;
            }
        }
        public int FanSpeed
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IGetFanSpeedPercentage get)
                {
                    var (ok, percentage) = get.GetFanSpeedPercentage();
                    if (ok == 0) return percentage;
                }
                return -1;
            }
        }
        public int FanSpeedRPM
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IFanSpeedRPM get) return get.FanSpeedRPM;
                return -1;
            }
        }
        public double PowerUsage
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IPowerUsage get) return get.PowerUsage;
                return -1;
            }
        }

        public bool CanSetPowerMode
        {
            get
            {
                var canSet = !GlobalDeviceSettings.Instance.DisableDevicePowerModeSettings && DeviceMonitor != null && DeviceMonitor is ITDP;
                return canSet;
            }
        }
        #endregion Getters

        #region Setters

        public bool SetPowerMode(TDPSimpleType level)
        {
            if (!GlobalDeviceSettings.Instance.DisableDevicePowerModeSettings && DeviceMonitor != null && DeviceMonitor is ITDP set)
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
            Enabled = true;

            GlobalDeviceSettings.Instance.PropertyChanged += OnShowGPUPCIeBusIDs;
            this.PropertyChanged += BenchmarkManagerState.Instance.ComputeDeviceOnPropertyChanged;
        }

        public void UpdateEstimatePaying(Dictionary<AlgorithmType, double> paying)
        {
            foreach (var algo in AlgorithmSettings)
            {
                algo.UpdateEstimatedProfit(paying);
            }
        }

        private void OnShowGPUPCIeBusIDs(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GlobalDeviceSettings.ShowGPUPCIeBusIDs))
            {
                OnPropertyChanged(nameof(FullName));
            }
        }

        // combines long and short name
        public string GetFullName()
        {
            if (GlobalDeviceSettings.Instance.ShowGPUPCIeBusIDs && BaseDevice is IGpuDevice gpu)
            {
                return $"{NameCount} {Name} (pcie {gpu.PCIeBusID})";
            }
            return $"{NameCount} {Name}";
        }

        public void RemovePluginAlgorithms(string pluginUUID)
        {
            // get all data from file configs 
            var pluginConfs = GetDeviceConfig().PluginAlgorithmSettings.Where(c => c.PluginUUID == pluginUUID);
            foreach (var pluginConf in pluginConfs)
            {
                // check and update from the chache
                var removeIndexAt = PluginAlgorithmSettings.FindIndex(algo => algo.GetAlgorithmStringID() == pluginConf.GetAlgorithmStringID());
                // remove old if any
                if (removeIndexAt > -1)
                {
                    PluginAlgorithmSettings.RemoveAt(removeIndexAt);
                }
                // cahce pluginConf
                PluginAlgorithmSettings.Add(pluginConf);
            }

            var toRemove = AlgorithmSettings.Where(algo => algo.Algorithm.MinerID == pluginUUID);
            if (toRemove.Count() == 0) return;
            foreach (var removeAlgo in toRemove)
            {
                BenchmarkManagerState.Instance.RemoveAlgorithmContainer(removeAlgo);
            }
            var newList = AlgorithmSettings.Where(algo => toRemove.Contains(algo) == false).ToList();
            AlgorithmSettings = newList;
            OnPropertyChanged(nameof(AlgorithmSettings));
        }

        //public void RemovePluginAlgorithms(IEnumerable<AlgorithmContainer> algos)
        //{
        //    foreach (var algo in algos)
        //    {
        //        AlgorithmSettings.Remove(algo);
        //    }
        //    OnPropertyChanged(nameof(AlgorithmSettings));
        //}

        public void AddPluginAlgorithms(IEnumerable<AlgorithmContainer> algos)
        {
            AlgorithmSettings.AddRange(algos);
            foreach (var addAlgo in algos)
            {
                BenchmarkManagerState.Instance.AddAlgorithmContainer(addAlgo);
            }
            OnPropertyChanged(nameof(AlgorithmSettings));
        }

        public void CopyBenchmarkSettingsFrom(ComputeDevice copyBenchCDev)
        {
            foreach (var copyFromAlgo in copyBenchCDev.AlgorithmSettings)
            {
                var setAlgo = AlgorithmSettings.FirstOrDefault(a => a.AlgorithmStringID == copyFromAlgo.AlgorithmStringID);
                if (setAlgo != null)
                {
                    setAlgo.Enabled = copyFromAlgo.Enabled;
                    setAlgo.BenchmarkSpeed = copyFromAlgo.BenchmarkSpeed;
                    setAlgo.ExtraLaunchParameters = copyFromAlgo.ExtraLaunchParameters;
                    setAlgo.PowerUsage = copyFromAlgo.PowerUsage;
                }
            }
        }

        public AlgorithmContainer GetAlgorithm(string minerUUID, params AlgorithmType[] ids)
        {
            return AlgorithmSettings.FirstOrDefault(a => a.MinerUUID == minerUUID && !a.IDs.Except(ids).Any());
        }

        public PluginAlgorithmConfig GetPluginAlgorithmConfig(string algorithmStringID)
        {
            var configs = GetDeviceConfig();
            // try get data from configs
            var pluginConf = configs.PluginAlgorithmSettings.Where(c => c.GetAlgorithmStringID() == algorithmStringID).FirstOrDefault();
            if (pluginConf == null)
            {
                // get cahced data
                pluginConf = PluginAlgorithmSettings.Where(c => c.GetAlgorithmStringID() == algorithmStringID).FirstOrDefault();
            }
            return pluginConf;
        }

        #region Config Setters/Getters

        public void SetDeviceConfig(DeviceConfig config)
        {
            if (config == null || config.DeviceUUID != Uuid) return;
            // set device settings
            //Enabled = config.Enabled;
            Enabled = config.Enabled;
            MinimumProfit = config.MinimumProfit;

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
            if (DeviceMonitor is ITDP tdp)
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

        public bool AnyAlgorithmEnabled() => AlgorithmSettings.Any(a => a.Enabled);

        public bool AllEnabledAlgorithmsZeroPaying()
        {
            return AlgorithmSettings
                .Where(a => a.Enabled)
                .All(a => a.CurrentEstimatedProfit <= 0d);
        }

        public bool AnyEnabledAlgorithmsNeedBenchmarking() => AlgorithmsForBenchmark().Any();

        public IEnumerable<AlgorithmContainer> AlgorithmsForBenchmark()
        {
            return AlgorithmSettings
                .Where(algo => algo.Enabled)
                .Where(algo => algo.IsReBenchmark || algo.BenchmarkNeeded);
        }

        #endregion Checker
    }
}
