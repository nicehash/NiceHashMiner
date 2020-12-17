using System;
using System.Collections.Generic;

namespace XMRig
{
    [Serializable]
    public class Hashrate
    {
        public List<double?> total { get; set; }
    }

    [Serializable]
    public class JsonApiResponse
    {
        public string version { get; set; }
        public Hashrate hashrate { get; set; }
    }
}
