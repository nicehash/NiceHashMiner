using Newtonsoft.Json;
using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NiceHashMiner {
    public class Algorithm {

        private const double STD_PROF_MULT = 1.0;  // profit is considered deviant if it is this many std devs above average
        private const int PROF_HIST = 5;  // num of recent profits to consider for average

        public readonly string AlgorithmName;
        public readonly string MinerBaseTypeName;
        public readonly AlgorithmType NiceHashID;
        public readonly AlgorithmType SecondaryNiceHashID;
        public readonly MinerBaseType MinerBaseType;
        public readonly string AlgorithmStringID;
        // Miner name is used for miner ALGO flag parameter
        public readonly string MinerName;
        public double BenchmarkSpeed { get; set; }
        public double SecondaryBenchmarkSpeed { get; set; }
        public string ExtraLaunchParameters { get; set; }
        public bool Enabled { get; set; }

        // CPU miners only setting
        public int LessThreads { get; set; }

        // avarage speed of same devices to increase mining stability
        public double AvaragedSpeed { get; set; }
        public double SecondaryAveragedSpeed { get; set; }
        // based on device and settings here we set the miner path
        public string MinerBinaryPath = "";
        // these are changing (logging reasons)
        public double CurrentProfit = 0;
        public SMADataVal CurNhmSMADataVal;
        public SMADataVal SecondaryCurNhmSMADataVal;
        
        public Algorithm(MinerBaseType minerBaseType, AlgorithmType niceHashID, string minerName, AlgorithmType secondaryNiceHashID=AlgorithmType.NONE) {
            NiceHashID = niceHashID;
            SecondaryNiceHashID = secondaryNiceHashID;

            this.AlgorithmName = AlgorithmNiceHashNames.GetName(DualNiceHashID());
            this.MinerBaseTypeName = Enum.GetName(typeof(MinerBaseType), minerBaseType);
            this.AlgorithmStringID = this.MinerBaseTypeName + "_" + this.AlgorithmName;

            CurNhmSMADataVal = new SMADataVal(AlgorithmName);
            SecondaryCurNhmSMADataVal = new SMADataVal(AlgorithmName);

            MinerBaseType = minerBaseType;
            MinerName = minerName;

            BenchmarkSpeed = 0.0d;
            SecondaryBenchmarkSpeed = 0.0d;
            ExtraLaunchParameters = "";
            LessThreads = 0;
            Enabled = !(NiceHashID == AlgorithmType.Nist5);
            BenchmarkStatus = "";
        }

        // benchmark info
        public string BenchmarkStatus { get; set; }
        public bool IsBenchmarkPending { get; private set; }
        public string CurPayingRatio {
            get {
                string ratio = International.GetText("BenchmarkRatioRateN_A");
                if (Globals.NiceHashData != null) {
                    ratio = Globals.NiceHashData[NiceHashID].paying.ToString("F8");
                    if (IsDual() && Globals.NiceHashData.ContainsKey(SecondaryNiceHashID)) {
                        ratio += "/" + Globals.NiceHashData[SecondaryNiceHashID].paying.ToString("F8");
                    }
                }
                return ratio;
            }
        }
        public string CurPayingRate {
            get {
                string rate = International.GetText("BenchmarkRatioRateN_A");
                var payingRate = 0.0d;
                if (Globals.NiceHashData != null) {
                    if (BenchmarkSpeed > 0) {
                        payingRate += BenchmarkSpeed * Globals.NiceHashData[NiceHashID].paying * 0.000000001;
                    }
                    if (SecondaryBenchmarkSpeed > 0 && IsDual()) {
                        payingRate += SecondaryBenchmarkSpeed * Globals.NiceHashData[SecondaryNiceHashID].paying * 0.000000001;
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

        private bool IsPendingString() {
            return BenchmarkStatus == International.GetText("Algorithm_Waiting_Benchmark")
                || BenchmarkStatus == "."
                || BenchmarkStatus == ".."
                || BenchmarkStatus == "...";
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

        public string BenchmarkSpeedString() {
            if (Enabled && IsBenchmarkPending && !string.IsNullOrEmpty(BenchmarkStatus)) {
                return BenchmarkStatus;
            } else if (BenchmarkSpeed > 0) {
                return Helpers.FormatDualSpeedOutput(BenchmarkSpeed, SecondaryBenchmarkSpeed);
            } else if (!IsPendingString() && !string.IsNullOrEmpty(BenchmarkStatus)) {
                return BenchmarkStatus;
            }
            return International.GetText("BenchmarkSpeedStringNone");
        }
   
        // return hybrid type if dual, else standard ID
        public AlgorithmType DualNiceHashID() {
            if (NiceHashID == AlgorithmType.DaggerHashimoto) {
                switch (SecondaryNiceHashID) {
                    case AlgorithmType.Decred:
                        return AlgorithmType.DaggerDecred;
                    case AlgorithmType.Lbry:
                        return AlgorithmType.DaggerLbry;
                    case AlgorithmType.Pascal:
                        return AlgorithmType.DaggerPascal;
                    case AlgorithmType.Sia:
                        return AlgorithmType.DaggerSia;
                }
            }
            return NiceHashID;
        }
        public bool IsDual() {
            return (AlgorithmType.DaggerSia <= DualNiceHashID() && DualNiceHashID() <= AlgorithmType.DaggerPascal);
        }

        public class SMADataVal
        {
            public double CurrentValue {
                get {
                    return recentValues.LastOrDefault();
                }
            }
            public double NormalizedValue {
                get {
                    double avg = recentValues.Average();
                    double std = Math.Sqrt(recentValues.Average(v => Math.Pow(v - avg, 2)));

                    if (CurrentValue > (std * STD_PROF_MULT) + avg) {  // result is deviant over
                        Helpers.ConsolePrint("PROFITNORM", String.Format("Algorithm {0} profit deviant, {1} std devs over ({2} over {3}",
                            name,
                            (CurrentValue - avg) / std,
                            CurrentValue,
                            avg));
                        return (std * STD_PROF_MULT) + avg;
                    }
                    return CurrentValue;
                }
            }

            private List<Double> recentValues = new List<Double> { 0 };
            private string name = "";  // for logging

            public SMADataVal(string name) {
                this.name = name;
            }

            public void AppendSMAPaying(double paying) {
                if (recentValues.Count > PROF_HIST)
                    recentValues.RemoveAt(0);
                recentValues.Add(paying);
            }
        }
    }
}
