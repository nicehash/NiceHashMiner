using System;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Mining.Grouping
{
    public static class GroupingUtils
    {
        private static string CalcGroupedDevicesKey(List<AlgorithmContainer> group, string algorithmStringID)
        {
            var sortedDeviceUuids = new SortedSet<string>(group.Select(pair => pair.ComputeDevice.Uuid));
            var key = $"{algorithmStringID}({string.Join(",", sortedDeviceUuids)})";
            return key;
        }

        private static bool CanGroupAlgorithmContainer(AlgorithmContainer a, AlgorithmContainer b)
        {
            // must be from the same plugin container instance (same miner binary)
            if (a.PluginContainer != b.PluginContainer) return false;
            // never group same devices
            if (a.ComputeDevice.Uuid == b.ComputeDevice.Uuid) return false;
            return a.PluginContainer.CanGroupAlgorithmContainer(a, b);
        }

        private static bool IsCurrentWithinGroup(AlgorithmContainer current, List<AlgorithmContainer> group) => group.Any(p => p.ComputeDevice.Uuid == current.ComputeDevice.Uuid);

        public static Dictionary<string, List<AlgorithmContainer>> GetGroupedAlgorithmContainers(List<AlgorithmContainer> profitableAlgorithmContainers)
        {
            // Group compatible mining pairs into miners.
            // #1 mining pairs are compatible if they use the same miner binary (same Plugin/Miner UUID + Version)
            // #2 after that we check the plugins CanGroup (most often the case is different devices same algorithm but this is dependant on the plugin)
            // The less miner instances we have the better.
            var groupedAlgorithms = new Dictionary<string, List<AlgorithmContainer>>();
            Func<AlgorithmContainer, bool> isAlreadyGrouped = (AlgorithmContainer current) => groupedAlgorithms.Values.Any(group => IsCurrentWithinGroup(current, group));

            foreach (var current in profitableAlgorithmContainers)
            {
                if (isAlreadyGrouped(current)) continue;

                var newGroup = new List<AlgorithmContainer>() { current };
                var restInGroup = profitableAlgorithmContainers
                    .SkipWhile(algo => current != algo)
                    .Where(algo => !isAlreadyGrouped(algo))
                    .Where(algo => CanGroupAlgorithmContainer(current, algo));
                newGroup.AddRange(restInGroup);

                // save newly grouped mining pairs
                var newGroupKey = CalcGroupedDevicesKey(newGroup, current.AlgorithmStringID);
                groupedAlgorithms[newGroupKey] = newGroup;
            }

            return groupedAlgorithms;
        }
    }
}
