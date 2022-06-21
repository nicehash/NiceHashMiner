using NHM.MinerPluginToolkitV1.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NHM.MinerPluginToolkitV1.CommandLine.MinerExtraParameters;
using static NHM.MinerPluginToolkitV1.CommandLine.MinerConfigManager;

namespace NHM.MinerPluginToolkitV1Test
{
    using MinerParameters = List<List<string>>;
    using AlgorithmParameters = List<List<string>>;
    using DevicesParametersList = List<List<List<string>>>;

    [TestClass]
    public class MinerPluginToolkitV1Tests
    {
        [TestMethod]
        public void TestJsonDeserializer()
        {
            ElpSettings DeserializeTest(string path) => ReadJson(path);
            Assert.IsNotNull(DeserializeTest(@"..\..\..\CommandLine\command_line01.json"));
        }

        [TestMethod]
        public void TestBasicELPs()
        {
            var elps = ReadJson(@"..\..\..\CommandLine\command_line01.json");
            var miner = elps.MinerParameters;
            var algo = elps.AlgorithmParameters;
            var devices = elps.DevicesParametersList;

            string ParseTest(MinerParameters minerParameters, AlgorithmParameters algoParameters, DevicesParametersList devicesParameters) => Parse(minerParameters, algoParameters, devicesParameters);
            Assert.AreEqual("--apiport 4109 --coin ETH --pool daggerhashimoto.net --test 55 --lhr-mode 1,2", ParseTest(miner, algo, devices));
            Assert.AreNotEqual("--apiport 4000 --coin ETH --zombie-mode 1,2", ParseTest(miner, algo, devices));
            Assert.AreNotEqual("--apiport 4000 --disablewatchdog 1 --coin ETH --pool nhmp.auto.nicehash.com:443 --makex --test 3 --zombie-mode 1,2", ParseTest(miner, algo, devices));
        }

        [TestMethod]
        public void TestDeviceAlgoSameELPs()
        {
            var elps = ReadJson(@"..\..\..\CommandLine\command_line02.json");
            var miner = elps.MinerParameters;
            var algo = elps.AlgorithmParameters;
            var devices = elps.DevicesParametersList;

            string ParseTest(MinerParameters minerParameters, AlgorithmParameters algoParameters, DevicesParametersList devicesParameters) => Parse(minerParameters, algoParameters, devicesParameters);
            Assert.AreEqual("--apiport 4109 --coin ETH --pool daggerhashimoto.net --test 55 --lhr-mode 1,2", ParseTest(miner, algo, devices));
        }

        [TestMethod]
        public void TestDeviceMinerSameELPs()
        {
            var elps = ReadJson(@"..\..\..\CommandLine\command_line03.json");
            var miner = elps.MinerParameters;
            var algo = elps.AlgorithmParameters;
            var devices = elps.DevicesParametersList;

            string ParseTest(MinerParameters minerParameters, AlgorithmParameters algoParameters, DevicesParametersList devicesParameters) => Parse(minerParameters, algoParameters, devicesParameters);
            Assert.AreEqual("--apiport 4109 --coin ETH --pool daggerhashimoto.net --test 55 --lhr-mode 1,2", ParseTest(miner, algo, devices));
        }

        [TestMethod]
        public void TestAlgoMinerSameELPs()
        {
            var elps = ReadJson(@"..\..\..\CommandLine\command_line04.json");
            var miner = elps.MinerParameters;
            var algo = elps.AlgorithmParameters;
            var devices = elps.DevicesParametersList;

            string ParseTest(MinerParameters minerParameters, AlgorithmParameters algoParameters, DevicesParametersList devicesParameters) => Parse(minerParameters, algoParameters, devicesParameters);
            Assert.AreEqual("--apiport 4109 --coin ETH --pool daggerhashimoto.net --test 55 --lhr-mode 1,2", ParseTest(miner, algo, devices));
        }

        [TestMethod]
        public void TestDeviceAlgoMinerSameELPs()
        {
            var elps = ReadJson(@"..\..\..\CommandLine\command_line05.json");
            var miner = elps.MinerParameters;
            var algo = elps.AlgorithmParameters;
            var devices = elps.DevicesParametersList;

            string ParseTest(MinerParameters minerParameters, AlgorithmParameters algoParameters, DevicesParametersList devicesParameters) => Parse(minerParameters, algoParameters, devicesParameters);
            Assert.AreEqual("--apiport 4109 --coin ETH --pool daggerhashimoto.net --test 55 --lhr-mode 1,2", ParseTest(miner, algo, devices));
        }

        [TestMethod]
        public void TestComplexELPs()
        {
            var elps = ReadJson(@"..\..\..\CommandLine\command_line06.json");
            var miner = elps.MinerParameters;
            var algo = elps.AlgorithmParameters;
            var devices = elps.DevicesParametersList;

            string ParseTest(MinerParameters minerParameters, AlgorithmParameters algoParameters, DevicesParametersList devicesParameters) => Parse(minerParameters, algoParameters, devicesParameters);
            Assert.AreEqual("--apiport 4109 --coin ETH --pool daggerhashimoto.net --test --disable-watchdog 1 --lhr-mode 1,2 --core 451,808", ParseTest(miner, algo, devices));
        }

        [TestMethod]
        public void TestConfigReader()
        {
            MinerConfig ReadConfig(string path) => ReadConfig(path);
            Assert.IsNotNull(MinerConfigManager.ReadConfig(@"..\..\..\CommandLine\LolMiner-n12j41kwed8eswafk2.json"));
        }

        [TestMethod]
        public void TestConfigWriter()
        {
            var data = ReadConfig(@"..\..\..\CommandLine\LolMiner-n12j41kwed8eswafk2.json");

            void WriteConfig(MinerConfig minerConfig) => WriteConfig(minerConfig);
            data.MinerName = "NBMiner";
            data.MinerUUID = "dfsv56dfas6gha62fgv9fa2vg6";
            MinerConfigManager.WriteConfig(data);
            Assert.IsTrue(true);
        }
    }
}

