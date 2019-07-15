using NHM.Common.Enums;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Mining.Plugins;
using System.Collections.Generic;
using System.Linq;
using MinerPlugin;

namespace NiceHashMiner.Mining.Grouping
{
    public static class GroupingUtils
    {
        public static string CalcGroupedDevicesKey(SortedSet<string> sortedKeys, string algorithmStringID)
        {
            var key = $"{algorithmStringID}({string.Join(",", sortedKeys)})";
            return key;
        }

        public static bool ShouldGroup(MiningPair a, MiningPair b)
        {
            if (a.Algorithm.MinerID != b.Algorithm.MinerID) return false;
            var plugin = MinerPluginsManager.GetPluginWithUuid(a.Algorithm.MinerID);
            if (plugin == null) return false;
            var canGroup = plugin.CanGroup(a, b);
            return canGroup;
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
                var isInGroup = currentGroupedDevices.Any(groupedDevices => groupedDevices.Contains(firstDev.UUID));
                // if device is not in any group create new group and check if other device should group
                if (isInGroup == false)
                {
                    var newGroup = new SortedSet<string>();
                    var miningPairs = new List<MiningPair>()
                        {
                            profitableMiningPairs[first]
                        };
                    newGroup.Add(firstDev.UUID);
                    for (var second = first + 1; second < profitableMiningPairs.Count; ++second)
                    {
                        // check if we should group
                        var firstPair = profitableMiningPairs[first];
                        var secondPair = profitableMiningPairs[second];
                        if (ShouldGroup(firstPair, secondPair))
                        {
                            var secondDev = profitableMiningPairs[second].Device;
                            newGroup.Add(secondDev.UUID);
                            miningPairs.Add(profitableMiningPairs[second]);
                        }
                    }

                    currentGroupedDevices.Add(newGroup);
                    var newGroupKey = CalcGroupedDevicesKey(newGroup, firstAlgo.AlgorithmStringID);
                    newGroupedMiningPairs[newGroupKey] = miningPairs;
                }
            }

            return newGroupedMiningPairs;
        }
    }
}
