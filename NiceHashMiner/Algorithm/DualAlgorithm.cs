using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Enums;

namespace NiceHashMiner
{
    public class DualAlgorithm : Algorithm {
        public override AlgorithmType SecondaryNiceHashID { get; }

        public string SecondaryAlgorithmName = "";
        // ClaymoreDual intensity tuning
        public int CurrentIntensity = -1;

        public Dictionary<int, double> IntensitySpeeds;
        public Dictionary<int, double> SecondaryIntensitySpeeds;
        public bool TuningEnabled;
        public int TuningStart = 25;
        public int TuningEnd = 200;
        public int TuningInterval = 25;
        // And backups
        private Dictionary<int, double> intensitySpeedsBack;
        private Dictionary<int, double> secondaryIntensitySpeedsBack;
        private bool tuningEnabledBack;
        private int tuningStartBack;
        private int tuningEndBack;
        private int tuningIntervalBack;

        public double SecondaryCurNhmSMADataVal = 0;
        public bool IntensityUpToDate = false;

        public override double BenchmarkSpeed {
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
            set => secondaryBenchmarkSpeed = value;
        }
        public double SecondaryAveragedSpeed { get; set; }

        private int mostProfitableIntensity = -1;
        public int MostProfitableIntensity {
            get {
                if (!IntensityUpToDate) UpdateProfitableIntensity();
                return mostProfitableIntensity;
            }
        }
        public SortedSet<int> SelectedIntensities {
            get {
                var list = new SortedSet<int>();
                for (var i = TuningStart;
                    i <= TuningEnd;
                    i += TuningInterval) {
                    list.Add(i);
                }
                return list;
            }
        }
        public SortedSet<int> AllIntensities {
            get {
                var list = new List<int>(IntensitySpeeds.Keys);
                list.AddRange(SecondaryIntensitySpeeds.Keys);
                list.AddRange(SelectedIntensities);
                return new SortedSet<int>(list);
            }
        }

        public override bool BenchmarkNeeded {
            get {
                if (TuningEnabled) {
                    if (SelectedIntensities.Any(isIntensityEmpty)) return true;
                } else {
                    if (SecondaryBenchmarkSpeed <= 0 || BenchmarkSpeed <= 0) {
                        return true;
                    }
                }
                return false;
            }
        }

        public override AlgorithmType DualNiceHashID {
            get {
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
        }
        public override bool IsDual => true;

        public string SecondaryCurPayingRatio {
            get {
                string ratio = International.GetText("BenchmarkRatioRateN_A");
                if (Globals.NiceHashData != null && Globals.NiceHashData.ContainsKey(SecondaryNiceHashID)) {
                    ratio = Globals.NiceHashData[SecondaryNiceHashID].paying.ToString("F8");
                }
                return ratio;
            }
        }


        public DualAlgorithm(MinerBaseType minerBaseType, AlgorithmType niceHashID, AlgorithmType secondaryNiceHashID) 
            : base(minerBaseType, niceHashID, "") {
            SecondaryNiceHashID = secondaryNiceHashID;

            AlgorithmName = AlgorithmNiceHashNames.GetName(DualNiceHashID);  // needed to add secondary
            SecondaryAlgorithmName = AlgorithmNiceHashNames.GetName(secondaryNiceHashID);
            AlgorithmStringID = MinerBaseTypeName + "_" + AlgorithmName;

            SecondaryBenchmarkSpeed = 0.0d;
            
            IntensitySpeeds = new Dictionary<int, double> { };
            SecondaryIntensitySpeeds = new Dictionary<int, double> { };
        }
        
        public override string CurPayingRate {
            get {
                string rate = International.GetText("BenchmarkRatioRateN_A");
                var payingRate = 0.0d;
                if (Globals.NiceHashData != null) {
                    if (BenchmarkSpeed > 0) {
                        payingRate += BenchmarkSpeed * Globals.NiceHashData[NiceHashID].paying * 0.000000001;
                    }
                    if (SecondaryBenchmarkSpeed > 0) {
                        payingRate += SecondaryBenchmarkSpeed * Globals.NiceHashData[SecondaryNiceHashID].paying * 0.000000001;
                    }
                    rate = payingRate.ToString("F8");
                }
                return rate;
            }
        }

        public string SecondaryBenchmarkSpeedString() {
            const string dcriStatus = " (dcri:{0})";
            if (Enabled && IsBenchmarkPending && TuningEnabled && !string.IsNullOrEmpty(BenchmarkStatus)) {
                return CurrentIntensity >= 0 ? String.Format(dcriStatus, CurrentIntensity) : BenchmarkSpeedString();
            }
            if (SecondaryBenchmarkSpeed > 0) {
                return Helpers.FormatDualSpeedOutput(SecondaryBenchmarkSpeed) 
                    + ((TuningEnabled) ? String.Format(dcriStatus, MostProfitableIntensity) : "");
            }
            return International.GetText("BenchmarkSpeedStringNone");
        }

        #region ClaymoreDual Tuning

        public void SetIntensitySpeedsForCurrent(double speed, double secondarySpeed) {
            IntensitySpeeds[CurrentIntensity] = speed;
            SecondaryIntensitySpeeds[CurrentIntensity] = secondarySpeed;
            Helpers.ConsolePrint("CDTUNING", String.Format("Speeds set for intensity {0}: {1} / {2}", CurrentIntensity, speed, secondarySpeed));
            IntensityUpToDate = false;
        }

        public void UpdateProfitableIntensity() {
            var NiceHashSMA = Globals.NiceHashData;
            if (NiceHashSMA == null) {
                mostProfitableIntensity = -1;
                IntensityUpToDate = true;
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
            return IntensitySpeeds[i] <= 0 || SecondaryIntensitySpeeds[i] <= 0;
        }

        public bool IncrementToNextEmptyIntensity() {  // Return false if no more needed increment
            if (!TuningEnabled) return false;
            CurrentIntensity = SelectedIntensities.FirstOrDefault(isIntensityEmpty);
            return CurrentIntensity > 0;
        }

        public bool StartTuning() {  // Return false if no benchmark needed
            CurrentIntensity = TuningStart;
            return IncrementToNextEmptyIntensity();
        }
        
        public override void ClearBenchmarkPendingFirst() {
            base.ClearBenchmarkPendingFirst();
            CurrentIntensity = -1;
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

        public double SpeedForIntensity(int intensity) {
            IntensitySpeeds.TryGetValue(intensity, out var speed);
            return speed;
        }

        public double SecondarySpeedForIntensity(int intensity) {
            SecondaryIntensitySpeeds.TryGetValue(intensity, out var speed);
            return speed;
        }

        public string SpeedStringForIntensity(int intensity) {
            double speed = SpeedForIntensity(intensity);
            if (speed > 0) return Helpers.FormatSpeedOutput(speed) + "H/s";
            return International.GetText("BenchmarkSpeedStringNone");
        }

        public string SecondarySpeedStringForIntensity(int intensity) {
            double speed = SecondarySpeedForIntensity(intensity);
            if (speed > 0) return Helpers.FormatSpeedOutput(speed) + "H/s";
            return International.GetText("BenchmarkSpeedStringNone");
        }

        public void MakeIntensityBackup() {
            intensitySpeedsBack = new Dictionary<int, double>(IntensitySpeeds);
            secondaryIntensitySpeedsBack = new Dictionary<int, double>(SecondaryIntensitySpeeds);
            tuningEnabledBack = TuningEnabled;
            tuningStartBack = TuningStart;
            tuningEndBack = TuningEnd;
            tuningIntervalBack = TuningInterval;
        }

        public void RestoreIntensityBackup() {
            IntensitySpeeds = new Dictionary<int, double>(intensitySpeedsBack);
            SecondaryIntensitySpeeds = new Dictionary<int, double>(secondaryIntensitySpeedsBack);
            TuningEnabled = tuningEnabledBack;
            TuningStart = tuningStartBack;
            TuningEnd = tuningEndBack;
            TuningInterval = tuningIntervalBack;
        }

        #endregion
    }
}
