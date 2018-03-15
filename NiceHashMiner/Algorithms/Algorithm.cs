using System;
using NiceHashMiner.Enums;
using NiceHashMiner.Switching;

namespace NiceHashMiner.Algorithms
{
    public class Algorithm
    {
        private const double Mult = 0.000000001;

        public string AlgorithmName { get; protected set; }
        public readonly string MinerBaseTypeName;
        public readonly AlgorithmType NiceHashID;
        // Useful placeholder for sorting/finding
        public virtual AlgorithmType SecondaryNiceHashID => AlgorithmType.NONE;

        public readonly MinerBaseType MinerBaseType;
        public string AlgorithmStringID { get; protected set; }

        // Miner name is used for miner ALGO flag parameter
        public string MinerName;
        protected double benchmarkSpeed;
        public virtual double BenchmarkSpeed 
        {
            get => benchmarkSpeed;
            set => benchmarkSpeed = value;
        }
        public string ExtraLaunchParameters { get; set; }
        public bool Enabled { get; set; }

        // CPU miners only setting
        public int LessThreads { get; set; }

        // avarage speed of same devices to increase mining stability
        public double AvaragedSpeed { get; set; }

        // based on device and settings here we set the miner path
        public string MinerBinaryPath = "";

        // these are changing (logging reasons)
        public double CurrentProfit = 0;
        public double CurNhmSmaDataVal = 0;
        public virtual bool BenchmarkNeeded => BenchmarkSpeed <= 0;

        // Power switching
        /// <summary>
        /// Power consumption of this algorithm, in Watts
        /// </summary>
        public virtual double PowerUsage { get; set; }

        public Algorithm(MinerBaseType minerBaseType, AlgorithmType niceHashID, string minerName) 
        {
            NiceHashID = niceHashID;

            AlgorithmName = AlgorithmNiceHashNames.GetName(DualNiceHashID);
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

        // benchmark info
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

        public virtual void ClearBenchmarkPending()
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

        public virtual string BenchmarkSpeedString()
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
        
        public virtual AlgorithmType DualNiceHashID => NiceHashID;

        public virtual bool IsDual => false;
    }
}
