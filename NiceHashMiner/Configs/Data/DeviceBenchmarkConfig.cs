using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMiner.Configs.Data {
    [Serializable]
    public class DeviceBenchmarkConfig {
        public string DeviceUUID = "";
        public string DeviceName = "";
        //Window location and monitor settings
        public int WindowMonitor = -1;
        public int WindowX = -1;
        public int WindowY = -1;
        //public int TimeLimit { get; set; }
        public List<AlgorithmConfig> AlgorithmSettings = new List<AlgorithmConfig>();
    }
}
