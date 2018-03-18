using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Enums;
using NiceHashMiner.Switching;

namespace NiceHashMiner.Algorithms
{
    public class DualAlgorithm : Algorithm
    {
        public override AlgorithmType SecondaryNiceHashID { get; }

        public string SecondaryAlgorithmName;

        // ClaymoreDual intensity tuning
        public int CurrentIntensity = -1;

        public Dictionary<int, double> IntensitySpeeds;
        public Dictionary<int, double> SecondaryIntensitySpeeds;
        public bool TuningEnabled;
        public int TuningStart = 25;
        public int TuningEnd = 200;

        public int TuningInterval = 25;

        // And backups
        private Dictionary<int, double> _intensitySpeedsBack;
        private Dictionary<int, double> _secondaryIntensitySpeedsBack;
        private bool _tuningEnabledBack;
        private int _tuningStartBack;
        private int _tuningEndBack;
        private int _tuningIntervalBack;

        public double SecondaryCurNhmSmaDataVal = 0;
        public bool IntensityUpToDate;

        public override double BenchmarkSpeed
        {
            get
            {
                if (MostProfitableIntensity > 0)
                {
                    try
                    {
                        return IntensitySpeeds[MostProfitableIntensity];
                    }
                    catch (Exception e)
                    {
                        Helpers.ConsolePrint("CDTUNING", e.ToString());
                        IntensityUpToDate = false;
                        return 0;
                    }
                }

                return benchmarkSpeed;
            }
        }

        private double _secondaryBenchmarkSpeed;

        public double SecondaryBenchmarkSpeed
        {
            get
            {
                if (MostProfitableIntensity > 0)
                {
                    try
                    {
                        return SecondaryIntensitySpeeds[MostProfitableIntensity];
                    }
                    catch (Exception e)
                    {
                        Helpers.ConsolePrint("CDTUNING", e.ToString());
                        IntensityUpToDate = false;
                        return 0;
                    }
                }

                return _secondaryBenchmarkSpeed;
            }
            set => _secondaryBenchmarkSpeed = value;
        }

        public double SecondaryAveragedSpeed { get; set; }

        private int _mostProfitableIntensity = -1;

        public int MostProfitableIntensity
        {
            get
            {
                if (!IntensityUpToDate) UpdateProfitableIntensity();
                return _mostProfitableIntensity;
            }
        }

        public SortedSet<int> SelectedIntensities
        {
            get
            {
                var list = new SortedSet<int>();
                for (var i = TuningStart;
                    i <= TuningEnd;
                    i += TuningInterval)
                {
                    list.Add(i);
                }

                return list;
            }
        }

        public SortedSet<int> AllIntensities
        {
            get
            {
                var list = new List<int>(IntensitySpeeds.Keys);
                list.AddRange(SecondaryIntensitySpeeds.Keys);
                list.AddRange(SelectedIntensities);
                return new SortedSet<int>(list);
            }
        }

        public override bool BenchmarkNeeded
        {
            get
            {
                if (TuningEnabled)
                {
                    if (SelectedIntensities.Any(IsIntensityEmpty)) return true;
                }
                else
                {
                    if (SecondaryBenchmarkSpeed <= 0 || BenchmarkSpeed <= 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public override AlgorithmType DualNiceHashID
        {
            get
            {
                if (NiceHashID == AlgorithmType.DaggerHashimoto)
                {
                    switch (SecondaryNiceHashID)
                    {
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

        public string SecondaryCurPayingRatio
        {
            get
            {
                var ratio = International.GetText("BenchmarkRatioRateN_A");
                if (NHSmaData.TryGetPaying(SecondaryNiceHashID, out var paying))
                {
                    ratio = paying.ToString("F8");
                }

                return ratio;
            }
        }

        #region Power Switching

        public Dictionary<int, double> IntensityPowers;
        private Dictionary<int, double> _intensityPowersBack;

        public bool UseIntensityPowers;
        private bool _useIntensityPowersBack;

        public override double PowerUsage
        {
            get
            {
                if (UseIntensityPowers &&
                    MostProfitableIntensity > 0 &&
                    IntensityPowers.TryGetValue(MostProfitableIntensity, out var power))
                {
                    return power;
                }

                return base.PowerUsage;
            }
            set => base.PowerUsage = value;
        }

        #endregion


        public DualAlgorithm(MinerBaseType minerBaseType, AlgorithmType niceHashID, AlgorithmType secondaryNiceHashID)
            : base(minerBaseType, niceHashID, "")
        {
            SecondaryNiceHashID = secondaryNiceHashID;

            AlgorithmName = AlgorithmNiceHashNames.GetName(DualNiceHashID); // needed to add secondary
            SecondaryAlgorithmName = AlgorithmNiceHashNames.GetName(secondaryNiceHashID);
            AlgorithmStringID = MinerBaseTypeName + "_" + AlgorithmName;

            SecondaryBenchmarkSpeed = 0.0d;

            IntensitySpeeds = new Dictionary<int, double>();
            SecondaryIntensitySpeeds = new Dictionary<int, double>();
            IntensityPowers = new Dictionary<int, double>();
        }

        public override string CurPayingRate
        {
            get
            {
                var rate = International.GetText("BenchmarkRatioRateN_A");
                var payingRate = 0.0d;

                if (BenchmarkSpeed > 0 && NHSmaData.TryGetPaying(NiceHashID, out var paying))
                {
                    payingRate += BenchmarkSpeed * paying * Mult;
                    rate = payingRate.ToString("F8");
                }

                if (SecondaryBenchmarkSpeed > 0 && NHSmaData.TryGetPaying(SecondaryNiceHashID, out var secPaying))
                {
                    payingRate += SecondaryBenchmarkSpeed * secPaying * Mult;
                    rate = payingRate.ToString("F8");
                }

                return rate;
            }
        }

        public string SecondaryBenchmarkSpeedString()
        {
            const string dcriStatus = " (dcri:{0})";
            if (Enabled && IsBenchmarkPending && TuningEnabled && !string.IsNullOrEmpty(BenchmarkStatus))
            {
                return CurrentIntensity >= 0 ? string.Format(dcriStatus, CurrentIntensity) : BenchmarkSpeedString();
            }

            if (SecondaryBenchmarkSpeed > 0)
            {
                return Helpers.FormatDualSpeedOutput(SecondaryBenchmarkSpeed)
                       + ((TuningEnabled) ? string.Format(dcriStatus, MostProfitableIntensity) : "");
            }

            return International.GetText("BenchmarkSpeedStringNone");
        }

        public override void UpdateCurProfit(Dictionary<AlgorithmType, double> profits)
        {
            base.UpdateCurProfit(profits);
            profits.TryGetValue(SecondaryNiceHashID, out var secPaying);
            
            SecondaryCurNhmSmaDataVal = secPaying;

            CurrentProfit = (CurNhmSmaDataVal * BenchmarkSpeed + SecondaryCurNhmSmaDataVal * SecondaryBenchmarkSpeed) * Mult;

            SubtractPowerFromProfit();
        }

        #region ClaymoreDual Tuning

        public void SetIntensitySpeedsForCurrent(double speed, double secondarySpeed)
        {
            IntensitySpeeds[CurrentIntensity] = speed;
            SecondaryIntensitySpeeds[CurrentIntensity] = secondarySpeed;
            Helpers.ConsolePrint("CDTUNING", $"Speeds set for intensity {CurrentIntensity}: {speed} / {secondarySpeed}");
            IntensityUpToDate = false;
        }

        public void UpdateProfitableIntensity()
        {
            if (!NHSmaData.HasData)
            {
                _mostProfitableIntensity = -1;
                IntensityUpToDate = true;
                return;
            }

            var maxProfit = 0d;
            var intensity = -1;
            // Max sure to use single | here so second expression evaluates
            if (NHSmaData.TryGetPaying(NiceHashID, out var paying) |
                NHSmaData.TryGetPaying(SecondaryNiceHashID, out var secPaying))
            {
                foreach (var key in IntensitySpeeds.Keys)
                {
                    var profit = IntensitySpeeds[key] * paying;
                    if (SecondaryIntensitySpeeds.TryGetValue(key, out var speed))
                    {
                        profit += speed * secPaying;
                    }

                    if (profit > maxProfit)
                    {
                        maxProfit = profit;
                        intensity = key;
                    }
                }
            }

            _mostProfitableIntensity = intensity;
            IntensityUpToDate = true;
        }

        private bool IsIntensityEmpty(int i)
        {
            if (!IntensitySpeeds.ContainsKey(i) || !SecondaryIntensitySpeeds.ContainsKey(i)) return true;
            return IntensitySpeeds[i] <= 0 || SecondaryIntensitySpeeds[i] <= 0;
        }

        public bool IncrementToNextEmptyIntensity()
        {
            // Return false if no more needed increment
            if (!TuningEnabled) return false;
            CurrentIntensity = SelectedIntensities.FirstOrDefault(IsIntensityEmpty);
            return CurrentIntensity > 0;
        }

        public bool StartTuning()
        {
            // Return false if no benchmark needed
            CurrentIntensity = TuningStart;
            return IncrementToNextEmptyIntensity();
        }

        public override void ClearBenchmarkPendingFirst()
        {
            base.ClearBenchmarkPendingFirst();
            CurrentIntensity = -1;
        }

        public double ProfitForIntensity(int intensity)
        {
            var profit = 0d;
            if (NHSmaData.TryGetPaying(NiceHashID, out var paying) && 
                IntensitySpeeds.TryGetValue(intensity, out var speed))
            {
                profit += speed * paying * Mult;
            }
            
            if (NHSmaData.TryGetPaying(SecondaryNiceHashID, out var secPaying) && 
                SecondaryIntensitySpeeds.TryGetValue(intensity, out var secSpeed))
            {
                profit += secSpeed * secPaying * Mult;
            }

            return profit;
        }

        public double SpeedForIntensity(int intensity)
        {
            IntensitySpeeds.TryGetValue(intensity, out var speed);
            return speed;
        }

        public double SecondarySpeedForIntensity(int intensity)
        {
            SecondaryIntensitySpeeds.TryGetValue(intensity, out var speed);
            return speed;
        }

        public string SpeedStringForIntensity(int intensity)
        {
            var speed = SpeedForIntensity(intensity);
            if (speed > 0) return Helpers.FormatSpeedOutput(speed) + "H/s";
            return International.GetText("BenchmarkSpeedStringNone");
        }

        public string SecondarySpeedStringForIntensity(int intensity)
        {
            var speed = SecondarySpeedForIntensity(intensity);
            if (speed > 0) return Helpers.FormatSpeedOutput(speed) + "H/s";
            return International.GetText("BenchmarkSpeedStringNone");
        }

        public void MakeIntensityBackup()
        {
            _intensitySpeedsBack = new Dictionary<int, double>(IntensitySpeeds);
            _secondaryIntensitySpeedsBack = new Dictionary<int, double>(SecondaryIntensitySpeeds);
            _tuningEnabledBack = TuningEnabled;
            _tuningStartBack = TuningStart;
            _tuningEndBack = TuningEnd;
            _tuningIntervalBack = TuningInterval;
            _intensityPowersBack = new Dictionary<int, double>(IntensityPowers);
            _useIntensityPowersBack = UseIntensityPowers;
        }

        public void RestoreIntensityBackup()
        {
            IntensitySpeeds = new Dictionary<int, double>(_intensitySpeedsBack);
            SecondaryIntensitySpeeds = new Dictionary<int, double>(_secondaryIntensitySpeedsBack);
            TuningEnabled = _tuningEnabledBack;
            TuningStart = _tuningStartBack;
            TuningEnd = _tuningEndBack;
            TuningInterval = _tuningIntervalBack;
            IntensityPowers = new Dictionary<int, double>(_intensityPowersBack);
            UseIntensityPowers = _useIntensityPowersBack;
        }

        #endregion
    }
}
