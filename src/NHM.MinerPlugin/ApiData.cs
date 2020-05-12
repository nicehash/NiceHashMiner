using NHM.Common.Enums;
using System;
using System.Collections.Generic;

namespace NHM.MinerPlugin
{
    /// <summary>
    /// This class is used for saving of the data we get from miner API
    /// For speed data it has AlgorithmSpeedsTotal and AlgorithmSpeedsPerDevice properties
    /// For power usage data it has PowerUsageTotal and PowerUsagePerDevice properties
    /// </summary>
    [Serializable]
    public class ApiData
    {
        // total
        public IReadOnlyList<(AlgorithmType type, double speed)> AlgorithmSpeedsTotal;
        public int PowerUsageTotal;
        // per device
        // key is device UUID
        public IReadOnlyDictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>> AlgorithmSpeedsPerDevice;
        public IReadOnlyDictionary<string, int> PowerUsagePerDevice;
        public string ApiResponse;
    }
}
