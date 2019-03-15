using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Equihash;
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

        private static Miner CreateClaymore(Algorithm algorithm)
        {
            switch (algorithm.NiceHashID)
            {
                //case AlgorithmType.Equihash:
                //    return new ClaymoreZcashMiner();
                //case AlgorithmType.CryptoNightV7:
                //    return new ClaymoreCryptoNightMiner();
                case AlgorithmType.DaggerHashimoto:
                    return new ClaymoreDual(algorithm.SecondaryNiceHashID);
            }

            return null;
        }

        private static Miner CreateEwbf(AlgorithmType type)
        {
            //if (type == AlgorithmType.Equihash)
            //{
            //    return new Ewbf();
            //}
            if (type == AlgorithmType.ZHash)
            {
                return new Ewbf144();
            }

            return null;
        }

        public static Miner CreateMiner(DeviceType deviceType, Algorithm algorithm)
        {
            switch (algorithm.MinerBaseType)
            {
                case MinerBaseType.TTMiner:
                    return new Ttminer();
                case MinerBaseType.ccminer:
                case MinerBaseType.ccminer_alexis:
                    return new Ccminer();
                case MinerBaseType.sgminer:
                    return new Sgminer();
                case MinerBaseType.ethminer:
                    return CreateEthminer(deviceType);
                case MinerBaseType.Claymore:
                    return CreateClaymore(algorithm);
                case MinerBaseType.XmrStak:
                    return new XmrStak.XmrStak();
                case MinerBaseType.EWBF:
                    return CreateEwbf(algorithm.NiceHashID);
                case MinerBaseType.Prospector:
                    return new Prospector();
                case MinerBaseType.trex:
                    return new Trex();
                case MinerBaseType.Phoenix:
                    return new Phoenix();
                case MinerBaseType.GMiner:
                    return new GMiner();
                case MinerBaseType.BMiner:
                    return new BMiner(algorithm.NiceHashID);
                case MinerBaseType.NBMiner:
                    return new NBMiner();
                case MinerBaseType.TeamRedMiner:
                    return new TeamRedMiner();
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
    }
}
