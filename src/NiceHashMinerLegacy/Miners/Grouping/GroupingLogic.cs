using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Plugin;

namespace NiceHashMiner.Miners.Grouping
{
    public static class GroupingLogic
    {
        public static bool ShouldGroup(MiningPair a, MiningPair b)
        {
            // now all are plugin cases
            return CheckPluginCanGroup(a, b);
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
