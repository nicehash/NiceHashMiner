using System;
using System.Collections.Generic;

namespace GMinerPlugin
{
    [Serializable]
    internal class Device
    {
        public int gpu_id { get; set; }
        public string bus_id { get; set; }
        public string name { get; set; }
        public double speed { get; set; }
        public double speed2 { get; set; }
        public int accepted_shares { get; set; }
        public int accepted_shares2 { get; set; }
        public int rejected_shares { get; set; }
        public int rejected_shares2 { get; set; }
        public int temperature { get; set; }
        public int temperature_limit { get; set; }
        public int power_usage { get; set; }
    }

    [Serializable]
    internal class JsonApiResponse
    {
        public int uptime { get; set; }
        public string server { get; set; }
        public string user { get; set; }
        public string algorithm { get; set; }
        public double electricity { get; set; }
        public int total_accepted_shares { get; set; }
        public int total_rejected_shares { get; set; }
        public List<Device> devices { get; set; }
        public int speedRatePrecision { get; set; }
        public string speedUnit { get; set; }
        public string powerUnit { get; set; }
    }
}
