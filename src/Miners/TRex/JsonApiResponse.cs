using System;
using System.Collections.Generic;

namespace TRex
{
    [Serializable]
    internal class Gpu
    {
        public int device_id { get; set; }
        public string efficiency { get; set; }
        public int fan_speed { get; set; }
        public int gpu_id { get; set; }
        public int hashrate { get; set; }
        public double intensity { get; set; }
        public string name { get; set; }
        public int power { get; set; }
        public int temperature { get; set; }
        public string vendor { get; set; }
    }

    [Serializable]
    internal class JsonApiResponse
    {
        public string algorithm { get; set; }
        public string api { get; set; }
        public string cuda { get; set; }
        public string description { get; set; }
        public double difficulty { get; set; }
        public int gpu_total { get; set; }
        public List<Gpu> gpus { get; set; }
        public int hashrate { get; set; }
        public string name { get; set; }
        public string os { get; set; }
    }

}
