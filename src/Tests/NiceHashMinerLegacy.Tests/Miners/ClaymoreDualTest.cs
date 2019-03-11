using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMinerLegacy.Tests.Miners
{
    [TestClass]
    public class ClaymoreDualTest
    {
        private static int count;

        private class ClaymoreDualDummy : ClaymoreDual
        {
            public ClaymoreDualDummy(AlgorithmType secondaryAlgorithmType = AlgorithmType.NONE) 
                : base(secondaryAlgorithmType)
            { }
            
            public string GetDevCmd() => GetDevicesCommandString();
        }

        private class ComputeDevDummy : ComputeDevice
        {
            public ComputeDevDummy(int id, DeviceType type, int busID)
                : base(id, $"dummy-{type}-{id}", true, DeviceGroupType.NONE,
                    type, $"GPU{++count}", 1)
            {
                IDByBus = busID;
                Uuid = "fake";
            }
        }

        private static readonly List<ComputeDevice> Devices = new List<ComputeDevice>
        {
            new ComputeDevDummy(0, DeviceType.AMD, 1),
            new ComputeDevDummy(0, DeviceType.NVIDIA, 2),
            new ComputeDevDummy(1, DeviceType.AMD, 0),
            new ComputeDevDummy(2, DeviceType.NVIDIA, 0),
            new ComputeDevDummy(1, DeviceType.NVIDIA, 1)
        };

        internal static MiningSetup CreateMiningSetupAndInit(Predicate<ComputeDevice> selector)
        {
            AvailableDevices.NumDetectedAmdDevs = 2;
            AvailableDevices.NumDetectedNvDevs = 3;

            var algo = new Algorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto);
            var pairs = new List<MiningPair>();
            foreach (var dev in Devices.Where(d => selector(d)))
            {
                pairs.Add(new MiningPair(dev, algo));
            }

            return new MiningSetup(pairs);
        }

        internal static MiningSetup CreateMiningSetupAndInit()
        {
            return CreateMiningSetupAndInit(i => true);
        }

        [TestMethod]
        public void AllDevicesString_ShouldMatch()
        {
            var cd = new ClaymoreDualDummy();
            cd.InitMiningSetup(CreateMiningSetupAndInit());

            var devCmd = cd.GetDevCmd();

            Assert.AreEqual("-di 01234", devCmd);
        }

        [TestMethod]
        public void MissingDevicesString_ShouldMatch()
        {
            var cd = new ClaymoreDualDummy();
            cd.InitMiningSetup(CreateMiningSetupAndInit(d => d.ID != 1));

            Assert.AreEqual("-di 124", cd.GetDevCmd());
        }

        [TestMethod]
        public void AmdOnly_ShouldMatch()
        {
            var cd = new ClaymoreDualDummy();
            cd.InitMiningSetup(CreateMiningSetupAndInit(d => d.DeviceType == DeviceType.AMD));

            Assert.AreEqual("-di 01 -platform 1 ", cd.GetDevCmd());
        }

        [TestMethod]
        public void NVOnly_ShouldMatch()
        {
            var cd = new ClaymoreDualDummy();
            cd.InitMiningSetup(CreateMiningSetupAndInit(d => d.DeviceType == DeviceType.NVIDIA));

            Assert.AreEqual("-di 012 -platform 2 ", cd.GetDevCmd());
        }
    }
}
