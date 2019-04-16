using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public static class MinerFactory
    {
        public static Miner CreateMiner(DeviceType deviceType, Algorithm algorithm)
        {
            switch (algorithm.MinerBaseType)
            {
                case MinerBaseType.Claymore:
                    return new ClaymoreDualOld(algorithm.SecondaryNiceHashID);
                case MinerBaseType.PLUGIN:
                    return CreateMinerFromPlugin(algorithm);
            }

            return null;
        }

        // create miner creates new miners based on device type and algorithm/miner path
        public static Miner CreateMiner(ComputeDevice device, Algorithm algorithm)
        {
            if (device != null && algorithm != null)
            {
                return CreateMiner(device.DeviceType, algorithm);
            }

            return null;
        }

        private static MinerFromPlugin CreateMinerFromPlugin(Algorithm algorithm)
        {
            if (algorithm is PluginAlgorithm pAlgo)
            {
                return new MinerFromPlugin(pAlgo.BaseAlgo.MinerID);
            }
            return null;
        }
    }
}
