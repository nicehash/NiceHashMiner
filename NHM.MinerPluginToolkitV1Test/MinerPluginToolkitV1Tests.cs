using NHM.MinerPluginToolkitV1.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NHM.MinerPluginToolkitV1.CommandLine.MinerExtraParameters;

namespace NHM.MinerPluginToolkitV1Test
{
    [TestClass]
    public class MinerPluginToolkitV1Tests
    {
        internal class TestLabel
        {
            private int _testCount = 0;
            public string label() => $"#{++_testCount}";
        }

        [TestMethod]
        public void TestMinerExtraParametersParse()
        {
            var tl = new TestLabel { };
            var devices = new List<List<List<string>>>();
            devices.Add(new List<List<string>>());
            devices[0].Add(new List<string>());
            devices[0].Add(new List<string>());
            devices[0].Add(new List<string>());
            devices[0][0].Add("--zombie-mode");
            devices[0][0].Add("1");
            devices[0][0].Add(",");
            devices[0][1].Add("--test");
            devices[0][1].Add("3");
            devices[0][2].Add("--makex");
            devices.Add(new List<List<string>>());
            devices[1].Add(new List<string>());
            devices[1].Add(new List<string>());
            devices[1].Add(new List<string>());
            devices[1][0].Add("--zombie-mode");
            devices[1][0].Add("2");
            devices[1][0].Add(",");
            devices[1][1].Add("--test");
            devices[1][1].Add("3");
            devices[1][2].Add("--makex");

            var miner = new List<List<string>>();
            miner.Add(new List<string>());
            miner[0].Add("--apiport");
            miner[0].Add("4000");

            var algo = new List<List<string>>();
            algo.Add(new List<string>());
            algo[0].Add("--coin");
            algo[0].Add("ETH");
            

            devices[0].Add(new List<string>());
            string ParseTest(List<List<string>> minerParameters, List<List<string>> algoParameters,List<List<List<string>>> devicesParameters) => Parse(minerParameters, algoParameters, devicesParameters);
            Assert.AreEqual("--apiport 4000 --coin ETH --makex --test 3 --zombie-mode 1,2", ParseTest(miner, algo, devices));
            Assert.AreNotEqual("--apiport 4000 --coin ETH --zombie-mode 1,2", ParseTest(miner, algo, devices));

            miner.Add(new List<string>());
            miner[1].Add("--disablewatchdog");
            miner[1].Add("1");

            algo.Add(new List<string>());
            algo[1].Add("--pool");
            algo[1].Add("nhmp.auto.nicehash.com:443");

            Assert.AreEqual("--apiport 4000 --disablewatchdog 1 --coin ETH --pool nhmp.auto.nicehash.com:443 --makex --test 3 --zombie-mode 1,2", ParseTest(miner, algo, devices));
        }
    }
}
