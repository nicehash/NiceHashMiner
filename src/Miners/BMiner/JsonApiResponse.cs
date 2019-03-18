using System;
using System.Collections.Generic;
using System.Text;

namespace BMiner
{
    [Serializable]
    public class Solver
    {
        public double solution_rate { get; set; }
        public double nonce_rate { get; set; }
    }

    [Serializable]
    public class Device
    {
        public int power { get; set; }
    }

    [Serializable]
    public class DeviceData
    {
        public Solver solver { get; set; }
        public Device device { get; set; }
    }

    [Serializable]
    public class JsonApiResponse
    {
        public Dictionary<string, DeviceData> miners { get; set; }
    }
}
