using Newtonsoft.Json;
using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NiceHashMiner.Configs;

namespace NiceHashMiner {
    public class Algorithm {

        public string AlgorithmName { get; protected set; }
        public readonly string MinerBaseTypeName;
        public readonly AlgorithmType NiceHashID;
        public readonly MinerBaseType MinerBaseType;
        public string AlgorithmStringID { get; protected set; }
        // Miner name is used for miner ALGO flag parameter
        public readonly string MinerName;
        protected double benchmarkSpeed;
        public virtual double BenchmarkSpeed {
            get {
                return benchmarkSpeed;
            }
            set {
                benchmarkSpeed = value;
            }
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
        public double CurNhmSMADataVal = 0;
        public double SecondaryCurNhmSMADataVal = 0;
        // This should ideally be false when SMA or speeds are updated and no check has been done yet
        public bool IntensityUpToDate = false;
        public virtual bool BenchmarkNeeded {
            get {
                return BenchmarkSpeed <= 0;
            }
        }

        public Algorithm(MinerBaseType minerBaseType, AlgorithmType niceHashID, string minerName) {
            NiceHashID = niceHashID;

            this.AlgorithmName = AlgorithmNiceHashNames.GetName(DualNiceHashID);
            this.MinerBaseTypeName = Enum.GetName(typeof(MinerBaseType), minerBaseType);
            this.AlgorithmStringID = this.MinerBaseTypeName + "_" + this.AlgorithmName;

            MinerBaseType = minerBaseType;
            MinerName = minerName;

            BenchmarkSpeed = 0.0d;
            ExtraLaunchParameters = "";
            LessThreads = 0;
            Enabled = !(NiceHashID == AlgorithmType.Nist5);
            BenchmarkStatus = "";
        }

        // benchmark info
        public string BenchmarkStatus { get; set; }
        public bool IsBenchmarkPending { get; private set; }
        public virtual string CurPayingRatio {
            get {
                string ratio = International.GetText("BenchmarkRatioRateN_A");
                if (Globals.NiceHashData != null) {
                    ratio = Globals.NiceHashData[NiceHashID].paying.ToString("F8");
                }
                return ratio;
            }
        }
        public virtual string CurPayingRate {
            get {
                string rate = International.GetText("BenchmarkRatioRateN_A");
                var payingRate = 0.0d;
                if (Globals.NiceHashData != null) {
                    if (BenchmarkSpeed > 0) {
                        payingRate += BenchmarkSpeed * Globals.NiceHashData[NiceHashID].paying * 0.000000001;
                    }
                    rate = payingRate.ToString("F8");
                }
                return rate;
            }
        }

        public void SetBenchmarkPending() {
            IsBenchmarkPending = true;
            BenchmarkStatus = International.GetText("Algorithm_Waiting_Benchmark");
        }
        public void SetBenchmarkPendingNoMsg() {
            IsBenchmarkPending = true;
        }

        protected bool IsPendingString() {
            return BenchmarkStatus == International.GetText("Algorithm_Waiting_Benchmark")
                || BenchmarkStatus.Contains(".");  // Workaround to allow dcri display
        }

        public void ClearBenchmarkPending() {
            IsBenchmarkPending = false;
            if (IsPendingString()) {
                BenchmarkStatus = "";
            }
        }

        public void ClearBenchmarkPendingFirst() {
            IsBenchmarkPending = false;
            BenchmarkStatus = "";
        }

        public virtual string BenchmarkSpeedString() {
            if (Enabled && IsBenchmarkPending && !string.IsNullOrEmpty(BenchmarkStatus)) {
                return BenchmarkStatus;
            } else if (BenchmarkSpeed > 0) {
                return Helpers.FormatDualSpeedOutput(BenchmarkSpeed);
            } else if (!IsPendingString() && !string.IsNullOrEmpty(BenchmarkStatus)) {
                return BenchmarkStatus;
            }
            return International.GetText("BenchmarkSpeedStringNone");
        }
        
        public virtual AlgorithmType DualNiceHashID {
            get {
                return NiceHashID;
            }
        }
    }
}
