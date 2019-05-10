using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Miners.Grouping
{
    // TODO rename to GroupingUtils
    public static class GroupingLogic
    {
        public static string CalcGroupedDevicesKey(SortedSet<string> sortedKeys, Algorithm algorithm)
        {
            var key = $"{algorithm.AlgorithmStringID}({string.Join(",", sortedKeys)})";
            return key;
        }

        public static AlgorithmType GetMinerPairAlgorithmType(IEnumerable<MiningPair> miningPairs)
        {
            return miningPairs.FirstOrDefault()?.Algorithm?.AlgorithmUUID ?? AlgorithmType.NONE;
        }

        public static bool ShouldGroup(MiningPair a, MiningPair b)
        {
            // now all are plugin cases
            return CheckPluginCanGroup(a, b);
        }

        public static Dictionary<string, List<MiningPair>> GetGroupedMiningPairs(List<MiningPair> profitableMiningPairs)
        {
            // group new miners 
            var newGroupedMiningPairs = new Dictionary<string, List<MiningPair>>();
            // group devices with same supported algorithms
            var currentGroupedDevices = new List<SortedSet<string>>();
            for (var first = 0; first < profitableMiningPairs.Count; ++first)
            {
                var firstDev = profitableMiningPairs[first].Device;
                var firstAlgo = profitableMiningPairs[first].Algorithm;
                // check if is in group
                var isInGroup = currentGroupedDevices.Any(groupedDevices => groupedDevices.Contains(firstDev.Uuid));
                // if device is not in any group create new group and check if other device should group
                if (isInGroup == false)
                {
                    var newGroup = new SortedSet<string>();
                    var miningPairs = new List<MiningPair>()
                        {
                            profitableMiningPairs[first]
                        };
                    newGroup.Add(firstDev.Uuid);
                    for (var second = first + 1; second < profitableMiningPairs.Count; ++second)
                    {
                        // check if we should group
                        var firstPair = profitableMiningPairs[first];
                        var secondPair = profitableMiningPairs[second];
                        if (GroupingLogic.ShouldGroup(firstPair, secondPair))
                        {
                            var secondDev = profitableMiningPairs[second].Device;
                            newGroup.Add(secondDev.Uuid);
                            miningPairs.Add(profitableMiningPairs[second]);
                        }
                    }

                    currentGroupedDevices.Add(newGroup);
                    var newGroupKey = GroupingLogic.CalcGroupedDevicesKey(newGroup, firstAlgo);
                    newGroupedMiningPairs[newGroupKey] = miningPairs;
                }
            }

            return newGroupedMiningPairs;
        }

        private static bool CheckPluginCanGroup(MiningPair a, MiningPair b)
        {
            var pluginA = new MinerPlugin.MiningPair
            {
                Device = a.Device.PluginDevice,
                Algorithm = ((PluginAlgorithm) a.Algorithm).BaseAlgo
            };
            var pluginB = new MinerPlugin.MiningPair
            {
                Device = b.Device.PluginDevice,
                Algorithm = ((PluginAlgorithm) b.Algorithm).BaseAlgo
            };

            if (pluginA.Algorithm.MinerID != pluginB.Algorithm.MinerID) return false;

            var plugin = MinerPluginsManager.GetPluginWithUuid(pluginA.Algorithm.MinerID);
            // TODO can plugin be null?
            var canGroup = plugin.CanGroup(pluginA, pluginB);
            return canGroup;
        }
    }
}
