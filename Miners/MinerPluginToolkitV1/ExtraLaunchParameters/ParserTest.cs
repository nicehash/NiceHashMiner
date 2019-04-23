using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinerPlugin;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System.Collections.Generic;

namespace Miners.MinerPluginToolkitV1.ExtraLaunchParameters.Test
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void Parse_Test()
        {
            var device1 = new BaseDevice(DeviceType.NVIDIA, "GPU-d97bdb7c-4155-9124-31b7-4743e16d3ac0", "MSI GeForce GTX 1070 Ti", 0);
            var algorithm1 = new Algorithm("BMiner", AlgorithmType.GrinCuckaroo29);
            algorithm1.ExtraLaunchParameters = "-intensity 10";
            var pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            var miningPairs = new List<MiningPair>() { pair1};
            var options = new List<MinerOption>
            {
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_dual_subsolver",
                    ShortName = "-dual-subsolver",
                    DefaultValue = "-1"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "bminer_cpu_intensity",
                    ShortName = "-intensity",
                    DefaultValue = "6"
                }
            };

            var ret = Parser.Parse(miningPairs, options);
            var exp = "-intensity 10";

            Assert.AreEqual(ret, exp);
        }
    }
}
