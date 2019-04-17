using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Plugin;

namespace NiceHashMiner.Miners.Grouping
{
    public static class GroupingLogic
    {
        public static bool ShouldGroup(MiningPair a, MiningPair b)
        {
            // plugin case
            if (IsPluginAlgorithm(a) && IsPluginAlgorithm(b))
            {
                return CheckPluginCanGroup(a, b);
            }
            // no mixinng of plugin and non plugin algorithms
            if (IsPluginAlgorithm(a) != IsPluginAlgorithm(b)) return false;

            // ClaymoreDual should be able to mix AMD and NVIDIA
            var canGroup = IsSameBinPath(a, b) && IsSameAlgorithmType(a, b); //&& IsSameDeviceType(a, b);
            return canGroup;
        }

        private static bool IsSameBinPath(MiningPair a, MiningPair b)
        {
            return a.Algorithm.MinerBinaryPath == b.Algorithm.MinerBinaryPath;
        }
        private static bool IsSameAlgorithmType(MiningPair a, MiningPair b) 
        {
            return a.Algorithm.DualNiceHashID == b.Algorithm.DualNiceHashID;
        }

        private static bool IsSameDeviceType(MiningPair a, MiningPair b)
        {
            return a.Device.DeviceType == b.Device.DeviceType;
        }

        // PLUGIN stuff 
        private static bool IsPluginAlgorithm(MiningPair a)
        {
            return a.Algorithm.MinerBaseType == MinerBaseType.PLUGIN && a.Algorithm is PluginAlgorithm;
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
