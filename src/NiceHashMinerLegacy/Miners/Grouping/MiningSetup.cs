using System.Collections.Generic;
using System.Linq;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Grouping
{
    public class MiningSetup
    {
        public List<MiningPair> MiningPairs { get; }
        public bool IsInit { get; }

        public AlgorithmType[] AlgorithmIDs()
        {
            return MiningPairs[0].Algorithm.IDs;
        }

        public MiningSetup(List<MiningPair> miningPairs)
        {
            IsInit = true; // TODO old
            if (miningPairs == null || miningPairs.Count <= 0) return;
            MiningPairs = miningPairs;
        }
    }
}
