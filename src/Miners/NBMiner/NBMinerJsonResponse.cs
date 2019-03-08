using System;
using System.Collections.Generic;
using System.Text;

namespace NBMiner
{
    internal class NBMinerJsonResponse
    {
        public class MinerModel
        {
            public class DeviceModel
            {
                public double hashrate { get; set; }
            }

            public List<DeviceModel> devices { get; set; }

            public double total_hashrate { get; set; }
        }

        public MinerModel miner { get; set; }

        public double? TotalHashrate => miner?.total_hashrate;
    }
}
