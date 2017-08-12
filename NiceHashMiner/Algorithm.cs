using Newtonsoft.Json;
using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using NiceHashMiner.Configs;

namespace NiceHashMiner {
    public class Algorithm {

        public readonly string AlgorithmName;
        public readonly string MinerBaseTypeName;
        public readonly AlgorithmType NiceHashID;
        public readonly AlgorithmType SecondaryNiceHashID;
        public readonly MinerBaseType MinerBaseType;
        public readonly string AlgorithmStringID;
        // ClaymoreDual intensity tuning
        public Dictionary<int, double> IntensitySpeeds;
        public Dictionary<int, double> SecondaryIntensitySpeeds;
        public bool TuningEnabled;
        public int TuningStart = 25;
        public int TuningEnd = 200;
        public int TuningInterval = 25;
        // Miner name is used for miner ALGO flag parameter
        public readonly string MinerName;
        private double benchmarkSpeed;
        public double BenchmarkSpeed {
            get {
                if (MostProfitableIntensity > 0) {
                    try {
                        return IntensitySpeeds[MostProfitableIntensity];
                    } catch (Exception e) {
                        Helpers.ConsolePrint("CDTUNING", e.ToString());
                        IntensityUpToDate = false;
                        return 0;
                    }
                }
                return benchmarkSpeed;
            }
            set {
                benchmarkSpeed = value;
            }
        }
        private double secondaryBenchmarkSpeed;
        public double SecondaryBenchmarkSpeed {
            get {
                if (MostProfitableIntensity > 0) {
                    try {
                        return SecondaryIntensitySpeeds[MostProfitableIntensity];
                    } catch (Exception e) {
                        Helpers.ConsolePrint("CDTUNING", e.ToString());
                        IntensityUpToDate = false;
                        return 0;
                    }
                }
                return secondaryBenchmarkSpeed;
            }
            set {
                secondaryBenchmarkSpeed = value;
            }
        }
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
        public double CurNhmSMADataVal = 0;
        public double SecondaryCurNhmSMADataVal = 0;
        public int CurrentIntensity = -1;
        // This should ideally be false when SMA or speeds are updated and no check has been done yet
        public bool IntensityUpToDate = false;
        private int mostProfitableIntensity = -1;
        public int MostProfitableIntensity {
            get {
                if (!IntensityUpToDate) UpdateProfitableIntensity();
                return mostProfitableIntensity;
            }
        }
        public List<int> Intensities {
            get {
                var list = new List<int>(IntensitySpeeds.Keys);
                foreach (var key in SecondaryIntensitySpeeds.Keys) {
                    if (!list.Contains(key)) {
                        list.Add(key);
                    }
                }
                return list;
            }
        }
        public bool BenchmarkNeeded {
            get {
                if (TuningEnabled) {
                    for (var i = ConfigManager.GeneralConfig.CDIntensityTuningStart;
                    i <= ConfigManager.GeneralConfig.CDIntensityTuningEnd;
                    i += ConfigManager.GeneralConfig.CDIntensityTuningInterval) {
                        if (isIntensityEmpty(i)) return true;
                    }
                } else {
                    if ((IsDual() && SecondaryBenchmarkSpeed <= 0) || BenchmarkSpeed <= 0) {
                        return true;
                    }
                }
                return false;
            }
        }

        public Algorithm(MinerBaseType minerBaseType, AlgorithmType niceHashID, string minerName, AlgorithmType secondaryNiceHashID=AlgorithmType.NONE) {
            NiceHashID = niceHashID;
            SecondaryNiceHashID = secondaryNiceHashID;

            this.AlgorithmName = AlgorithmNiceHashNames.GetName(DualNiceHashID());
            this.MinerBaseTypeName = Enum.GetName(typeof(MinerBaseType), minerBaseType);
            this.AlgorithmStringID = this.MinerBaseTypeName + "_" + this.AlgorithmName;

            MinerBaseType = minerBaseType;
            MinerName = minerName;

            BenchmarkSpeed = 0.0d;
            SecondaryBenchmarkSpeed = 0.0d;
            ExtraLaunchParameters = "";
            LessThreads = 0;
            Enabled = !(NiceHashID == AlgorithmType.Nist5);
            BenchmarkStatus = "";

            IntensitySpeeds = new Dictionary<int, double> { };
            SecondaryIntensitySpeeds = new Dictionary<int, double> { };
        }

        // benchmark info
        public string BenchmarkStatus { get; set; }
        public bool IsBenchmarkPending { get; private set; }
        public string CurPayingRatio {
            get {
                string ratio = International.GetText("BenchmarkRatioRateN_A");
                if (Globals.NiceHashData != null) {
                    ratio = Globals.NiceHashData[NiceHashID].paying.ToString("F8");
                }
                if (SecondaryNiceHashID != AlgorithmType.NONE) {
                    ratio += "/" + Globals.NiceHashData[SecondaryNiceHashID].paying.ToString("F8");
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
            return (MinerBaseType == MinerBaseType.Claymore) && (AlgorithmType.DaggerSia <= DualNiceHashID() && DualNiceHashID() <= AlgorithmType.DaggerPascal);
        }

        public void SetIntensitySpeedsForCurrent(double speed, double secondarySpeed) {
            IntensitySpeeds[CurrentIntensity] = speed;
            SecondaryIntensitySpeeds[CurrentIntensity] = secondarySpeed;
            Helpers.ConsolePrint("CDTUNING", String.Format("Speeds set for intensity {0}: {1} / {2}", CurrentIntensity, speed, secondarySpeed));
            IntensityUpToDate = false;
        }

        public void UpdateProfitableIntensity() {
            var NiceHashSMA = Globals.NiceHashData;
            if (!IsDual() || NiceHashSMA == null) {
                mostProfitableIntensity = -1;
                return;
            }

            var maxProfit = 0d;
            var intensity = -1;
            var paying = NiceHashSMA[NiceHashID].paying;
            var secondaryPaying = NiceHashSMA[SecondaryNiceHashID].paying;
            foreach (var key in IntensitySpeeds.Keys) {
                var profit = IntensitySpeeds[key] * paying;
                if (SecondaryIntensitySpeeds.ContainsKey(key)) {
                    profit += SecondaryIntensitySpeeds[key] * secondaryPaying;
                }
                if (profit > maxProfit) {
                    maxProfit = profit;
                    intensity = key;
                }
            }
            mostProfitableIntensity = intensity;
            IntensityUpToDate = true;
        }

        private bool isIntensityEmpty(int i) {
            if (!IntensitySpeeds.ContainsKey(i) || !SecondaryIntensitySpeeds.ContainsKey(i)) return true;
            if (IntensitySpeeds[i] <= 0 || SecondaryIntensitySpeeds[i] <= 0) return true;
            return false;
        }

        public bool IncrementToNextEmptyIntensity() {  // Return false if no more needed increment
            if (!TuningEnabled) return false;
            for (var i = Math.Max(CurrentIntensity, ConfigManager.GeneralConfig.CDIntensityTuningStart);
                i <= ConfigManager.GeneralConfig.CDIntensityTuningEnd;
                i += ConfigManager.GeneralConfig.CDIntensityTuningInterval) {
                if (isIntensityEmpty(i)) {
                    CurrentIntensity = i;
                    return true;
                }
            }
            return false;
        }

        public bool StartTuning() {  // Return false if no benchmark needed
            CurrentIntensity = ConfigManager.GeneralConfig.CDIntensityTuningStart;
            return IncrementToNextEmptyIntensity();
        }

        public double ProfitForIntensity(int intensity) {
            if (Globals.NiceHashData == null) return 0;
            var profit = 0d;
            if (Globals.NiceHashData.ContainsKey(NiceHashID) && IntensitySpeeds.ContainsKey(intensity)) {
                profit += IntensitySpeeds[intensity] * Globals.NiceHashData[NiceHashID].paying * 0.000000001;
            }
            if (Globals.NiceHashData.ContainsKey(SecondaryNiceHashID) && SecondaryIntensitySpeeds.ContainsKey(intensity)) {
                profit += SecondaryIntensitySpeeds[intensity] * Globals.NiceHashData[SecondaryNiceHashID].paying * 0.000000001;
            }
            return profit;
        }
    }
}
