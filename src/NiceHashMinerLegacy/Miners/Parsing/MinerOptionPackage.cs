using System;
using System.Collections.Generic;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Parsing
{
    public class MinerOptionPackage
    {
        public string Name;
        public MinerType Type;
        public List<MinerOption> GeneralOptions;
        public List<MinerOption> TemperatureOptions;

        public MinerOptionPackage(MinerType iType, List<MinerOption> iGeneralOptions, List<MinerOption> iTemperatureOptions)
        {
            Type = iType;
            GeneralOptions = iGeneralOptions;
            TemperatureOptions = iTemperatureOptions;
            Name = Enum.GetName(typeof(MinerType), iType);
        }
    }
}
