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
        public IReadOnlyList<(AlgorithmType type, double speed)> AlgorithmSpeedsTotal;
        public int PowerUsageTotal;
        // per device
        public IReadOnlyList<(string UUID, IReadOnlyList<(AlgorithmType type, double speed)>)> AlgorithmSpeedsPerDevice;
        public IReadOnlyList<(string UUID, int power)> PowerUsagePerDevice;
    }
}
