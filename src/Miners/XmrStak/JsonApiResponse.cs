using System;
using System.Collections.Generic;

namespace XmrStak
{
#pragma warning disable IDE1006 // Naming Styles
    [Serializable]
    public class Hashrate
    {
        public List<List<double?>> threads { get; set; }
        public List<double?> total { get; set; }
        public double highest { get; set; }
    }

    [Serializable]
    public class Results
    {
        public int diff_current { get; set; }
        public int shares_good { get; set; }
        public int shares_total { get; set; }
        public double avg_time { get; set; }
        public int hashes_total { get; set; }
        public List<int> best { get; set; }
        public List<object> error_log { get; set; }
    }

    [Serializable]
    public class Connection
    {
        public string pool { get; set; }
        public int uptime { get; set; }
        public int ping { get; set; }
        public List<object> error_log { get; set; }
    }

    [Serializable]
    public class JsonApiResponse
    {
        public string version { get; set; }
        public Hashrate hashrate { get; set; }
        public Results results { get; set; }
        public Connection connection { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
