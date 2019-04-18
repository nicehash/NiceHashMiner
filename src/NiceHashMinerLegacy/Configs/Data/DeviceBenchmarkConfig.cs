using System;
using System.Collections.Generic;

namespace NiceHashMiner.Configs.Data
{
    [Serializable]
    public class DeviceBenchmarkConfig
    {
        public string DeviceUUID = "";
        public string DeviceName = "";
        //public int TimeLimit { get; set; }
        public List<PluginAlgorithmConfig> PluginAlgorithmSettings = new List<PluginAlgorithmConfig>();
    }
}
