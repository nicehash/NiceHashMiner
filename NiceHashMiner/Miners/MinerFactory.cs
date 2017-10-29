﻿using NiceHashMiner.Devices;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Equihash;
using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMiner.Miners {
    public class MinerFactory {

        private static Miner CreateEthminer(DeviceType deviceType) {
            if (DeviceType.AMD == deviceType) {
                return new MinerEtherumOCL();
            } else if(DeviceType.NVIDIA == deviceType) {
                return new MinerEtherumCUDA();
            }
            return null;
        }

        private static Miner CreateClaymore(Algorithm algorithm) {
            var algorithmType = algorithm.NiceHashID;
            if (AlgorithmType.Equihash == algorithmType) {
                return new ClaymoreZcashMiner();
            } else if (AlgorithmType.CryptoNight == algorithmType) {
                return new ClaymoreCryptoNightMiner();
            } else if (AlgorithmType.DaggerHashimoto == algorithmType) {
                return new ClaymoreDual(algorithm.SecondaryNiceHashID);
            }
            return null;
        }

        private static Miner CreateExperimental(DeviceType deviceType, AlgorithmType algorithmType) {
            if (AlgorithmType.NeoScrypt == algorithmType && DeviceType.NVIDIA == deviceType) {
                return new ccminer();
            }
            return null;
        }

        public static Miner CreateMiner(DeviceType deviceType, Algorithm algorithm) {
            var minerBaseType = algorithm.MinerBaseType;
            switch (minerBaseType) {
                case MinerBaseType.ccminer:
                    return new ccminer();
                case MinerBaseType.sgminer:
                    return new sgminer();
                case MinerBaseType.nheqminer:
                    return new nheqminer();
                case MinerBaseType.ethminer:
                    return CreateEthminer(deviceType);
                case MinerBaseType.Claymore:
                    return CreateClaymore(algorithm);
                case MinerBaseType.OptiminerAMD:
                    return new OptiminerZcashMiner();
                case MinerBaseType.excavator:
                    return new excavator();
                case MinerBaseType.XmrStackCPU:
                    return new XmrStackCPUMiner();
                case MinerBaseType.ccminer_alexis:
                    return new ccminer();
                case MinerBaseType.experimental:
                    return CreateExperimental(deviceType, algorithm.NiceHashID);
                case MinerBaseType.EWBF:
                    return new EWBF();
                case MinerBaseType.Prospector:
                    return new Prospector();
                case MinerBaseType.Xmrig:
                    return new Xmrig();
                case MinerBaseType.XmrStakAMD:
                    return new XmrStakAMD();
            }
            return null;
        }

        // create miner creates new miners based on device type and algorithm/miner path
        public static Miner CreateMiner(ComputeDevice device, Algorithm algorithm) {
            if (device != null && algorithm != null) {
                return CreateMiner(device.DeviceType, algorithm);
            }
            return null;
        }
    }
}
