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
                Task.Run(async () => await NHWebSocketV4.UpdateMinerStatus(true));
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
        public virtual double PowerUsage {
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
        public enum ActionQueue
        {
            ApplyOcTest,
            ApplyOC,
            ResetOC,
            ApplyFanTest,
            ApplyFan,
            ResetFan,
            ApplyELPTest,
            ApplyELP,
            ResetELP
        }
        public Queue<ActionQueue> RigManagementActions = new Queue<ActionQueue>(); //THREAD SAFETY
        private bool _IsTesting = false;
        public bool IsTesting
        {
            get
            {
                return _IsTesting;
            }
            set
            {
                _IsTesting = value;
            }
        }
        #region OC
        public string OCProfile {
            get
            {
                if (ActiveOCTestProfile != null) return ActiveOCTestProfile.Name;
                if (ActiveOCProfile != null) return ActiveOCProfile.Name;
                return string.Empty;
            }
        }
        public string OCProfileID
        {
            get
            {
                if (ActiveOCTestProfile != null) return ActiveOCTestProfile.Id;
                if (ActiveOCProfile != null) return ActiveOCProfile.Id;
                return string.Empty;
            }
        }


        private OcBundle _ActiveOCTestProfile = null;
        public OcBundle ActiveOCTestProfile => _ActiveOCTestProfile;
        private OcBundle TestOcProfilePrev { get; set; }

        private OcBundle _ActiveOCProfile = null;
        public OcBundle ActiveOCProfile => _ActiveOCProfile;
        private OcBundle OcProfilePrev { get; set; }

        public void SetTargetOcTestProfile(OcBundle profile)
        {
            IsTesting = profile == null ? false : true;
            TestOcProfilePrev = ActiveOCTestProfile;
            _ActiveOCTestProfile = profile;
            RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetOC : ActionQueue.ApplyOcTest);
        }
        public void SetTargetOcProfile(OcBundle profile)
        {
            OcProfilePrev = ActiveOCProfile;
            _ActiveOCProfile = profile;
            RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetOC : ActionQueue.ApplyOC);
        }
        public Task<OcReturn> SetOcForDevice(OcBundle bundle, bool test = false, bool reset = false)
        {
            if(bundle != null) Logger.Warn(_TAG, $"Setting OC for {ComputeDevice.Name}: TDP={bundle.TDP},CC={bundle.CoreClock},MC={bundle.MemoryClock}");
            var ret = OcReturn.Fail;
            int valuesToSet = 0;
            if (bundle.CoreClock > 0) valuesToSet++;
            if (bundle.MemoryClock > 0) valuesToSet++;
            if (bundle.TDP > 0) valuesToSet++;
            if (valuesToSet == 0)
            {
                Logger.Error(_TAG, "Have no values to set");
                return Task.FromResult(ret);
            }
            int setValues = 3;
            var setCC = bundle.CoreClock <= 0 ? false : ComputeDevice.SetCoreClock(bundle.CoreClock);
            var setMC = bundle.MemoryClock <= 0 ? false : ComputeDevice.SetMemoryClock(bundle.MemoryClock);
            var setTDP = bundle.TDP <= 0 ? false : ComputeDevice.SetPowerModeManual(bundle.TDP);

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

            if (setValues == valuesToSet) ret = OcReturn.Success;
            else if (setValues != 0 && setValues < valuesToSet) ret = OcReturn.PartialSuccess;

            if (!reset && (ret == OcReturn.Success || ret == OcReturn.PartialSuccess))
            {
                if (test) IsTesting = true;
                Logger.Warn(_TAG, $"Setting OC is successful");
                return Task.FromResult(ret);
            }
            if(test) IsTesting = false;
            Logger.Warn(_TAG, $"OC not in test mode anymore");
            return Task.FromResult(ret);
        }
        public Task<OcReturn> ResetOcForDevice()
        {
            var defCC = ComputeDevice.CoreClockRange;
            var defMC = ComputeDevice.MemoryClockRange;
            var defTDP = ComputeDevice.TDPLimits;
            var bundle = new OcBundle() { CoreClock = defCC.def, MemoryClock = defMC.def, TDP = (int)defTDP.def };
            var res = SetOcForDevice(bundle, IsTesting, true);
            return Task.FromResult(res.Result);
        }
        #endregion
        #region ELP
        public string ELPProfile
        {
            get
            {
                if (ActiveELPTestProfile != null) return ActiveELPTestProfile.Name;
                if (ActiveELPProfile != null) return ActiveELPProfile.Name;
                return string.Empty;
            }
        }
        public string ELPProfileID
        {
            get
            {
                if (ActiveELPTestProfile != null) return ActiveELPTestProfile.Id;
                if (ActiveELPProfile != null) return ActiveELPProfile.Id;
                return string.Empty;
            }
        }
        private ElpBundle _ActiveELPTestProfile = null;
        public ElpBundle ActiveELPTestProfile => _ActiveELPTestProfile;
        private ElpBundle TestELPProfilePrev { get; set; }

        private ElpBundle _ActiveELPProfile = null;
        public ElpBundle ActiveELPProfile => _ActiveELPProfile;
        private ElpBundle ELPProfilePrev { get; set; }
        public bool NewTestProfile = false;
        public bool NewProfile = false;
        public void ResetNewTestProfileStatus() { NewTestProfile = false; }
        public void ResetNewProfileStatus() { NewProfile = false; }
        public Task SetTargetElpTestProfile(ElpBundle profile)
        {
            IsTesting = profile == null ? false : true;
            TestELPProfilePrev = ActiveELPTestProfile;
            _ActiveELPTestProfile = profile;
            NewTestProfile = true;
            SetELPForDevice(profile, IsTesting, profile == null);//todo change reset
            OnPropertyChanged(nameof(IgnoreLocalELPInput));
            return Task.CompletedTask;
        }
        public void SetTargetElpProfile(ElpBundle profile)
        {
            ELPProfilePrev = ActiveELPProfile;
            _ActiveELPProfile = profile;
            NewProfile = true;
            SetELPForDevice(profile, false, profile == null);//todo change reset
            OnPropertyChanged(nameof(IgnoreLocalELPInput));
        }
        public Task<OcReturn> SetELPForDevice(ElpBundle bundle, bool test = false, bool reset = false)
        {
            if (bundle != null) Logger.Warn(_TAG, $"Setting ELP for {ComputeDevice.Name}: ELP={bundle.Elp}");
            var ret = OcReturn.Success;

            if (!reset)
            {
                if (test) IsTesting = true;
                ELPManager.Instance.SetAlgoCMDString(this, bundle.Elp);
                Logger.Warn(_TAG, $"Setting ELP is successful");

                return Task.FromResult(ret);
            }
            if (test) IsTesting = false;
            ELPManager.Instance.IterateSubModelsAndConstructELPs();
            Logger.Warn(_TAG, $"ELP not in test mode anymore");
            return Task.FromResult(ret);
        }
        #endregion
        #region FAN
        public string FanProfile
        {
            get
            {
                if (ActiveFanTestProfile != null) return ActiveFanTestProfile.Name;
                if (ActiveFanProfile != null) return ActiveFanProfile.Name;
                return string.Empty;
            }
        }
        public string FanProfileID
        {
            get
            {
                if (ActiveFanTestProfile != null) return ActiveFanTestProfile.Id;
                if (ActiveFanProfile != null) return ActiveFanProfile.Id;
                return string.Empty;
            }
        }
        private FanBundle _activeFanTestProfile = null;
        public FanBundle ActiveFanTestProfile => _activeFanTestProfile;
        private FanBundle TestFanProfilePrev { get; set; }
        private FanBundle _activeFanProfile = null;
        public FanBundle ActiveFanProfile => _activeFanProfile;
        private FanBundle FanProfilePrev { get; set; }
        public void SetTargetFanTestProfile(FanBundle profile)
        {
            IsTesting = profile == null ? false : true;
            TestFanProfilePrev = ActiveFanTestProfile;
            _activeFanTestProfile = profile;
            RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetFan : ActionQueue.ApplyFanTest);
        }

        public void SetTargetFanProfile(FanBundle profile)
        {
            FanProfilePrev = ActiveFanProfile;
            _activeFanProfile = profile;
            RigManagementActions.Enqueue(profile == null ? ActionQueue.ResetFan : ActionQueue.ApplyFan);
        }

        #endregion
#endif
    }
}
