using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public static class MinerFactory
    {
        private static Miner CreateEthminer(DeviceType deviceType)
        {
            if (DeviceType.AMD == deviceType)
            {
                return new MinerEtherumOCL();
            }

            return DeviceType.NVIDIA == deviceType ? new MinerEtherumCUDA() : null;
        }

        public static Miner CreateMiner(DeviceType deviceType, Algorithm algorithm)
        {
            switch (algorithm.MinerBaseType)
            {
                //case MinerBaseType.sgminer:
                //    return new Sgminer();
                case MinerBaseType.ethminer:
                    return CreateEthminer(deviceType);
                case MinerBaseType.Claymore:
                    return new ClaymoreDual(algorithm.SecondaryNiceHashID);
                case MinerBaseType.Phoenix:
                    return new Phoenix();
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
