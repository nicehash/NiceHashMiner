using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Miners;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMinerLegacy.Tests.Miners
{
    [TestClass]
    public class PhoenixTest
    {
        private class PhoenixDummy : Phoenix
        {
            public string GetDevCmd() => GetDevicesCommandString();
        }

        [TestMethod]
        public void AllDevicesString_ShouldMatch()
        {
            var cd = new PhoenixDummy();
            cd.InitMiningSetup(ClaymoreDualTest.CreateMiningSetupAndInit());

            var devCmd = cd.GetDevCmd();

            Assert.AreEqual("-di 01234", devCmd);
        }

        [TestMethod]
        public void MissingDevicesString_ShouldMatch()
        {
            var cd = new PhoenixDummy();
            cd.InitMiningSetup(ClaymoreDualTest.CreateMiningSetupAndInit(d => d.ID != 1));

            Assert.AreEqual("-di 024", cd.GetDevCmd());
        }

        [TestMethod]
        public void AmdOnly_ShouldMatch()
        {
            var cd = new PhoenixDummy();
            cd.InitMiningSetup(ClaymoreDualTest.CreateMiningSetupAndInit(d => d.DeviceType == DeviceType.AMD));

            Assert.AreEqual("-di 01 -platform 1 ", cd.GetDevCmd());
        }

        [TestMethod]
        public void NVOnly_ShouldMatch()
        {
            var cd = new PhoenixDummy();
            cd.InitMiningSetup(ClaymoreDualTest.CreateMiningSetupAndInit(d => d.DeviceType == DeviceType.NVIDIA));

            Assert.AreEqual("-di 012 -platform 2 ", cd.GetDevCmd());
        }
    }
}
