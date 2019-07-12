using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinerPlugin;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miners.MinerPluginToolkitV1.ExtraLaunchParameters.Test
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void Parse_Test()
        {
            var device1 = new BaseDevice(DeviceType.NVIDIA, "GPU-device1", "MSI GeForce GTX 1070 Ti", 0);
            var device2 = new BaseDevice(DeviceType.NVIDIA, "GPU-device2", "MSI GeForce GTX 1060 6GB", 1);
            var algorithm1 = new Algorithm("BMiner", AlgorithmType.GrinCuckaroo29);
            var algorithm2 = new Algorithm("BMiner", AlgorithmType.GrinCuckaroo29);
            var options = new List<MinerOption>
            {
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "optionParameter",
                    ShortName = "-nofee"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "singleParam",
                    ShortName = "-intensity",
                    DefaultValue = "6"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "multiParam",
                    ShortName = "--multi-intensity",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "multiParam_EqSign",
                    ShortName = "--multi-power=",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
            };

            //option is param
            algorithm1.ExtraLaunchParameters = "-nofee";
            var pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            var miningPairs = new List<MiningPair>() { pair1 };

            var ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            var exp = " -nofee";
            Assert.AreEqual(ret, exp);

            // Single param
            algorithm1.ExtraLaunchParameters = "-intensity 10";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            miningPairs = new List<MiningPair>() { pair1 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            exp = " -intensity 10";
            Assert.AreEqual(ret, exp);

            algorithm1.ExtraLaunchParameters = "-intensity 10 11";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            miningPairs = new List<MiningPair>() { pair1 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            exp = " -intensity 10";
            Assert.AreEqual(ret, exp);

            // Multi param
            algorithm1.ExtraLaunchParameters = "--multi-intensity 10,11";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            miningPairs = new List<MiningPair>() { pair1 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            exp = " --multi-intensity 10,11";
            Assert.AreEqual(ret, exp);

            // multi param, equal sign
            algorithm1.ExtraLaunchParameters = "--multi-power=10,11";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            miningPairs = new List<MiningPair>() { pair1 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            exp = " --multi-power=10,11";
            Assert.AreEqual(ret, exp);

            //multi param, 1 value
            algorithm1.ExtraLaunchParameters = "--multi-intensity 15";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            miningPairs = new List<MiningPair>() { pair1 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            exp = " --multi-intensity 15";
            Assert.AreEqual(ret, exp);

            //multi param, 1 value others default
            algorithm1.ExtraLaunchParameters = "--multi-intensity 15";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            miningPairs = new List<MiningPair>() { pair1 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            exp = " --multi-intensity 15";
            Assert.AreEqual(ret, exp);

            //multi param, no value
            algorithm1.ExtraLaunchParameters = "--multi-intensity";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            miningPairs = new List<MiningPair>() { pair1 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            exp = "";
            Assert.AreEqual(ret, exp);

            //multi param, 1value, other devices default 
            algorithm1.ExtraLaunchParameters = "--multi-intensity 15";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            var pair2 = new MiningPair
            {
                Device = device2,
                Algorithm = algorithm2
            };
            miningPairs = new List<MiningPair>() { pair1, pair2 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options);
            exp = " --multi-intensity 15,-1";
            Assert.AreEqual(ret, exp);

            //default values
            algorithm1.ExtraLaunchParameters = "";
            pair1 = new MiningPair
            {
                Device = device1,
                Algorithm = algorithm1
            };
            miningPairs = new List<MiningPair>() { pair1 };
            ret = ExtraLaunchParametersParser.Parse(miningPairs, options, true);
            exp = " -nofee -intensity 6 --multi-intensity -1 --multi-power=-1";
            Assert.AreEqual(ret, exp);
        }
    }
}
