using System;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Configs.Data
{
    [Serializable]
    public class ComputeDeviceConfig
    {
        public string Name = "";
        public bool Enabled = true;
        public string UUID = "";
        public double MinimumProfit = 0;

        public uint PowerTarget = uint.MinValue;
        public PowerLevel PowerLevel = PowerLevel.High;
        // TODO check last set power mode if it works
    }
}
