using System;
using System.Collections.Generic;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Algorithms
{
    public class Algorithm
    {
        /// <summary>
        /// Used for converting SMA values to BTC/H/Day
        /// </summary>
        protected const double Mult = 0.000000001;

        #region Identity

        /// <summary>
        /// Friendly display name for this algorithm
        /// </summary>
        public string AlgorithmName { get; protected set; }
        /// <summary>
        /// Friendly name for miner type
        /// </summary>
        public readonly string MinerBaseTypeName;
        /// <summary>
        /// Friendly name for this algorithm/miner combo
        /// </summary>
        public string AlgorithmStringID { get; protected set; }
        /// <summary>
        /// AlgorithmType used by this Algorithm
        /// </summary>
        public readonly AlgorithmType NiceHashID;
        /// <summary>
        /// MinerBaseType used by this algorithm
        /// </summary>
        public readonly MinerBaseType MinerBaseType;
        /// <summary>
        /// Used for miner ALGO flag parameter
        /// </summary>
        public readonly string MinerName;

        #endregion

        #region Mining settings

        /// <summary>
        /// Hashrate in H/s set by benchmark or user
        /// </summary>
        public virtual double BenchmarkSpeed { get; set; }
        /// <summary>
        /// Gets the averaged speed for this algorithm in H/s
        /// <para>When multiple devices of the same model are used, this will be set to their averaged hashrate</para>
        /// </summary>
        public double AvaragedSpeed { get; set; }

        /// <summary>
        /// String containing raw extralaunchparams entered by user
        /// </summary>
        public string ExtraLaunchParameters { get; set; }

        /// <summary>
        /// Get or set whether this algorithm is enabled for mining
        /// </summary>
        public bool Enabled { get; set; }

        // TODO not needed with new xmr-stak?
        public int LessThreads { get; set; }

        /// <summary>
        /// Path to the miner executable
        /// <para>Path may differ for the same miner/algo combos depending on devices and user settings</para>
        /// </summary>
        public string MinerBinaryPath = "";
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
        
        /// <summary>
        /// Power consumption of this algorithm, in Watts
        /// </summary>
        public virtual double PowerUsage { get; set; }

        #endregion

        #region Dual stubs
        
        // Useful placeholders for finding/sorting
        public virtual AlgorithmType SecondaryNiceHashID => AlgorithmType.NONE;
        public virtual AlgorithmType DualNiceHashID => NiceHashID;

        #endregion

        public Algorithm(MinerBaseType minerBaseType, AlgorithmType niceHashID, string minerName) 
        {
            NiceHashID = niceHashID;

            AlgorithmName = AlgorithmNiceHashNames.GetName(NiceHashID);
            MinerBaseTypeName = Enum.GetName(typeof(MinerBaseType), minerBaseType);
            AlgorithmStringID = MinerBaseTypeName + "_" + AlgorithmName;

            MinerBaseType = minerBaseType;
            MinerName = minerName;
            
            ExtraLaunchParameters = "";
            LessThreads = 0;
            Enabled = !(NiceHashID == AlgorithmType.Nist5 ||
                        (NiceHashID == AlgorithmType.NeoScrypt && minerBaseType == MinerBaseType.sgminer));
            BenchmarkStatus = "";
        }

        #region Benchmark info

        public string BenchmarkStatus { get; set; }

        public bool IsBenchmarkPending { get; private set; }

        public string CurPayingRatio
        {
            get
            {
                var ratio = International.GetText("BenchmarkRatioRateN_A");
                if (NHSmaData.TryGetPaying(NiceHashID, out var paying))
                {
                    ratio = paying.ToString("F8");
                }
                return ratio;
            }
        }

        public virtual string CurPayingRate
        {
            get
            {
                var rate = International.GetText("BenchmarkRatioRateN_A");
                if (BenchmarkSpeed > 0 && NHSmaData.TryGetPaying(NiceHashID, out var paying))
                {
                    var payingRate = BenchmarkSpeed * paying * Mult;
                    rate = payingRate.ToString("F8");
                }
                return rate;
            }
        }

        #endregion

        #region Benchmark methods

        public void SetBenchmarkPending()
        {
            IsBenchmarkPending = true;
            BenchmarkStatus = International.GetText("Algorithm_Waiting_Benchmark");
        }

        public void SetBenchmarkPendingNoMsg()
        {
            IsBenchmarkPending = true;
        }

        private bool IsPendingString()
        {
            return BenchmarkStatus == International.GetText("Algorithm_Waiting_Benchmark")
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
                return Helpers.FormatDualSpeedOutput(BenchmarkSpeed, 0, NiceHashID);
            }
            if (!IsPendingString() && !string.IsNullOrEmpty(BenchmarkStatus))
            {
                return BenchmarkStatus;
            }
            return International.GetText("BenchmarkSpeedStringNone");
        }

        #endregion
        
        #region Profitability methods

        public virtual void UpdateCurProfit(Dictionary<AlgorithmType, double> profits)
        {
            profits.TryGetValue(NiceHashID, out var paying);
            CurNhmSmaDataVal = paying;
            CurrentProfit = CurNhmSmaDataVal * AvaragedSpeed * Mult;
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
