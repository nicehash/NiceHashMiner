using System;
using System.Collections.Generic;
using System.Text;

namespace EWBF
{
#pragma warning disable IDE1006
    [Serializable]
    internal class Result
    {
        public uint gpuid { get; set; }
        public uint cudaid { get; set; }
        public string busid { get; set; }
        public uint gpu_status { get; set; }
        public int solver { get; set; }
        public int temperature { get; set; }
        public uint gpu_power_usage { get; set; }
        public uint speed_sps { get; set; }
        public uint accepted_shares { get; set; }
        public uint rejected_shares { get; set; }
    }

    [Serializable]
    internal class JsonApiResponse
    {
        public uint id { get; set; }
        public string method { get; set; }
        public object error { get; set; }
        public List<Result> result { get; set; }
    }
#pragma warning restore IDE1006
}
