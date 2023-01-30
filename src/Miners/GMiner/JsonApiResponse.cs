using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP.GMiner
{
    internal class DeviceApi
    {
        public int gpu_id { get; set; }
        public string bus_id { get; set; }
        public  string name { get; set; }
        public double speed { get; set; }
        public int accepted_shares { get; set; }
        public int rejected_shares { get; set; }
        public int stale_shares { get; set; }
        public int invalid_shares { get; set; }
        public int fan { get; set; }
        public int temperature { get; set; }
        public int temperature_limit { get; set; }
        public int memory_temperature { get; set; }
        public int memory_temperature_limit { get; set; }
        public int core_clock { get; set; }
        public int memory_clock { get; set; }
        public int power_usage { get; set; }
    }

    internal class JsonApiResponse
    {
        public string miner { get; set; }
        public string algorithm { get; set; }
        public int total_accepted_shares { get; set; }
        public int total_rejected_shares { get; set; }
        public int total_stale_shares { get; set; }
        public int total_invalid_shares { get; set; }

        public List<DeviceApi> devices { get; set; }
    }
}
