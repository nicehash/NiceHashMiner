using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Configs.ELPDataModels;
using NHMCore.Configs.Managers;
using NHMCore.Mining.Plugins;
using NHMCore.Nhmws.V4;
using NHMCore.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using static NHMCore.Configs.Managers.OCManager;

namespace NHMCore.Mining
{
    public class AlgorithmContainer : NotifyChangedBase
    {
        public Algorithm Algorithm { get; private set; }

        public PluginContainer PluginContainer { get; private set; }

        public ComputeDevice ComputeDevice { get; private set; }

        public string PluginName => PluginContainer?.Name ?? "N/A";

        public Version ConfigVersion { get; internal set; } = new Version(1, 0);
        public Version PluginVersion => PluginContainer?.Version;

        public DateTime IgnoreUntil { get; internal set; } = DateTime.UtcNow;
        private List<double> _powerUsageHistory = new List<double>();
        private List<double> _speedHistory = new List<double>();
        private string _TAG = string.Empty;

        // status is always calculated
        public AlgorithmStatus Status
        {
            get
            {
                if (!Enabled) return AlgorithmStatus.Disabled;
                if (IsCurrentlyMining) return AlgorithmStatus.Mining;

                // TODO errors
                if (BenchmarkErrorMessage != null)
                {
                    return AlgorithmStatus.ErrorBenchmark;
                }
                // pending states
                if (IsBenchmarking) return AlgorithmStatus.Benchmarking;
                if (IsBenchmarkPending) return AlgorithmStatus.BenchmarkPending;

                // order matters here!!!
                if (!EstimatedProfitAllSMAPresent) return AlgorithmStatus.MissingSMA;
                if (!EstimatedProfitAllSMAPositiveOrZero) return AlgorithmStatus.ErrorNegativeSMA;
                if (Speeds.Sum() == 0) return AlgorithmStatus.NoBenchmark;
                if (IsReBenchmark) return AlgorithmStatus.ReBenchmark;

                if (0 >= CurrentEstimatedProfit) return AlgorithmStatus.Unprofitable;

                if (IgnoreUntil > DateTime.UtcNow) return AlgorithmStatus.Unstable;

                return AlgorithmStatus.Benchmarked;
            }
        }

        public AlgorithmContainer(Algorithm algorithm, PluginContainer pluginContainer, ComputeDevice computeDevice)
        {
            PluginContainer = pluginContainer;
            Algorithm = algorithm;
            ComputeDevice = computeDevice;
            _TAG = $"AC->{pluginContainer.Name}/{algorithm.AlgorithmName}/{computeDevice.Name}";

            computeDevice.PropertyChanged += ComputeDevice_PropertyChanged;
            SwitchSettings.Instance.PropertyChanged += SettingsChanged;
            GUISettings.Instance.PropertyChanged += SettingsChanged;
            OnPropertyChanged(nameof(IsUserEditable));
        }

        private void ComputeDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(NHMCore.Mining.ComputeDevice.State) == e.PropertyName)
            {
#if NHMWS4
                var miningOrBenchmarking = ComputeDevice.State == DeviceState.Benchmarking || ComputeDevice.State == DeviceState.Mining || ComputeDevice.State == DeviceState.Testing;
#else
                var miningOrBenchmarking = ComputeDevice.State == DeviceState.Benchmarking || ComputeDevice.State == DeviceState.Mining;
#endif
                IsUserEditable = !miningOrBenchmarking;
                OnPropertyChanged(nameof(IsUserEditable));
            }
        }

        public MiningPair ToMiningPair()
        {
            return new MiningPair
            {
                Device = ComputeDevice.BaseDevice,
                Algorithm = Algorithm
            };
        }
        public DeviceELPData FindInELPTree(string deviceUUID) => ELPManager.Instance.FindDeviceNode(this, deviceUUID);
        public void UpdateConfigVersionIfNeeded()
        {
            if ((_powerUsageHistory.Count >= 2 && _powerUsageHistory.Last() != _powerUsageHistory[_powerUsageHistory.Count - 2]) ||
                (_speedHistory.Count >= 2 && _speedHistory.Last() != _speedHistory[_speedHistory.Count - 2]))
            {
                ConfigVersion = PluginVersion;
            }
        }
        public bool IsUserEditable { get; private set; } = true;

        /// <summary>
        /// Used for converting SMA values to BTC/H/Day
        /// </summary>
        protected const double Mult = 0.000000001;

        // so we don't want to go to a benchmark loop when benchmarking fails
        private bool _lastBenchmarkingFailed = false;
        public bool LastBenchmarkingFailed
        {
            get
            {
                return _lastBenchmarkingFailed;
            }
            set
            {
                _lastBenchmarkingFailed = value;
            }
        }

        #region Identity

        /// <summary>
        /// Friendly display name for this algorithm
        /// </summary>
        public string AlgorithmName => Algorithm?.AlgorithmName ?? "";

        /// <summary>
        /// Friendly name for this algorithm/miner combo
        /// </summary>
        public string AlgorithmStringID => Algorithm?.AlgorithmStringID ?? "";
        /// <summary>
        /// AlgorithmType used by this Algorithm
        /// </summary>
        public AlgorithmType[] IDs => Algorithm.IDs.ToArray();

        public string MinerUUID => Algorithm?.MinerID;
        public bool IsDual => Algorithm.IDs.Count > 1;
        #endregion


        #region Mining settings

        /// <summary>
        /// Hashrate in H/s set by benchmark or user
        /// </summary>
        public double BenchmarkSpeed
        {
            get
            {
                if (BuildOptions.FORCE_MINING)
                {
                    return 1000;
                }
                return Algorithm.Speeds[0];
            }
            set
            {
                _speedHistory.Add(value);
                Algorithm.Speeds[0] = value;
                UpdateConfigVersionIfNeeded();
                NotifySpeedChanged();
                Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
            }
        }

        public double SecondaryBenchmarkSpeed
        {
            get
            {
                if (IsDual) return Algorithm.Speeds[1];
                return 0d;
            }
            set
            {
                if (IsDual) Algorithm.Speeds[1] = value;
                NotifySpeedChanged();
            }
        }

        public List<double> Speeds
        {
            get
            {
                return Algorithm.Speeds.ToList();
            }
            set
            {
                for (var i = 0; i < Algorithm.Speeds.Count && i < value.Count; i++)
                {
                    Algorithm.Speeds[i] = value[i];
                }
                _speedHistory.Add(Algorithm.Speeds[0]);
                UpdateConfigVersionIfNeeded();
                NotifySpeedChanged();
            }
        }

        public IReadOnlyList<Hashrate> AnnotatedSpeeds
        {
            get
            {
                var list = new List<Hashrate>();
                for (var i = 0; i < IDs.Length; i++)
                {
                    var speed = 0d;
                    if (Speeds.Count > i) speed = Speeds[i];
                    list.Add(new Hashrate(speed, IDs[i]));
                }

                return list;
            }
        }


        /// <summary>
        /// Get or set whether this algorithm is enabled for mining
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                if (Algorithm == null) return false;
                return Algorithm.Enabled;
            }
            set
            {
                if (Algorithm != null) Algorithm.Enabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
                Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus());
                if (MinerUUID == null || MinerUUID == string.Empty) return; //initial stuff
                var notifType = value ? EventType.AlgoEnabled : EventType.AlgoDisabled;
                EventManager.Instance.AddEvent(notifType, AlgorithmName);
            }
        }
        public void SetEnabled(bool enabled) //for enable without WS (bulk setting)
        {
            if (Algorithm != null) Algorithm.Enabled = enabled;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Enabled));
            OnPropertyChanged(nameof(Status));
        }
        /// <summary>
        /// Indicates whether this algorithm requires a benchmark
        /// </summary>
        public virtual bool BenchmarkNeeded => BenchmarkSpeed <= 0 && !LastBenchmarkingFailed;

        public bool HasBenchmark => Speeds.Sum() > 0;

        protected void NotifySpeedChanged()
        {
            OnPropertyChanged(nameof(BenchmarkSpeed));
            OnPropertyChanged(nameof(SecondaryBenchmarkSpeed));
            OnPropertyChanged(nameof(Speeds));
            OnPropertyChanged(nameof(AnnotatedSpeeds));
            OnPropertyChanged(nameof(BenchmarkNeeded));
            OnPropertyChanged(nameof(CurrentEstimatedProfit));
            OnPropertyChanged(nameof(CurrentEstimatedProfitPure));
            OnPropertyChanged(nameof(CurrentEstimatedProfitStr));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(HasBenchmark));
        }
        protected void NotifyPowerChanged()
        {
            OnPropertyChanged(nameof(CurrentEstimatedProfit));
            OnPropertyChanged(nameof(CurrentEstimatedProfitStr));
            OnPropertyChanged(nameof(CurrentEstimatedProfitPure));
        }

        #endregion

        #region Profitability

        #region EstimatedProfit NOT FOR SWITCHING


        internal void UpdateEstimatedProfit(Dictionary<AlgorithmType, double> profits)
        {
            _updateEstimatedProfitCalled = true;
            if (profits == null) return;
            var containedIDs = profits.Keys.Where(key => IDs.Contains(key)).ToArray();

            EstimatedProfitAllSMAPresent = IDs.Length == containedIDs.Length;
            EstimatedProfitAllSMAPositiveOrZero = EstimatedProfitAllSMAPresent && IDs.All(id => profits[id] >= 0);

            // clear old values
            _lastEstimatedProfitSMA.Clear();
            foreach (var id in containedIDs) _lastEstimatedProfitSMA[id] = profits[id];

            // notify changed
            OnPropertyChanged(nameof(CurrentEstimatedProfit));
            OnPropertyChanged(nameof(CurrentEstimatedProfitStr));
            OnPropertyChanged(nameof(CurrentEstimatedProfitPure));
            OnPropertyChanged(nameof(Status));
        }

        private bool _updateEstimatedProfitCalled = false;
        internal readonly Dictionary<AlgorithmType, double> _lastEstimatedProfitSMA = new Dictionary<AlgorithmType, double>();

        private bool EstimatedProfitAllSMAPresent = false;
        private bool EstimatedProfitAllSMAPositiveOrZero = false;

        public double CurrentEstimatedProfit
        {
            get
            {
                if (BuildOptions.FORCE_MINING)
                {
                    return 1000;
                }

                if (!_updateEstimatedProfitCalled) return -2;

                if (EstimatedProfitAllSMAPresent && EstimatedProfitAllSMAPositiveOrZero)
                {
                    var newProfit = 0d;
                    foreach (var speed in AnnotatedSpeeds)
                    {
                        var paying = _lastEstimatedProfitSMA[speed.Algo];
                        newProfit += paying * speed.Value * Mult;
                    }
                    return Math.Round(newProfit, 8);
                }
                // we can't calculate 
                return -1;
            }
        }
        public double CurrentEstimatedProfitPure
        {
            get
            {
                if (GUISettings.Instance.DisplayPureProfit)
                {
                    var power = (PowerUsage / 1000 * BalanceAndExchangeRates.Instance.GetKwhPriceInBtc()) * 24;
                    return (CurrentEstimatedProfit - power);
                }
                return CurrentEstimatedProfit;
            }
        }
        public string CurrentEstimatedProfitStr
        {

            get
            {
                if (GUISettings.Instance.DisplayPureProfit)
                {
                    var power = (PowerUsage / 1000 * BalanceAndExchangeRates.Instance.GetKwhPriceInBtc()) * 24;
                    return (CurrentEstimatedProfit - power).ToString("0.00000000");
                }
                var currentEstimatedProfit = CurrentEstimatedProfit;
                // WPF or null
                if (currentEstimatedProfit < 0) return "---";
                // TODO BTC scaling
                return currentEstimatedProfit.ToString("0.00000000");
            }
        }

        [Obsolete("TODO DELETE WinForms")]
        public string CurPayingRatioStr
        {
            get
            {
                if (!_updateEstimatedProfitCalled || !EstimatedProfitAllSMAPresent) return Translations.Tr("N/A");
                var payingRatios = IDs.Select(id => _lastEstimatedProfitSMA[id]).ToArray();
                if (payingRatios.Length > 0)
                {
                    return string.Join("+", payingRatios);
                }
                return Translations.Tr("N/A");
            }
        }

        #endregion EstimatedProfit NOT FOR SWITCHING

        #region NormalizedProfit FOR SWITCHING
        // TODO with this implementation WE ONLY SUPPORT dual algorithms
        /// <summary>
        /// Gets the averaged speed for this algorithm in H/s
        /// <para>When multiple devices of the same model are used, this will be set to their averaged hashrate</para>
        /// </summary>
        public double[] AveragedSpeeds { get; private set; } = new double[2] { 0.0d, 0.0d };
        /// <summary>
        /// Current SMA profitability for this algorithm type in BTC/GH/Day
        /// </summary>
        public double[] NormalizedSMAData { get; private set; } = new double[2] { 0.0d, 0.0d };

        /// <summary>
        /// Current profit for this algorithm in BTC/Day
        /// </summary>
        public double CurrentNormalizedProfit { get; protected set; }



        #endregion NormalizedProfit FOR SWITCHING




        /// <summary>
        /// Power consumption of this algorithm, in Watts
        /// </summary>
        private double _powerUsage = 0;
        public virtual double PowerUsage
        {
            get
            {
                return _powerUsage;
            }
            set
            {
                _powerUsageHistory.Add(value);
                _powerUsage = value;
                UpdateConfigVersionIfNeeded();
                OnPropertyChanged(nameof(PowerUsage));
                NotifyPowerChanged();
            }
        }

        #endregion

        private bool _isReBenchmark = false;
        public bool IsReBenchmark
        {
            get => _isReBenchmark;
            set
            {
                _isReBenchmark = value;
                OnPropertyChanged(nameof(IsReBenchmark));
                OnPropertyChanged(nameof(Status));
            }
        }

        private bool _isCurrentlyMining = false;
        internal bool IsCurrentlyMining
        {
            get => _isCurrentlyMining;
            set
            {
                _isCurrentlyMining = value;
                OnPropertyChanged(nameof(IsCurrentlyMining));
                OnPropertyChanged(nameof(Status));
            }
        }



        #region Benchmark info

        private bool _benchmarkPending;
        public bool IsBenchmarkPending
        {
            get => _benchmarkPending;
            private set
            {
                _benchmarkPending = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
            }
        }

        private bool _inBenchmark;
        public bool IsBenchmarking
        {
            get => _inBenchmark;
            set
            {
                _inBenchmark = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool BenchmarkErred => BenchmarkErrorMessage != null;

        private string _errorMessage;
        public string BenchmarkErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BenchmarkErred));
                OnPropertyChanged(nameof(Status));
            }
        }

        public void ClearSpeeds()
        {
            var allZero = this.Speeds.Select(v => 0d).ToList();
            this.Speeds = allZero;
        }



        #endregion

        #region Benchmark methods

        public void SetBenchmarkPending()
        {
            IsBenchmarkPending = true;
            BenchmarkErrorMessage = null;
        }

        public void ClearBenchmarkPending()
        {
            IsBenchmarkPending = false;
        }

        public void SetBenchmarkError(string message)
        {
            BenchmarkErrorMessage = message;
        }

        #endregion

        #region Profitability methods

        public virtual void UpdateCurrentNormalizedProfit(Dictionary<AlgorithmType, double> profits)
        {
            var newProfit = 0d;
            for (int i = 0; i < IDs.Length; i++)
            {
                var id = IDs[i];
                if (profits.TryGetValue(id, out var paying) == false)
                {
                    NormalizedSMAData[i] = -1;
                    continue;
                }
                NormalizedSMAData[i] = paying;
                newProfit += paying * AveragedSpeeds[i] * Mult;
            }
            CurrentNormalizedProfit = newProfit;

            if (!MiningProfitSettings.Instance.MineRegardlessOfProfit)
            {
                // This is power usage in BTC/hr
                var power = PowerUsage / 1000 * BalanceAndExchangeRates.Instance.GetKwhPriceInBtc();
                // Now it is power usage in BTC/day
                power *= 24;
                // Now we subtract from profit, which may make profit negative
                CurrentNormalizedProfit -= power;
            }
        }
        void SettingsChanged(object sender, EventArgs e)
        {
            NotifyPowerChanged();
        }

        #endregion


        internal bool IgnoreLocalELPInput //if ignore local ELPs for rig manager ones
        {
            get
            {
#if NHMWS4
                if (ActiveELPProfile != null || ActiveELPTestProfile != null) return true;
                return false;
#else
                return false;
#endif
            }
        }
#if NHMWS4
        private readonly object _lock = new object();
        public enum ProfileType
        {
            None,
            Normal,
            Test,
        }
        public ProfileType GetTargetProfileType()
        {
            if (ActiveFanTestProfile != null || ActiveOCTestProfile != null || ActiveELPTestProfile != null) return ProfileType.Test;
            if (ActiveFanProfile != null || ActiveOCProfile != null || ActiveELPProfile != null) return ProfileType.Normal;
            return ProfileType.None;
        }
        public bool HasNormalProfileToSet()
        {
            return ActiveFanProfile != null || ActiveOCProfile != null || ActiveELPProfile != null;
        }
        public bool HasTestProfileToSet()
        {
            return ActiveFanTestProfile != null || ActiveOCTestProfile != null || ActiveELPTestProfile != null;
        }
        public bool HasTestProfileAndCanSet()
        {
            return GetTargetProfileType() == ProfileType.Test && HasTestProfileToSet();
        }
        public bool HasNormalProfileAndCanSet()
        {
            return GetTargetProfileType() == ProfileType.Normal && HasNormalProfileToSet();
        }
        public enum ActionQueue
        {
            ApplyOC,
            ResetOC,
            ResetOCTest,
            ApplyOCTest,
            ApplyFan,
            ResetFan,
            ResetFanTest,
            ApplyFanTest,
            ApplyELP,
            ResetELP,
            ApplyELPTest,
            ResetELPTest
        }
        private Queue<ActionQueue> _rigManagementActions = new Queue<ActionQueue>();
        public Queue<ActionQueue> RigManagementActions
        {
            get
            {
                lock (_lock)
                {
                    return _rigManagementActions;
                }
            }
            set
            {
                lock (_lock)
                {
                    _rigManagementActions = value;
                }
            }
        }
        public bool IsTesting => HasTestProfileToSet();
        #region OC
        public string OCProfile
        {
            get
            {
                if (ActiveOCTestProfile != null) return ActiveOCTestProfile.Name;
                if (ActiveOCProfile != null && !IsTesting) return ActiveOCProfile.Name;
                return string.Empty;
            }
        }
        public string OCProfileID
        {
            get
            {
                if (ActiveOCTestProfile != null) return ActiveOCTestProfile.Id;
                if (ActiveOCProfile != null && !IsTesting) return ActiveOCProfile.Id;
                return string.Empty;
            }
        }


        private OcProfile _ActiveOCTestProfile = null;
        public OcProfile ActiveOCTestProfile
        {
            get
            {
                lock (_lock)
                {
                    return _ActiveOCTestProfile;
                }
            }
            set
            {
                lock (_lock)
                {
                    _ActiveOCTestProfile = value;
                }
            }
        }
        private OcProfile _ActiveOCProfile = null;
        public OcProfile ActiveOCProfile
        {
            get
            {
                lock (_lock)
                {
                    return _ActiveOCProfile;
                }
            }
            set
            {
                lock (_lock)
                {
                    _ActiveOCProfile = value;
                }
            }
        }
        public void SetTargetOcProfile(OcProfile profile, bool test)
        {
            if (test)
            {
                ActiveOCTestProfile = profile;
                RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetOCTest : ActionQueue.ApplyOCTest);
            }
            else
            {
                ActiveOCProfile = profile;
                RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetOC : ActionQueue.ApplyOC);
            }
        }
        public void SwitchOCTestToInactive()
        {
            ActiveOCTestProfile = null;
        }
        public void SwitchOCToInactive()
        {
            ActiveOCProfile = null;
        }
        public Task<RigManagementReturn> SetOcForDevice(OcProfile bundle, bool reset = false)
        {
            //if (bundle != null) Logger.Warn(_TAG, $"Setting OC for {ComputeDevice.Name}: TDP={bundle.TDP},CC={bundle.CoreClock},MC={bundle.MemoryClock}");
            var ret = RigManagementReturn.Fail;
            int valuesToSet = 0;
            bool willSetCC = false;
            bool willSetCCDelta = false;
            bool willSetMC = false;
            bool willSetMCDelta = false;

            if (bundle.CoreClockDelta != null) willSetCCDelta = true;
            if (bundle.CoreClock != null) willSetCC = true;
            if (bundle.MemoryClockDelta != null) willSetMCDelta = true;
            if (bundle.MemoryClock != null) willSetMC = true;

            if (willSetCC || willSetCCDelta) valuesToSet++;
            if (willSetMC || willSetMCDelta) valuesToSet++;
            if (bundle.TDP != null) valuesToSet++;
            if (bundle.CoreVoltage != null) valuesToSet++;

            if (valuesToSet == 0 && !reset)
            {
                Logger.Error(_TAG, "Have no values to set");
                return Task.FromResult(ret);
            }

            int setValues = 4;
            var setTDP = bundle.TDP == null ? false : ComputeDevice.SetPowerModeManual((int)bundle.TDP);
            var setCCabs = willSetCC ? ComputeDevice.SetCoreClock((int)bundle.CoreClock) : false;
            var setCCdelta = willSetCCDelta ? ComputeDevice.SetCoreClockDelta((int)bundle.CoreClockDelta) : false;
            var setMCabs = willSetMC ? ComputeDevice.SetMemoryClock((int)bundle.MemoryClock) : false;
            var setMCdelta = willSetMCDelta ? ComputeDevice.SetMemoryClockDelta((int)bundle.MemoryClockDelta) : false;
            var setCV = bundle.CoreVoltage == null ? false : ComputeDevice.SetCoreVoltage((int)bundle.CoreVoltage);

            var setCC = setCCabs || setCCdelta;
            var setMC = setMCabs || setMCdelta;

            if (reset)
            {
                setCV = ComputeDevice.ResetCoreVoltage();
                bool setCC1 = ComputeDevice.ResetCoreClock();
                bool setCC2 = ComputeDevice.ResetCoreClockDelta();
                bool setMC1 = ComputeDevice.ResetMemoryClock();
                bool setMC2 = ComputeDevice.ResetMemoryClockDelta();
                setCC = setCC1 || setCC2;
                setMC = setMC1 || setMC2;
            }

            if (!setCC)
            {
                Logger.Warn(_TAG, $"Setting core clock success: {setCC}");
                setValues--;
            }
            if (!setMC)
            {
                Logger.Warn(_TAG, $"Setting memory clock success: {setMC}");
                setValues--;
            }
            if (!setTDP)
            {
                Logger.Warn(_TAG, $"Setting TDP success: {setTDP}");
                setValues--;
            }
            if(!setCV)
            {
                Logger.Warn(_TAG, $"Setting voltage success: {setCV}");
                setValues--;
            }

            if (setValues == valuesToSet) ret = RigManagementReturn.Success;
            else if (setValues != 0 && setValues < valuesToSet) ret = RigManagementReturn.PartialSuccess;

            if (!reset && (ret == RigManagementReturn.Success || ret == RigManagementReturn.PartialSuccess))
            {
                Logger.Warn(_TAG, $"Setting OC is successful");
                return Task.FromResult(ret);
            }
            Logger.Warn(_TAG, $"OC not in test mode anymore");
            return Task.FromResult(ret);
        }
        public Task<RigManagementReturn> ResetOcForDevice()
        {
            var defTDP = ComputeDevice.TDPLimits;
            var bundle = new OcProfile() { TDP = (int)defTDP.def }; // tdp is only value without reset
            var res = SetOcForDevice(bundle, true);
            return Task.FromResult(res.Result);
        }
        #endregion
        #region ELP
        public string DelayedELPString = string.Empty;
        public string ELPProfile
        {
            get
            {
                if (ActiveELPTestProfile != null) return ActiveELPTestProfile.Name;
                if (ActiveELPProfile != null && !IsTesting) return ActiveELPProfile.Name;
                return string.Empty;
            }
        }
        public string ELPProfileID
        {
            get
            {
                if (ActiveELPTestProfile != null) return ActiveELPTestProfile.Id;
                if (ActiveELPProfile != null && !IsTesting) return ActiveELPProfile.Id;
                return string.Empty;
            }
        }
        private ElpProfile _ActiveELPTestProfile = null;
        public ElpProfile ActiveELPTestProfile
        {
            get
            {
                lock (_lock)
                {
                    return _ActiveELPTestProfile;
                }
            }
            set
            {
                lock (_lock)
                {
                    if ((value != null && _ActiveELPTestProfile == null) ||
                    (value != null && _ActiveELPTestProfile != null))
                    {
                        ELPTestChange = true;
                    }
                    else if (value == null && _ActiveELPTestProfile != null)
                    {
                        ELPTestChange = false;
                    }
                    _ActiveELPTestProfile = value;
                }
            }
        }
        private ElpProfile _ActiveELPProfile = null;
        public ElpProfile ActiveELPProfile
        {
            get
            {
                lock (_lock)
                {
                    return _ActiveELPProfile;
                }
            }
            set
            {
                lock (_lock)
                {
                    if ((value != null && _ActiveELPProfile == null) ||
                    (value != null && _ActiveELPProfile != null))
                    {
                        ELPChange = true;
                    }
                    else if (value == null && _ActiveELPProfile != null)
                    {
                        ELPChange = false;
                    }
                    _ActiveELPProfile = value;
                }
            }
        }
        private bool _newTestProfile = false;
        private bool _newProfile = false;
        public bool NewTestELPProfile
        {
            get
            {
                lock (_lock)
                {
                    return _newTestProfile;
                }
            }
            set
            {
                lock (_lock)
                {
                    _newTestProfile = value;
                }
            }
        }
        public bool NewELPProfile
        {
            get
            {
                lock (_lock)
                {
                    return _newProfile;
                }
            }
            set
            {
                lock (_lock)
                {
                    _newProfile = value;
                }
            }
        }
        private bool _ELPChange = false;
        public bool ELPChange
        {
            get
            {
                lock (_lock)
                {
                    return _ELPChange;
                }
            }
            set
            {
                lock (_lock)
                {
                    _ELPChange = value;
                }
            }
        }
        private bool _ELPTestChange = false;
        public bool ELPTestChange
        {
            get
            {
                lock (_lock)
                {
                    return _ELPTestChange;
                }
            }
            set
            {
                lock (_lock)
                {
                    _ELPTestChange = value;
                }
            }
        }
        public void ResetNewTestProfileStatus() { NewTestELPProfile = false; }
        public void ResetNewProfileStatus() { NewELPProfile = false; }
        public void SetTargetElpProfile(ElpProfile profile, bool test)
        {
            if (test)
            {
                ActiveELPTestProfile = profile;
                NewTestELPProfile = true;
                RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetELPTest : ActionQueue.ApplyELPTest);
            }
            else
            {
                ActiveELPProfile = profile;
                NewELPProfile = true;
                RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetELP : ActionQueue.ApplyELP);
            }
            SetELPForDevice(profile == null);
            OnPropertyChanged(nameof(IgnoreLocalELPInput));
        }
        public void TriggerELPReset()
        {
            NewELPProfile = true;
        }
        public RigManagementReturn SetELPForDevice(bool reset = false)
        {
            var ret = RigManagementReturn.Success;
            if (!reset)
            {
                var cmd = string.Empty;
                if (ActiveELPProfile != null && !IsTesting) cmd = ActiveELPProfile.Elp;
                if (ActiveELPTestProfile != null) cmd = ActiveELPTestProfile.Elp;
                Logger.Warn(_TAG, $"Setting ELP for {ComputeDevice.Name}: ELP={cmd}");
                ELPManager.Instance.SetAlgoCMDString(this, cmd);
                Logger.Warn(_TAG, $"Setting ELP is successful");
                return ret;
            }
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
            Logger.Warn(_TAG, $"ELP not in test mode anymore");
            return ret;
        }
        #endregion
        #region FAN
        public string FanProfile
        {
            get
            {
                if (ActiveFanTestProfile != null) return ActiveFanTestProfile.Name;
                if (ActiveFanProfile != null && !IsTesting) return ActiveFanProfile.Name;
                return string.Empty;
            }
        }
        public string FanProfileID
        {
            get
            {
                if (ActiveFanTestProfile != null) return ActiveFanTestProfile.Id;
                if (ActiveFanProfile != null && !IsTesting) return ActiveFanProfile.Id;
                return string.Empty;
            }
        }
        private FanProfile _ActiveFanTestProfile = null;
        public FanProfile ActiveFanTestProfile
        {
            get
            {
                lock (_lock)
                {
                    return _ActiveFanTestProfile;
                }
            }
            set
            {
                lock (_lock)
                {
                    _ActiveFanTestProfile = value;
                }
            }
        }
        private FanProfile _ActiveFanProfile = null;
        public FanProfile ActiveFanProfile
        {
            get
            {
                lock (_lock)
                {
                    return _ActiveFanProfile;
                }
            }
            set
            {
                lock (_lock)
                {
                    _ActiveFanProfile = value;
                }
            }
        }
        public void SetTargetFanProfile(FanProfile profile, bool test)
        {
            if (test)
            {
                ActiveFanTestProfile = profile;
                RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetFanTest : ActionQueue.ApplyFanTest);
            }
            else
            {
                ActiveFanProfile = profile;
                RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetFan : ActionQueue.ApplyFan);
            }
        }
        public Task<RigManagementReturn> ResetFanForDevice()
        {
            return Task.FromResult(RigManagementReturn.Fail);
        }
        public Task<RigManagementReturn> SetFanForDevice(FanProfile bundle, bool reset = false)
        {
            return Task.FromResult(RigManagementReturn.Fail);
        }
        public void SwitchFanTestToInactive()
        {
            ActiveFanTestProfile = null;
        }
        public void SwitchFanToInactive()
        {
            ActiveFanProfile = null;
        }
        #endregion
#endif
    }
}
