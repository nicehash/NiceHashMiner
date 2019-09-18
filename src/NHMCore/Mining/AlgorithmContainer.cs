using System;
using System.Linq;
using System.Collections.Generic;
using NHM.Common;
using NHMCore.Stats;
using NHMCore.Switching;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using NHMCore.Configs;
using NHMCore.Utils;
using NHMCore.Mining.Plugins;

namespace NHMCore.Mining
{
    public class AlgorithmContainer : NotifyChangedBase
    {
        public Algorithm Algorithm { get; private set; }
        
        public PluginContainer PluginContainer { get; private set; }

        public ComputeDevice ComputeDevice { get; private set; }

        public string PluginName => PluginContainer?.Name ?? "N/A";

        public Version ConfigVersion { get; set; } = new Version(1, 0);
        public Version PluginVersion => PluginContainer?.Version;

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

        #region Identity

        /// <summary>
        /// Friendly display name for this algorithm
        /// </summary>
        public string AlgorithmName => Algorithm?.AlgorithmName ?? "";
        /// <summary>
        /// Friendly name for miner type
        /// </summary>
        public string MinerBaseTypeName
        {
            get
            {
                if (PluginContainer == null) return "";
                var isIntegrated = PluginContainer.IsIntegrated;
                var minerName = PluginContainer.Name + (isIntegrated ? "" : "(PLUGIN)");
                return minerName;
            }
        }
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
        /// Gets the averaged speed for this algorithm in H/s
        /// <para>When multiple devices of the same model are used, this will be set to their averaged hashrate</para>
        /// </summary>
        public double AvaragedSpeed { get; set; }

        public double SecondaryAveragedSpeed { get; set; }

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
        public virtual bool BenchmarkNeeded => BenchmarkSpeed <= 0;

        protected void NotifySpeedChanged()
        {
            OnPropertyChanged(nameof(BenchmarkSpeed));
            OnPropertyChanged(nameof(SecondaryBenchmarkSpeed));
            OnPropertyChanged(nameof(Speeds));
            OnPropertyChanged(nameof(AnnotatedSpeeds));
            OnPropertyChanged(nameof(BenchmarkNeeded));
            OnPropertyChanged(nameof(CurPayingRateStr));
        }

        #endregion

        #region Profitability

        /// <summary>
        /// Current profit for this algorithm in BTC/Day
        /// </summary>
        public double CurrentProfit { get; protected set; }
        /// <summary>
        /// Current SMA profitability for this algorithm type in BTC/GH/Day
        /// </summary>
        public double CurNhmSmaDataVal { get; private set; }
        public double SecondaryCurNhmSmaDataVal { get; private set; }

        /// <summary>
        /// Power consumption of this algorithm, in Watts
        /// </summary>
        public virtual double PowerUsage { get; set; }

        #endregion

        public bool IsReBenchmark { get; set; } = false;

        #region Benchmark info

        public string BenchmarkStatus { get; set; }

        private bool _benchmarkPending;
        public bool IsBenchmarkPending
        {
            get => _benchmarkPending;
            private set
            {
                _benchmarkPending = value;
                OnPropertyChanged();
            }
        }

        private bool _inBenchmark;
        public bool InBenchmark
        {
            get => _inBenchmark;
            set
            {
                _inBenchmark = value;
                OnPropertyChanged();
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

        public string CurPayingRatio
        {
            get
            {
                var payingRatios = new List<double>();
                for (int i = 0; i < IDs.Count(); i++)
                {
                    var id = IDs[i];
                    if (NHSmaData.TryGetPaying(id, out var paying) == false) continue;
                    payingRatios.Add(paying);
                }
                if (payingRatios.Count > 0)
                {
                    return string.Join("+", payingRatios);
                }
                return Translations.Tr("N/A");
            }
        }

#if FORCE_MINING
        public double CurPayingRate
        {
            get
            {
                return 1000;
            }
        }
#else
        public double CurPayingRate
        {
            get
            {
                var payingRate = 0d;
                for (int i = 0; i < IDs.Count(); i++)
                {
                    var id = IDs[i];
                    if (NHSmaData.TryGetPaying(id, out var paying) == false) continue;
                    var speed = Speeds[i];
                    payingRate += speed * paying * Mult;
                }
                return payingRate;
            }
        }
#endif

        public string CurPayingRateStr
        {
            get
            {
                var payingRate = 0d;
                for (int i = 0; i < IDs.Count(); i++)
                {
                    var id = IDs[i];
                    if (NHSmaData.TryGetPaying(id, out var paying) == false) continue;
                    var speed = Speeds[i];
                    payingRate += speed * paying * Mult;
                }
                if (payingRate > 0)
                {
                    return payingRate.ToString("F8");
                }
                return Translations.Tr("N/A"); ;
            }
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
            if (IsPendingString())
            {
                BenchmarkStatus = "";
            }
        }

        public virtual void ClearBenchmarkPendingFirst()
        {
            IsBenchmarkPending = false;
            BenchmarkStatus = "";
        }

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

        public virtual void UpdateCurProfit(Dictionary<AlgorithmType, double> profits)
        {
            var newProfit = 0d;
            for (int i = 0; i < IDs.Length; i++)
            {
                var id = IDs[i];
                if (profits.TryGetValue(id, out var paying) == false) continue;
                if (i == 0)
                {
                    CurNhmSmaDataVal = paying;
                    newProfit += CurNhmSmaDataVal * AvaragedSpeed * Mult;
                }
                else if (i == 1)
                {
                    SecondaryCurNhmSmaDataVal = paying;
                    newProfit += SecondaryCurNhmSmaDataVal * SecondaryAveragedSpeed * Mult;
                }
            }
            CurrentProfit = newProfit;
            //profits.TryGetValue(NiceHashID, out var paying);
            //CurNhmSmaDataVal = paying;
            //CurrentProfit = CurNhmSmaDataVal * AvaragedSpeed * Mult;
            if (!ConfigManager.IsMiningRegardlesOfProfit) SubtractPowerFromProfit();
        }

        protected void SubtractPowerFromProfit()
        {
            // This is power usage in BTC/hr
            var power = PowerUsage / 1000 * ExchangeRateApi.GetKwhPriceInBtc();
            // Now it is power usage in BTC/day
            power *= 24;
            // Now we subtract from profit, which may make profit negative
            CurrentProfit -= power;
        }

        #endregion
    }
}
