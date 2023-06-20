using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.DeviceMonitoring;
using NHM.DeviceMonitoring.Core_clock;
using NHM.DeviceMonitoring.Core_voltage;
using NHM.DeviceMonitoring.Memory_clock;
using NHM.DeviceMonitoring.TDP;
using NHM.UUID;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Configs.Data;
using NHMCore.Nhmws;
using NHMCore.Nhmws.V4;
using NHMCore.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        // name count is the short name for displaying in moning groups
        public string NameCount { get; private set; }
#if NHMWS4
        public bool IsTesting => AlgorithmSettings.Any(a => a.IsTesting);
        public bool IsMiningBenchingTesting => State == DeviceState.Mining || State == DeviceState.Testing || State == DeviceState.Benchmarking;
        private PidController _pidController = new();
#endif
        private bool IsLessThan2KSeries(string name)
        {
            string pattern = @"\b(1[0-9]{3}|[1-9][0-9]{2})\b";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(name);
            return match.Success;
        }
        public bool IsNvidiaAndSub2KSeries()
        {
            return DeviceType == DeviceType.NVIDIA && IsLessThan2KSeries(Name);
        }
        private int _memoryControlCounter = 0;

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
                if (B64Uuid == null || B64Uuid == string.Empty || B64Uuid == "-1") return; //initial stuff
                var eventType = value ? EventType.DeviceEnabled : EventType.DeviceDisabled;
                if (value) EventManager.Instance.AddEventDevEnabled(Name, B64Uuid, true);
                else EventManager.Instance.AddEventDevDisabled(Name, B64Uuid, true);
            }
        }

        private bool _pauseMiningWhenGamingMode = false;
        public bool PauseMiningWhenGamingMode
        {
            get => _pauseMiningWhenGamingMode;
            internal set
            {
                if (value == _pauseMiningWhenGamingMode) return;
                _pauseMiningWhenGamingMode = value;
                OnPropertyChanged();
            }
        }
        public List<DeviceDynamicProperties> SupportedDynamicProperties { get; set; } = new();

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

        private bool _isGaming = false;

        public bool IsGaming
        {
            get => _isGaming;
            internal set
            {
                if (_isGaming == value) return;
                _isGaming = value;
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
                //UUIDs types
                //RIG - 0
                //CPU - 1
                //GPU - 2 // NVIDIA
                //AMD - 3
                int type = DeviceType switch
                {
                    DeviceType.CPU => 1,
                    DeviceType.NVIDIA => 2,
                    DeviceType.AMD => 3,
                    DeviceType.INTEL => 4,
                    _ => throw new Exception($"Unknown DeviceType {(int)DeviceType}"),
                };
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
        public int ApplyNewAlgoStates(MinerAlgoState state)
        {
            if (State == DeviceState.Mining || State == DeviceState.Testing || State == DeviceState.Benchmarking) return -1;
            foreach (var miner in state.Miners)
            {
                foreach (var algo in miner.Algos)
                {
                    var targets = AlgorithmSettings.Where(a => a.AlgorithmName == algo.Id && a.PluginName == miner.Id)?.ToList();
                    if (targets == null) continue;
                    if (!miner.Enabled)
                    {
                        targets.ForEach(t => t.SetEnabled((bool)false));
                        continue;
                    }
                    targets.ForEach(t => t.SetEnabled((bool)algo.Enabled));
                }
                var enabledAlgos = miner.Algos.Where(a => (bool)a.Enabled);
                var disabledAlgos = miner.Algos.Where(a => (bool)!a.Enabled);
                if(enabledAlgos != null && enabledAlgos.Count() > 0 && miner.Enabled)
                {
                    EventManager.Instance.AddEventAlgoEnabled(B64Uuid, miner.Id, enabledAlgos.Select(a => a.Id).ToList(), true);
                }
                if(disabledAlgos != null && disabledAlgos.Count() > 0)
                {
                    EventManager.Instance.AddEventAlgoDisabled(B64Uuid, miner.Id, disabledAlgos.Select(a => a.Id).ToList(), true);
                }
                else if (!miner.Enabled)
                {
                    EventManager.Instance.AddEventAlgoDisabled(B64Uuid, miner.Id, miner.Algos.Select(a => a.Id).ToList(), true);
                }
            }
            Task.Run(async () => NHWebSocketV4.UpdateMinerStatus());
            return 0;
        }

        public int ApplyNewAlgoSpeeds(MinerAlgoSpeed speed)
        {
            foreach (var miner in speed.Miners)
            {
                foreach (var algo in miner.Combinations)
                {
                    var targets = AlgorithmSettings.Where(a => a.AlgorithmName == algo.Id && a.PluginName == miner.Id)?.ToList();
                    if (targets == null) continue;
                    targets.ForEach(t => t.BenchmarkSpeed = Convert.ToDouble(algo.Algos.FirstOrDefault().Speed));
                }
            }
            Task.Run(async () => NHWebSocketV4.UpdateMinerStatus());
            return 0;
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

        private bool CanMonitorStatus => !GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null;

        private bool CanSetTDP => DeviceMonitor != null;

        public uint PowerTarget
        {
            get
            {
                if (CanSetTDP && DeviceMonitor is IPowerTarget get) return get.PowerTarget;
                //throw new NotSupportedException($"Device with {Uuid} doesn't support PowerTarget");
                return 0;
            }
        }

        public TDPSimpleType TDPSimple
        {
            get
            {
                if (CanMonitorStatus && DeviceMonitor is ITDP get) return get.TDPSimple;
                return (TDPSimpleType)(-1);
            }
        }

        public float Load
        {
            get
            {
                if (CanMonitorStatus && DeviceMonitor is ILoad get) return get.Load;
                return -1;
            }
        }
        public float Temp
        {
            get
            {
                if (CanMonitorStatus && DeviceMonitor is ITemp get) return get.Temp;
                return -1;
            }
        }
        public int FanSpeed
        {
            get
            {
                if (CanMonitorStatus && DeviceMonitor is IGetFanSpeedPercentage get)
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
                if (CanMonitorStatus && DeviceMonitor is IFanSpeedRPM get) return get.FanSpeedRPM;
                return -1;
            }
        }
        public double PowerUsage
        {
            get
            {
                if (CanMonitorStatus && DeviceMonitor is IPowerUsage get) return get.PowerUsage;
                return -1;
            }
        }

        public bool CanSetPowerMode
        {
            get
            {
                var canSet = CanSetTDP && DeviceMonitor is ITDP;
                return canSet;
            }
        }
        public int VramTemperature
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ISpecialTemps get) return get.VramTemp;
                return -1;
            }
        }
        public int HotspotTemperature
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ISpecialTemps get) return get.HotspotTemp;
                return -1;
            }
        }
        public int MemoryControllerLoad
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IMemControllerLoad get) return get.MemoryControllerLoad;
                return -1;
            }
        }
        public int CoreClock
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ICoreClock get) return get.CoreClock;
                return -1;
            }
        }
        public int CoreClockDelta
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ICoreClockDelta get) return get.CoreClockDelta;
                return -1;
            }
        }
        public int PreferredCoreClock
        {
            get
            {
                if (DeviceType == DeviceType.NVIDIA) return CoreClockDelta;
                if (DeviceType == DeviceType.AMD) return CoreClock;
                if (DeviceType == DeviceType.INTEL) return CoreClockDelta;
                return -1;
            }
        }
        public int MemoryClock
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IMemoryClock get) return get.MemoryClock;
                return -1;
            }
        }

        public int MemoryClockDelta
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IMemoryClockDelta get) return get.MemoryClockDelta;
                return -1;
            }
        }
        public int PreferredMemoryClock
        {
            get
            {
                if (DeviceType == DeviceType.NVIDIA) return MemoryClockDelta;
                if (DeviceType == DeviceType.AMD) return MemoryClock;
                if (DeviceType == DeviceType.INTEL) return MemoryClockDelta;
                return -1;
            }
        }

        public (int min, int max, int def) TDPLimits
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ITDPLimits get)
                {
                    var ret = get.GetTDPLimits();
                    return (ret.min, ret.max, ret.def);
                }
                return (0, 0, 0);
            }
        }

        public (bool ok, int min, int max, int def) CoreClockRange
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ICoreClockRange get)
                {
                    var ret = get.CoreClockRange;
                    return (ret.ok, ret.min, ret.max, ret.def);
                }
                return (false, -1, -1, -1);
            }
        }

        public (bool ok, int min, int max, int def) MemoryClockRange
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IMemoryClockRange get)
                {
                    var ret = get.MemoryClockRange;
                    return (ret.ok, ret.min, ret.max, ret.def);
                }
                return (false, -1, -1, -1);
            }
        }

        public (bool ok, int min, int max, int def) MemoryClockRangeDelta
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is IMemoryClockRangeDelta get)
                {
                    var ret = get.MemoryClockRangeDelta;
                    return (ret.ok, ret.min, ret.max, ret.def);
                }
                return (false, -1, -1, -1);
            }
        }

        public int CoreVoltage
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ICoreVoltage get) return get.CoreVoltage;
                return -1;
            }
        }

        public (bool ok, int min, int max, int def) CoreVoltageRange
        {
            get
            {
                if (!GlobalDeviceSettings.Instance.DisableDeviceStatusMonitoring && DeviceMonitor != null && DeviceMonitor is ICoreVoltageRange get)
                {
                    var ret = get.CoreVoltageRange;
                    return (ret.ok, ret.min, ret.max, ret.def);
                }
                return (false, -1, -1, -1);
            }
        }
        #endregion Getters

        #region Setters

        public bool SetPowerMode(TDPSimpleType level)
        {
            if (CanSetTDP && DeviceMonitor is ITDP set) return set.SetTDPSimple(level);
            return false;
        }
        public bool SetPowerModeManual(int TDP)
        {
            if (CanSetTDP && DeviceMonitor is ITDP set) return set.SetTDP(TDP);
            return false;
        }
        public bool SetCoreClock(int coreClock)
        {
            if (CanSetTDP && DeviceMonitor is ICoreClockSet set) return set.SetCoreClock(coreClock);
            return false;
        }
        public bool SetCoreClockDelta(int coreClockDelta)
        {
            if(CanSetTDP && DeviceMonitor is ICoreClockSetDelta set) return set.SetCoreClockDelta(coreClockDelta);
            return false;
        }
        public bool SetMemoryClock(int memoryClock)
        {
            if (CanSetTDP && DeviceMonitor is IMemoryClockSet set) return set.SetMemoryClock(memoryClock);
            return false;
        }
        public bool SetMemoryClockDelta(int memoryClockDelta)
        {
            if(CanSetTDP && DeviceMonitor is IMemoryClockSetDelta set) return set.SetMemoryClockDelta(memoryClockDelta);
            return false;
        }
        public bool SetFanSpeedPercentage(int percent)
        {
            if (DeviceMonitor is ISetFanSpeedPercentage set) return set.SetFanSpeedPercentage(percent);
            return false;
        }
        public bool ResetFanSpeed()
        {
            if (DeviceMonitor is IResetFanSpeed set) return set.ResetFanSpeedPercentage();
            return false;
        }
        public bool SetCoreVoltage(int voltage)
        {
            if (DeviceMonitor is ICoreVoltageSet set) return set.SetCoreVoltage(voltage);
            return false;
        }
        public bool ResetCoreVoltage()
        {
            if(DeviceMonitor is ICoreVoltageSet set) return set.ResetCoreVoltage();
            return false;
        }
        public bool ResetCoreClock()
        {
            if (DeviceMonitor is ICoreClockSet set) return set.ResetCoreClock();
            return false;
        }
        public bool ResetMemoryClock()
        {
            if (DeviceMonitor is IMemoryClockSet set) return set.ResetMemoryClock();
            return false;
        }
        public bool ResetCoreClockDelta()
        {
            if(DeviceMonitor is ICoreClockSetDelta set) return set.ResetCoreClockDelta();
            return false;
        }
        public bool ResetMemoryClockDelta()
        {
            if(DeviceMonitor is IMemoryClockSetDelta set) return set.ResetMemoryClockDelta();
            return false;
        }

        #endregion

        #endregion DeviceMonitor


        // constructor
        public ComputeDevice(BaseDevice baseDevice, string nameCount)
        {
            BaseDevice = baseDevice;
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
            PauseMiningWhenGamingMode = config.PauseMiningWhenGamingMode;
           
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
                            tdp.SetTDP(config.TDPSettings.Percentage.Value / 100);
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
                pluginAlgo.PowerUsage = pluginConf.PowerUsage;
                pluginAlgo.ConfigVersion = pluginConf.GetVersion();
            }
        }

        public DeviceConfig GetDeviceConfig()
        {
            var TDPSettings = new DeviceTDPSettings { SettingType = TDPSettingType.UNSUPPORTED };
            if (DeviceMonitor is ITDP tdp)
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
            var ret = new DeviceConfig
            {
                DeviceName = Name,
                DeviceUUID = Uuid,
                Enabled = Enabled,
                MinimumProfit = MinimumProfit,
                TDPSettings = TDPSettings,
                PauseMiningWhenGamingMode = PauseMiningWhenGamingMode
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
                    PluginVersion = $"{algo.ConfigVersion.Major}.{algo.ConfigVersion.Minor}",
                    PowerUsage = algo.PowerUsage,
                    Speeds = algo.Speeds
                };
                if (!algo.HasBenchmark)
                {
                    pluginConf.PluginVersion = $"{algo.PluginVersion.Major}.{algo.PluginVersion.Minor}";
                }
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
        public void PrepareForRebenchmark()
        {
            var algosToRebenchmark = AlgorithmSettings.Where(a => a.Enabled);
            foreach(var algo in algosToRebenchmark) algo.IsReBenchmark = true;
        }

        public IEnumerable<AlgorithmContainer> AlgorithmsForBenchmark()
        {
            return AlgorithmSettings
                .Where(algo => algo.Enabled)
                .Where(algo => algo.IsReBenchmark || algo.BenchmarkNeeded);
        }

        #endregion Checker

        public int TrySetMemoryTimings(string mtString)
        {
            if (DeviceMonitor is IMemoryTimings mp)
            {
                return mp.SetMemoryTimings(mtString);
            }
            return -1;
        }

        public int TryResetMemoryTimings()
        {
            if (DeviceMonitor is IMemoryTimings mp)
            {
                return mp.ResetMemoryTimings();
            }
            return -1;
        }
        public void PrintMemoryTimings()
        {
            if (DeviceMonitor is IMemoryTimings mp)
            {
                mp.PrintMemoryTimings();
            }
        }
#if NHMWS4
        public string OCProfile
        {
            get
            {
                var testTarget = AlgorithmSettings.FirstOrDefault(a => a.IsCurrentlyMining);
                if (testTarget != null)
                {
                    return testTarget.OCProfile;
                }
                return string.Empty;
            }
        }
        public string OCProfileID
        {
            get
            {
                var testTarget = AlgorithmSettings.FirstOrDefault(a => a.IsCurrentlyMining);
                if (testTarget != null)
                {
                    return testTarget.OCProfileID;
                }
                return string.Empty;
            }
        }
        public string FanProfile
        {
            get
            {
                var testTarget = AlgorithmSettings.FirstOrDefault(a => a.IsCurrentlyMining);
                if (testTarget != null)
                {
                    return testTarget.FanProfile;
                }
                return string.Empty;
            }
        }
        public string FanProfileID
        {
            get
            {
                var testTarget = AlgorithmSettings.FirstOrDefault(a => a.IsCurrentlyMining);
                if (testTarget != null)
                {
                    return testTarget.FanProfileID;
                }
                return string.Empty;
            }
        }
        public string ELPProfile
        {
            get
            {
                var testTarget = AlgorithmSettings.FirstOrDefault(a => a.IsCurrentlyMining);
                if (testTarget != null)
                {
                    return testTarget.ELPProfile;
                }
                return string.Empty;
            }
        }
        public string ELPProfileID
        {
            get
            {
                var testTarget = AlgorithmSettings.FirstOrDefault(a => a.IsCurrentlyMining);
                if (testTarget != null)
                {
                    return testTarget.ELPProfileID;
                }
                return string.Empty;
            }
        }
        private bool HasNewTestItem(AlgorithmContainer target, string runningOC, string runningELP, string runningFAN)
        {
            return JsonConvert.SerializeObject(target.ActiveOCTestProfile) != runningOC ||
                    JsonConvert.SerializeObject(target.ActiveELPTestProfile) != runningELP ||
                    JsonConvert.SerializeObject(target.ActiveFanTestProfile) != runningFAN;
        }
        private bool HasNewItem(AlgorithmContainer target, string runningOC, string runningELP, string runningFAN)
        {
            return JsonConvert.SerializeObject(target.ActiveOCProfile) != runningOC ||
                    JsonConvert.SerializeObject(target.ActiveELPProfile) != runningELP ||
                    JsonConvert.SerializeObject(target.ActiveFanProfile) != runningFAN;
        }

        public async Task AfterStartMining()
        {
            var target = AlgorithmSettings.Where(a => a.IsCurrentlyMining)?.FirstOrDefault();
            if (target == null) return;
            var serializedRunningOC = JsonConvert.SerializeObject(target.RunningOcProfile);
            var serializedRunningELP = JsonConvert.SerializeObject(target.RunningELPProfile);
            var serializedRunningFan = JsonConvert.SerializeObject(target.RunningFanProfile);
            if (target.HasTestProfileAndCanSet())
            {
                if(HasNewTestItem(target, serializedRunningOC, serializedRunningELP, serializedRunningFan))
                {
                    ResetFanSpeed();
                    await target.ResetOcForDevice();
                    target.RunningOcProfile = null;
                    target.RunningFanProfile = null;
                    target.RunningELPProfile = null;
                }

                if (target.ActiveOCTestProfile != null)
                {
                    await target.SetOcForDevice(target.ActiveOCTestProfile, false);
                    target.RunningOcProfile = target.ActiveOCTestProfile;
                    State = DeviceState.Testing;
                    return;
                }
                if (target.ActiveELPTestProfile != null)
                {
                    State = DeviceState.Testing;
                    target.RunningELPProfile = target.ActiveELPTestProfile;
                    return;
                }
                if (target.ActiveFanTestProfile != null)
                {
                    State = DeviceState.Testing;
                    target.RunningFanProfile = target.ActiveFanTestProfile;
                    return;
                }
            }
            if (target.HasNormalProfileAndCanSet() && !target.HasTestProfileAndCanSet())
            {
                if (HasNewItem(target, serializedRunningOC, serializedRunningELP, serializedRunningFan))
                {
                    ResetFanSpeed();
                    await target.ResetOcForDevice();
                    target.RunningOcProfile = null;
                    target.RunningELPProfile = null;
                    target.RunningFanProfile = null;
                }
                if (target.ActiveOCProfile != null)
                {
                    await target.SetOcForDevice(target.ActiveOCProfile, false);
                    target.RunningOcProfile = target.ActiveOCProfile;
                }
                else
                {
                    target.RunningOcProfile = null;
                }
                if (target.ActiveFanProfile != null)
                {
                    target.RunningFanProfile = target.ActiveFanProfile;
                }
                else
                {
                    target.RunningFanProfile = null;
                }
                if (target.ActiveELPProfile != null)
                {
                    target.RunningELPProfile = target.ActiveELPProfile;
                }
                else
                {
                    target.RunningELPProfile = null;
                }
                State = DeviceState.Mining;
                return;
            }
            if(!target.HasTestProfileAndCanSet() && !target.HasNormalProfileAndCanSet())
            {
                if (HasNewItem(target, serializedRunningOC, serializedRunningELP, serializedRunningFan) ||
                    HasNewTestItem(target, serializedRunningOC, serializedRunningELP, serializedRunningFan)) //IF HAS NEW ITEM
                {
                    ResetFanSpeed();
                    await target.ResetOcForDevice();
                    target.RunningOcProfile = null;
                    target.RunningFanProfile = null;
                    target.RunningELPProfile = null;
                }
                State = DeviceState.Mining;
                return;
            }
            State = DeviceState.Mining;
        }
        public void SetFanSpeedWithPidController()
        {
            var testTarget = AlgorithmSettings.Where(a => a.IsCurrentlyMining)?.FirstOrDefault();
            if (testTarget == null) return;
            var profile = testTarget.ActiveFanTestProfile ?? testTarget.ActiveFanProfile ?? null;
            if (profile == null) return;

            switch (profile.Type)
            {
                case 0:
                    SetFanSpeedPercentage(profile.FanSpeed);
                    return;
                case 1:
                    SetFanSpeed(profile);
                    return;
                case 2:
                    SetFanSpeed(profile);
                    return;
                case 3:
                    SetFanSpeedWithLoweringMemoryClocks(profile);
                    return;
                default:
                    return;
            }
        }

        private void SetFanSpeed(FanProfile profile)
        {
            if (profile.Type == 1)
            {
                _pidController.SetPid(10, 0.8, 1);
                _pidController.SetOutputLimit(100);
                _pidController.SetReversed(true);
                var speed = _pidController.GetOutput(Temp, profile.GpuTemp);
                SetFanSpeedPercentage((int)speed);
            }
            else
            {
                _pidController.SetPid(10, 0.8, 1);
                _pidController.SetOutputLimit(profile.MaxFanSpeed);
                _pidController.SetReversed(true);
                var speed = _pidController.GetOutput(Temp, Math.Min(profile.GpuTemp, profile.VramTemp));
                SetFanSpeedPercentage((int)speed);
            }
        }

        private void SetFanSpeedWithLoweringMemoryClocks(FanProfile profile)
        {
            _pidController.SetPid(10, 0.8, 1);
            _pidController.SetOutputLimit(profile.MaxFanSpeed);
            _pidController.SetReversed(true);
            var speed = _pidController.GetOutput(Temp, Math.Min(profile.GpuTemp, profile.VramTemp));
            SetFanSpeedPercentage((int)speed);

            var deltaTemp = Math.Max(Temp, VramTemperature) - Math.Min(profile.GpuTemp, profile.VramTemp);
            if (deltaTemp > 5) _memoryControlCounter++;

            if (_memoryControlCounter >= 5)
            {
                _pidController.SetPid(100, 0.8, 1);
                _pidController.SetOutputLimits(MemoryClockRange.min, MemoryClockDelta);
                _pidController.SetReversed(false);
                var memory_clock = _pidController.GetOutput(Temp, Math.Min(profile.GpuTemp, profile.VramTemp));
                SetMemoryClock((int)memory_clock);
                _memoryControlCounter = 0;
            }
        }

#endif
    }
}
