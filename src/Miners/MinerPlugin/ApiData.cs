using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinerPlugin
{
    // TODO find a way to improve this
    [Serializable]
    public class ApiData
    {
        // total
        public IReadOnlyList<AlgorithmTypeSpeedPair> AlgorithmSpeedsTotal;
        public IReadOnlyList<AlgorithmTypeSpeedPair> AlgorithmSecondarySpeedsTotal;
        public int PowerUsageTotal;
        // per device
        // key is device UUID
        public IReadOnlyDictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>> AlgorithmSpeedsPerDevice;
        public IReadOnlyDictionary<string, int> PowerUsagePerDevice;
    }
}
