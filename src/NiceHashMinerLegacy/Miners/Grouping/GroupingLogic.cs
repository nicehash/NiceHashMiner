using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Algorithms;

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

            var canGroup = IsGroupableMinerBaseType(a) && IsGroupableMinerBaseType(b);
            // group if same bin path and same algo type
            if (canGroup && IsSameBinPath(a, b) && IsSameAlgorithmType(a, b) &&
                ((IsNotCpuGroups(a, b) && IsSameDeviceType(a, b)) ||
                 (a.Algorithm.MinerBaseType == MinerBaseType.Prospector &&
                 b.Algorithm.MinerBaseType == MinerBaseType.Prospector) ||
                 a.Algorithm.MinerBaseType == MinerBaseType.XmrStak || a.Algorithm.MinerBaseType == MinerBaseType.GMiner)) 
                return true;

            return false;
        }

        private static bool IsNotCpuGroups(MiningPair a, MiningPair b)
        {
            return a.Device.DeviceType != DeviceType.CPU && b.Device.DeviceType != DeviceType.CPU;
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
        private static bool IsGroupableMinerBaseType(MiningPair a) 
        {
            return a.Algorithm.MinerBaseType != MinerBaseType.cpuminer;
        }

        // PLUGIN stuff 
        private static bool IsPluginAlgorithm(MiningPair a)
        {
            return a.Algorithm.MinerBaseType == MinerBaseType.PLUGIN && a.Algorithm is PluginAlgorithm;
        }
        private static bool CheckPluginCanGroup(MiningPair a, MiningPair b)
        {
            var pluginA = (device: a.Device.PluginDevice, algorithm: ((PluginAlgorithm)a.Algorithm).BaseAlgo);
            var pluginB = (device: b.Device.PluginDevice, algorithm: ((PluginAlgorithm)b.Algorithm).BaseAlgo);

            if (pluginA.algorithm.MinerID != pluginB.algorithm.MinerID) return false;

            var plugin = MinerPluginsManager.GetPluginWithUuid(pluginA.algorithm.MinerID);
            // TODO can plugin be null?
            var canGroup = plugin.CanGroup(pluginA, pluginB);
            return canGroup;
        }
    }
}
