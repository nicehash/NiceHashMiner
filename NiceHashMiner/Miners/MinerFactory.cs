using NiceHashMiner.Devices;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Equihash;

namespace NiceHashMiner.Miners
{
    public class MinerFactory
    {
        private static Miner CreateEthminer(DeviceType deviceType)
        {
            if (DeviceType.AMD == deviceType)
            {
                return new MinerEtherumOCL();
            }
            return DeviceType.NVIDIA == deviceType ? new MinerEtherumCUDA() : null;
        }

        private static Miner CreateClaymore(AlgorithmType algorithmType, AlgorithmType secondaryAlgorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Equihash:
                    return new ClaymoreZcashMiner();
                case AlgorithmType.CryptoNight:
                    return new ClaymoreCryptoNightMiner();
                case AlgorithmType.DaggerHashimoto:
                    return new ClaymoreDual(secondaryAlgorithmType);
            }
            return null;
        }

        private static Miner CreateExperimental(DeviceType deviceType, AlgorithmType algorithmType)
        {
            if (AlgorithmType.NeoScrypt == algorithmType && DeviceType.NVIDIA == deviceType)
            {
                return new Ccminer();
            }
            return null;
        }

        public static Miner CreateMiner(DeviceType deviceType, AlgorithmType algorithmType, MinerBaseType minerBaseType,
            AlgorithmType secondaryAlgorithmType = AlgorithmType.NONE)
        {
            switch (minerBaseType)
            {
                case MinerBaseType.ccminer:
                    return new Ccminer();
                case MinerBaseType.sgminer:
                    return new Sgminer();
                case MinerBaseType.nheqminer:
                    return new NhEqMiner();
                case MinerBaseType.ethminer:
                    return CreateEthminer(deviceType);
                case MinerBaseType.Claymore:
                    return CreateClaymore(algorithmType, secondaryAlgorithmType);
                case MinerBaseType.OptiminerAMD:
                    return new OptiminerZcashMiner();
                case MinerBaseType.excavator:
                    return new Excavator();
                case MinerBaseType.XmrStackCPU:
                    return new XmrStackCPUMiner();
                case MinerBaseType.ccminer_alexis:
                    return new Ccminer();
                case MinerBaseType.experimental:
                    return CreateExperimental(deviceType, algorithmType);
                case MinerBaseType.EWBF:
                    return new Ewbf();
                case MinerBaseType.Prospector:
                    return new Prospector();
                case MinerBaseType.Xmrig:
                    return new Xmrig();
                case MinerBaseType.XmrStakAMD:
                    return new XmrStakAMD();
                case MinerBaseType.Claymore_old:
                    return new ClaymoreCryptoNightMiner(true);
            }
            return null;
        }

        // create miner creates new miners based on device type and algorithm/miner path
        public static Miner CreateMiner(ComputeDevice device, Algorithm algorithm)
        {
            if (device != null && algorithm != null)
            {
                return CreateMiner(device.DeviceType, algorithm.NiceHashID, algorithm.MinerBaseType, algorithm.SecondaryNiceHashID);
            }
            return null;
        }
    }
}
