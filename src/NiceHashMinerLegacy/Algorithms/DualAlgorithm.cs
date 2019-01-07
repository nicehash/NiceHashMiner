using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Algorithms
{
    public class DualAlgorithm : Algorithm
    {
        #region Identity

        /// <summary>
        /// AlgorithmType used as the secondary for this algorithm
        /// </summary>
        public override AlgorithmType SecondaryNiceHashID { get; }
        /// <summary>
        /// Friendly name for secondary algorithm
        /// </summary>
        public readonly string SecondaryAlgorithmName;
        /// <summary>
        /// Current SMA profitability for the secondary algorithm type in BTC/GH/Day
        /// </summary>
        public double SecondaryCurNhmSmaDataVal = 0;
        /// <summary>
        /// AlgorithmType that uniquely identifies this choice of primary/secondary types
        /// </summary>
        public override AlgorithmType DualNiceHashID => Helpers.DualAlgoFromAlgos(NiceHashID, SecondaryNiceHashID);

        #endregion

        #region Intensity tuning

        /// <summary>
        /// Current intensity while mining or benchmarking
        /// </summary>
        public int CurrentIntensity = -1;

        /// <summary>
        /// Lower bound for intensity tuning
        /// </summary>
        public int TuningStart = 25;
        /// <summary>
        /// Upper bound for intensity tuning
        /// </summary>
        public int TuningEnd = 200;
        /// <summary>
        /// Interval for intensity tuning
        /// </summary>
        public int TuningInterval = 25;

        /// <summary>
        /// Dictionary of intensity values to speeds in hashrates
        /// </summary>
        public Dictionary<int, double> IntensitySpeeds;
        /// <summary>
        /// Dictionary of intensity values to secondary speeds in hashrates
        /// </summary>
        public Dictionary<int, double> SecondaryIntensitySpeeds;
        /// <summary>
        /// Get or set whether intensity tuning is enabled
        /// </summary>
        public bool TuningEnabled;

        // And backups
        private Dictionary<int, double> _intensitySpeedsBack;
        private Dictionary<int, double> _secondaryIntensitySpeedsBack;
        private bool _tuningEnabledBack;
        private int _tuningStartBack;
        private int _tuningEndBack;
        private int _tuningIntervalBack;

        /// <summary>
        /// Get or set whether intensity profitability is up to date
        /// <para>This should generally be set to false if tuning speeds or SMA profits are changed</para>
        /// </summary>
        public bool IntensityUpToDate;

        private int _mostProfitableIntensity = -1;
        /// <summary>
        /// Get the most profitable intensity value for this algorithm
        /// <para>If IntensityUpToDate = false, intensity profit will be updated first</para>
        /// </summary>
        public int MostProfitableIntensity
        {
            get
            {
                // UpdateProfitableIntensity() can take some time, so we store most profitable and only update when needed
                if (!IntensityUpToDate) UpdateProfitableIntensity();
                return _mostProfitableIntensity;
            }
        }

        /// <summary>
        /// Sorted list of intensities that are selected for tuning
        /// </summary>
        private SortedSet<int> SelectedIntensities
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

        /// <summary>
        /// Sorted list of all intensities that are selected for tuning or have speeds
        /// </summary>
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

        #endregion

        #region Mining settings
        
        /// <summary>
        /// Primary hashrate in H/s set by benchmark or user
        /// <para>If tuning is enabled, returns the hashrate from the most profitable intensity</para>
        /// </summary>
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

                return base.BenchmarkSpeed;
            }
        }

        private double _secondaryBenchmarkSpeed;
        /// <summary>
        /// Secondary hashrate in H/s set by benchmark or user
        /// <para>If tuning is enabled, returns the hashrate from the most profitable intensity</para>
        /// </summary>
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
        
        /// <summary>
        /// Gets the secondary averaged speed for this algorithm in H/s
        /// <para>When multiple devices of the same model are used, this will be set to their averaged hashrate</para>
        /// </summary>
        public double SecondaryAveragedSpeed { get; set; }
        
        /// <summary>
        /// Indicates whether this algorithm requires a benchmark
        /// </summary>
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

        #endregion
        
        #region Power Switching

        /// <summary>
        /// Dictionary of intensity values and power usage for each
        /// </summary>
        public Dictionary<int, double> IntensityPowers;
        /// <summary>
        /// Get or set whether we should use different powers for intensities
        /// </summary>
        public bool UseIntensityPowers;
        // Backup of above
        private Dictionary<int, double> _intensityPowersBack;
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

        #region Benchmark info

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

        #endregion

        public override void UpdateCurProfit(Dictionary<AlgorithmType, double> profits)
        {
            base.UpdateCurProfit(profits);
            profits.TryGetValue(SecondaryNiceHashID, out var secPaying);
            
            SecondaryCurNhmSmaDataVal = secPaying;

            IntensityUpToDate = false;

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

        public void SetPowerForCurrent(double power)
        {
            IntensityPowers[CurrentIntensity] = power;
            IntensityUpToDate = false;
        }

        private void UpdateProfitableIntensity()
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
