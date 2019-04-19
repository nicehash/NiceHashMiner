using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinerPlugin;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System.Collections.Generic;

namespace Miners.MinerPluginToolkitV1.ExtraLaunchParameters
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void Parse_Test()
        {
            var device1 = new BaseDevice(DeviceType.NVIDIA, "GPU-d97bdb7c-4155-9124-31b7-4743e16d3ac0", "MSI GeForce GTX 1070 Ti", 0);
            var algorithm1 = new Algorithm();
            var pair = new MiningPair
            {
                Device = device1,
                Algorithm = 
            };
            var miningPairs = new List<MiningPair>();

            var options = new List<MinerOption>();
        }
    }
}
