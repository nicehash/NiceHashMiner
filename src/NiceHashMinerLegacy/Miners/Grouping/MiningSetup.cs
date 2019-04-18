using System.Collections.Generic;
using System.Linq;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Grouping
{
    public class MiningSetup
    {
        public List<MiningPair> MiningPairs { get; }
        public bool IsInit { get; }

        public IEnumerable<int> DeviceIDs => MiningPairs.Select(p => p.Device.ID);

        public MiningSetup(List<MiningPair> miningPairs)
        {
            IsInit = true; // TODO old
            if (miningPairs == null || miningPairs.Count <= 0) return;
            MiningPairs = miningPairs;
        }
    }
}
