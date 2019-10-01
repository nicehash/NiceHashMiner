using System.Collections.Generic;

namespace NBMiner
{
    internal class DeviceApi
    {
        public double core_clock { get; set; }
        public double core_utilization { get; set; }
        public double fan { get; set; }
        public string hashrate { get; set; }
        public string hashrate2 { get; set; }
        public double hashrate2_raw { get; set; }
        public double hashrate_raw { get; set; }
        public int id { get; set; }
        public string info { get; set; }
        public double mem_clock { get; set; }
        public double mem_utilization { get; set; }
        public double power { get; set; }
        public double temperature { get; set; }
    }

    internal class Miner
    {
        public List<DeviceApi> devices { get; set; }
        public string total_hashrate { get; set; }
        public string total_hashrate2 { get; set; }
        public double total_hashrate2_raw { get; set; }
        public double total_hashrate_raw { get; set; }
        public double total_power_consume { get; set; }
    }

    //internal class Stratum
    //{
    //    public int accepted_shares { get; set; }
    //    public string algorithm { get; set; }
    //    public string difficulty { get; set; }
    //    public int latency { get; set; }
    //    public int rejected_shares { get; set; }
    //    public string url { get; set; }
    //    public bool use_ssl { get; set; }
    //    public string user { get; set; }
    //}

    internal class JsonApiResponse
    {
        public Miner miner { get; set; }
        //public int start_time { get; set; }
        //public Stratum stratum { get; set; }
        public string version { get; set; }
    }
}
