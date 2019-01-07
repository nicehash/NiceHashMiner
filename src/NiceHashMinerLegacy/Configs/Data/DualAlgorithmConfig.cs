using System;
using System.Collections.Generic;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Configs.Data
{
    public class DualAlgorithmConfig
    {
        public string Name = ""; // Used as an indicator for easier user interaction
        public AlgorithmType SecondaryNiceHashID = AlgorithmType.NONE;
        public Dictionary<int, double> IntensitySpeeds = new Dictionary<int, double>();
        public Dictionary<int, double> SecondaryIntensitySpeeds = new Dictionary<int, double>();
        public bool TuningEnabled = true;
        public int TuningStart = 25;
        public int TuningEnd = 250;
        public int TuningInterval = 25;
        public Dictionary<int, double> IntensityPowers = new Dictionary<int, double>();
        public bool UseIntensityPowers = false;

        public void FixSettingsBounds()
        {
            if (TuningStart < 0 || TuningStart > TuningEnd)
            {
                TuningStart = 25;
            }

            if (TuningInterval < 1)
            {
                TuningInterval = 25;
            }

            TuningEnd = Math.Max(TuningEnd, TuningStart);
        }
    }
}
