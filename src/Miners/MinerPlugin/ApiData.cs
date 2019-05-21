using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinerPlugin
{
    // TODO find a way to improve this
    /// <summary>
    /// This class is used for saving of the data we get from miner API
    /// For speed data it has AlgorithmSpeedsTotal and AlgorithmSpeedsPerDevice properties
    /// For power usage data it has PowerUsageTotal and PowerUsagePerDevice properties
    /// </summary>
    [Serializable]
    public class ApiData
    {
        // total
        public IReadOnlyList<AlgorithmTypeSpeedPair> AlgorithmSpeedsTotal;
        public int PowerUsageTotal;
        // per device
        // key is device UUID
        public IReadOnlyDictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>> AlgorithmSpeedsPerDevice;
        public IReadOnlyDictionary<string, int> PowerUsagePerDevice;
    }
}
