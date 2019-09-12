using System;
using System.Collections.Generic;

namespace NHMCore.Configs.Data
{
    [Serializable]
    public class DeviceConfig
    {
        public string DeviceUUID = "";
        public string DeviceName = "";


        public bool Enabled = true;
        public double MinimumProfit = 0;

        public DeviceTDPSettings TDPSettings;

        //public int TimeLimit { get; set; }

        // benchmarks
        public List<PluginAlgorithmConfig> PluginAlgorithmSettings = new List<PluginAlgorithmConfig>();
    }
}
