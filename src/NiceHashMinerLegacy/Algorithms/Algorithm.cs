using System.Linq;
using System.Collections.Generic;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Algorithms
{
    public abstract class Algorithm
    {
        /// <summary>
        /// Used for converting SMA values to BTC/H/Day
        /// </summary>
        protected const double Mult = 0.000000001;

        #region Identity

        /// <summary>
        /// Friendly display name for this algorithm
        /// </summary>
        public abstract string AlgorithmName { get; }
        /// <summary>
        /// Friendly name for miner type
        /// </summary>
        public abstract string MinerBaseTypeName { get; }
        /// <summary>
        /// Friendly name for this algorithm/miner combo
        /// </summary>
        public abstract string AlgorithmStringID { get; }
        /// <summary>
        /// AlgorithmType used by this Algorithm
        /// </summary>
        public abstract AlgorithmType[] IDs { get; }

        public abstract string MinerUUID { get; }
        public abstract bool IsDual { get; }
        #endregion


        #region Mining settings

        /// <summary>
        /// Hashrate in H/s set by benchmark or user
        /// </summary>
        public abstract double BenchmarkSpeed { get; set; }
        public abstract double SecondaryBenchmarkSpeed { get; set; }

        public abstract List<double> Speeds { get; set; }

        /// <summary>
        /// Gets the averaged speed for this algorithm in H/s
        /// <para>When multiple devices of the same model are used, this will be set to their averaged hashrate</para>
        /// </summary>
        public double AvaragedSpeed { get; set; }

        public double SecondaryAveragedSpeed { get; set; }

        /// <summary>
        /// String containing raw extralaunchparams entered by user
        /// </summary>
        public virtual string ExtraLaunchParameters { get; set; }

        /// <summary>
        /// Get or set whether this algorithm is enabled for mining
        /// </summary>
        public virtual bool Enabled { get; set; }
        
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
                else if(i == 1)
                {
                    SecondaryCurNhmSmaDataVal = paying;
                    newProfit += SecondaryCurNhmSmaDataVal * SecondaryAveragedSpeed * Mult;
                }
            }
            CurrentProfit = newProfit;
            //profits.TryGetValue(NiceHashID, out var paying);
            //CurNhmSmaDataVal = paying;
            //CurrentProfit = CurNhmSmaDataVal * AvaragedSpeed * Mult;
            SubtractPowerFromProfit();
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
