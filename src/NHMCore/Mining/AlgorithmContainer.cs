using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using NHMCore.Mining.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

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

                return AlgorithmStatus.Benchmarked;
            }
        }

        public AlgorithmContainer(Algorithm algorithm, PluginContainer pluginContainer, ComputeDevice computeDevice)
        {
            PluginContainer = pluginContainer;
            Algorithm = algorithm;
            ComputeDevice = computeDevice;

            computeDevice.PropertyChanged += ComputeDevice_PropertyChanged;
            OnPropertyChanged(nameof(IsUserEditable));
        }

        private void ComputeDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(NHMCore.Mining.ComputeDevice.State) == e.PropertyName)
            {
                var miningOrBenchmarking = ComputeDevice.State == DeviceState.Benchmarking || ComputeDevice.State == DeviceState.Mining;
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
                Algorithm.Speeds[0] = value;
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
        /// String containing raw extralaunchparams entered by user
        /// </summary>
        public string ExtraLaunchParameters
        {
            get
            {
                if (Algorithm == null) return "";
                return Algorithm.ExtraLaunchParameters;
            }
            set
            {
                if (Algorithm != null) Algorithm.ExtraLaunchParameters = value;
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
            }
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
            OnPropertyChanged(nameof(CurrentEstimatedProfitStr));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(HasBenchmark));
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
                    // TODO estimate profit subtraction ???
                    return Math.Round(newProfit, 8);
                }
                // we can't calculate 
                return -1;
            }
        }
        public string CurrentEstimatedProfitStr
        {

            get
            {
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
        public virtual double PowerUsage { get; set; }

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

        #endregion
    }
}
