using System;
using System.Collections.Generic;

namespace SRBMiner
{
    [Serializable]
    internal class Device
    {
        public int id { get; set; }
        public int bus_id { get; set; }
        public string device { get; set; }
    }

    [Serializable]
    internal class AlgorithmInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public Hashrate hashrate { get; set; }
    }

    [Serializable]
    internal class Hashrate
    {
        public Dictionary<string, double> gpu { get; set; }
    }

    [Serializable]
    internal class ApiJsonResponse
    {
        public double hashrate_total_now { get; set; }
        public List<Device> gpu_devices { get; set; }
        public List<AlgorithmInfo> algorithms { get; set; }
    }
}
