using System;
using System.Linq;
using System.Collections.Generic;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using NiceHashMiner.Configs;
using NiceHashMiner.Utils;
using NiceHashMiner.Mining.Plugins;

namespace NiceHashMiner.Mining
{
    // TODO find a better name... this depends on the way we will use this. DeviceAlgorithm/MiningAlgorithm, PluginAlgorithmForDevice????
    // AlgorithmContainer??? hold ComputeDevice? PluginContainer?
    public class PluginAlgorithm
    {
        public Algorithm Algorithm { get; private set; }

        public string PluginName { get; private set; }

        public Version ConfigVersion { get; set; } = new Version(1, 0);
        public Version PluginVersion { get; private set; } = new Version(1, 0);

        public PluginAlgorithm(string pluginName, Algorithm algorithm, Version pluginVersion)
        {
            PluginName = pluginName;
            Algorithm = algorithm;
            PluginVersion = pluginVersion;
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
                if (Algorithm == null) return "";
                var plugin = MinerPluginsManager.GetPluginWithUuid(Algorithm.MinerID);
                if (plugin == null) return "";
                var isIntegrated = plugin.IsIntegrated;
                var minerName = plugin.Name + (isIntegrated ? "" : "(PLUGIN)");
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
        public double BenchmarkSpeed
        {
            get
            {
                return Algorithm.Speeds[0];
            }
            set
            {
                Algorithm.Speeds[0] = value;
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
            }
        }

        /// <summary>
        /// Indicates whether this algorithm requires a benchmark
        /// </summary>
        public virtual bool BenchmarkNeeded => BenchmarkSpeed <= 0;

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

        public bool IsBenchmarkPending { get; private set; }

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
            IsBenchmarkPending = true;
            BenchmarkStatus = Translations.Tr("Waiting benchmark");
        }

        public void SetBenchmarkPendingNoMsg()
        {
            IsBenchmarkPending = true;
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
                return Helpers.FormatDualSpeedOutput(BenchmarkSpeed, SecondaryBenchmarkSpeed, IDs);
            }
            if (!IsPendingString() && !string.IsNullOrEmpty(BenchmarkStatus))
            {
                return BenchmarkStatus;
            }
            return Translations.Tr("none");
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
