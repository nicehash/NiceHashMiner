using MinerPlugin;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMiner.Plugin;
using NHM.Common.Enums;
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
                return new MinerFromPlugin(pAlgo.BaseAlgo.MinerID, null, "");
            }
            return null;
        }

        // For mining
        public static Miner CreateMinerForMining(List<MiningPair> miningPairs, string groupKey)
        {
            var pair = miningPairs.FirstOrDefault();
            if (pair == null || pair.Algorithm == null) return null;
            var algorithm = pair.Algorithm;
            var plugin = MinerPluginsManager.GetPluginWithUuid(algorithm.MinerID);
            if (plugin != null)
            {
                return new MinerFromPlugin(algorithm.MinerID, miningPairs, groupKey);
            }
            return null;
        }
    }
}
