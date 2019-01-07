using System;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Configs.Data
{
    [Serializable]
    public class AlgorithmConfig
    {
        public string Name = ""; // Used as an indicator for easier user interaction
        public AlgorithmType NiceHashID = AlgorithmType.NONE;
        public AlgorithmType SecondaryNiceHashID = AlgorithmType.NONE;
        public MinerBaseType MinerBaseType = MinerBaseType.NONE;
        public string MinerName = ""; // probably not needed
        public double BenchmarkSpeed = 0;
        public double SecondaryBenchmarkSpeed = 0;
        public string ExtraLaunchParameters = "";
        public bool Enabled = true;
        public int LessThreads = 0;
        public double PowerUsage = 0;
    }
}
