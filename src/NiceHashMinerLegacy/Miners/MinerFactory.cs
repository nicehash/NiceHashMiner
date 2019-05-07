using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Miners
{
    public static class MinerFactory
    {
        // For benchmark
        public static Miner CreateMinerForBenchmark(Algorithm algorithm)
        {
            if (algorithm is PluginAlgorithm pAlgo)
            {
                return new MinerFromPlugin(pAlgo.BaseAlgo.MinerID, null);
            }
            return null;
        }

        // For mining
        public static Miner CreateMinerForMining(List<Miners.Grouping.MiningPair> miningPairs)
        {
            var pair = miningPairs.FirstOrDefault();
            if (pair == null) return null;
            var algorithm = pair.Algorithm;
            if (algorithm is PluginAlgorithm pAlgo)
            {
                return new MinerFromPlugin(pAlgo.BaseAlgo.MinerID, miningPairs);
            }
            return null;
        }
    }
}
