using System;
using System.Collections.Generic;

namespace SRBMiner
{
    [Serializable]
    internal class Device
    {
        public string device { get; set; }
        public int device_id { get; set; }
        public int bus_id { get; set; }
    }

    [Serializable]
    internal class ApiJsonResponse
    {
        public double hashrate_total_now { get; set; }
        public List<Device> gpu_devices { get; set; }
        public List<Dictionary<string, double>> gpu_hashrate {get;set;}
    }
}
