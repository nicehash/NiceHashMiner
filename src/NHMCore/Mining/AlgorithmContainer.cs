using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using NHMCore.Benchmarking;
using NHMCore.Configs;
using NHMCore.Mining.Plugins;
using NHMCore.Stats;
using NHMCore.Switching;
using NHMCore.Utils;
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

        // status is always calculated
        public AlgorithmStatus Status
        {
            get
            {
                // TODO errors

                // pending states
                if (IsBenchmarking) return AlgorithmStatus.Benchmarking;
                if (IsBenchmarkPending) return AlgorithmStatus.BenchmarkPending;

                // order matters here!!!
                if (!EstimatedProfitAllSMAPresent) return AlgorithmStatus.MissingSMA;
                if (!EstimatedProfitAllSMAPositiveOrZero) return AlgorithmStatus.ErrorNegativeSMA;
                if (Speeds.Sum() == 0) return AlgorithmStatus.NoBenchmark;
                if (IsReBenchmark) return AlgorithmStatus.ReBenchmark;

                
                return AlgorithmStatus.Benchmarked;
            }
        }

        public AlgorithmContainer(Algorithm algorithm, PluginContainer pluginContainer, ComputeDevice computeDevice)
        {
            PluginContainer = pluginContainer;
            Algorithm = algorithm;
            ComputeDevice = computeDevice;
        }

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
                return _lastBenchmarkingFailed && !BenchmarkManager.DisableLastBenchmarkingFailed;
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

#if FORCE_MINING
        public double BenchmarkSpeed
        {
            get
            {
                return 1000;
            }
            set
            {
                Algorithm.Speeds[0] = 1000;
                NotifySpeedChanged();
            }
        }
#else
        public double BenchmarkSpeed
        {
            get
            {
                return Algorithm.Speeds[0];
            }
            set
            {
                Algorithm.Speeds[0] = value;
                NotifySpeedChanged();
            }
        }
#endif

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
            }
        }

        /// <summary>
        /// Indicates whether this algorithm requires a benchmark
        /// </summary>
        public virtual bool BenchmarkNeeded => BenchmarkSpeed <= 0 && !LastBenchmarkingFailed;

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
#if FORCE_MINING
                return 1000;
#endif
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
                return  -1;
            }
        }
        public string CurrentEstimatedProfitStr
        {
            
            get
            {
                var currentEstimatedProfit = CurrentEstimatedProfit;
                if (currentEstimatedProfit < 0)
                {
                    // WPF
                    return "---";
                    // WinForms
                    return Translations.Tr("N/A");
                }
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

        public bool IsReBenchmark { get; set; } = false;

        #region Benchmark info

        public string BenchmarkStatus { get; set; } = "";

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

        public bool BenchmarkErred => ErrorMessage != null;

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BenchmarkErred));
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
            SetBenchmarkPendingNoMsg();
            BenchmarkStatus = Translations.Tr("Waiting benchmark");
        }

        public void SetBenchmarkPendingNoMsg()
        {
            IsBenchmarkPending = true;
            ErrorMessage = null;
        }

        private bool IsPendingString()
        {
            return BenchmarkStatus == Translations.Tr("Waiting benchmark")
                   || BenchmarkStatus == "."
                   || BenchmarkStatus == ".."
                   || BenchmarkStatus == "...";
        }

        public void ClearBenchmarkPending()
        {
            IsBenchmarkPending = false;
            BenchmarkStatus = "";
        }

        [Obsolete("TODO DELETE WinForms")]
        public string BenchmarkSpeedString()
        {
            if (Enabled && IsBenchmarkPending && !string.IsNullOrEmpty(BenchmarkStatus))
            {
                return BenchmarkStatus;
            }
            if (BenchmarkSpeed > 0)
            {
                return Helpers.FormatSpeedOutput(AnnotatedSpeeds);
            }
            if (!IsPendingString() && !string.IsNullOrEmpty(BenchmarkStatus))
            {
                return BenchmarkStatus;
            }
            return Translations.Tr("none");
        }

        public void SetError(string message)
        {
            ErrorMessage = message;
        }

#endregion

#region Profitability methods

        public virtual void UpdateCurrentNormalizedProfit(Dictionary<AlgorithmType, double> profits)
        {
            var newProfit = 0d;
            for (int i = 0; i < IDs.Length; i++)
            {
                var id = IDs[i];
                if (profits.TryGetValue(id, out var paying) == false) {
                    NormalizedSMAData[i] = -1;
                    continue;
                }
                NormalizedSMAData[i] = paying;
                newProfit += paying * AveragedSpeeds[i] * Mult;
            }
            CurrentNormalizedProfit = newProfit;

            if (!ConfigManager.IsMiningRegardlesOfProfit)
            {
                // This is power usage in BTC/hr
                var power = PowerUsage / 1000 * ExchangeRateApi.GetKwhPriceInBtc();
                // Now it is power usage in BTC/day
                power *= 24;
                // Now we subtract from profit, which may make profit negative
                CurrentNormalizedProfit -= power;
            }
        }

#endregion
    }
}
