using System;
using System.Collections.Generic;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Configs.Data
{
    [Serializable]
    public class DeviceConfig
    {
        public string DeviceUUID = "";
        public string DeviceName = "";


        public bool Enabled = true;
        public double MinimumProfit = 0;
        public uint PowerTarget = uint.MinValue;
        public PowerLevel PowerLevel = PowerLevel.High;
        // TODO check last set power mode if it works


        //public int TimeLimit { get; set; }

        // benchmarks
        public List<PluginAlgorithmConfig> PluginAlgorithmSettings = new List<PluginAlgorithmConfig>();
    }
}
