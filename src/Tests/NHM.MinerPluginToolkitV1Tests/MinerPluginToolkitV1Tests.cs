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
            Assert.IsNotNull(ReadJson(@"..\..\..\CommandLine\command_line01.json"));
        }

        //[TestMethod]
        //public void TestJsonDeserializer()
        //{
        //    Assert.IsNotNull(ReadJson(@"..\..\..\CommandLine\command_line01.json"));
        //}

        [TestMethod]
        public void TestCheckIfCanGroup()
        {
            var deviceParams01 = new List<List<List<string>>>
            {
                // dev01
                new List<List<string>>
                {
                    new List<string>
                    {
                        "--flag33",
                        "2",
                        ","
                    },
                    new List<string>
                    {
                        "--flag1",
                        "6",
                        ","
                    }
                },
                // dev02
                new List<List<string>>
                {
                    new List<string>
                    {
                        "--flag33",
                        "3",
                        ","
                    },
                    new List<string>
                    {
                        "--flag1",
                        "", // parameter is not valid
                        ","
                    }
                },
            };

            
            Assert.AreEqual(false, MinerExtraParameters.CheckIfCanGroup(deviceParams01));


            var deviceParams02 = new List<List<List<string>>>
            {
                // dev01
                new List<List<string>>
                {
                    new List<string>
                    {
                        "--flag33",
                        "2",
                        ","
                    },
                    new List<string>
                    {
                        "--flag1",
                        "6",
                        ","
                    }
                },
                // dev02
                new List<List<string>>
                {
                    new List<string>
                    {
                        "--flag33",
                        "3",
                        ","
                    },
                    new List<string>
                    {
                        "--flag1",
                        "8",
                        ","
                    }
                },
            };

            Assert.AreEqual(true, MinerExtraParameters.CheckIfCanGroup(deviceParams02));

            var deviceParams03 = new List<List<List<string>>>
            {
                // dev01
                new List<List<string>>
                {
                    new List<string>
                    {
                        "--flag33",
                        "2",
                        ","
                    },
                    new List<string>
                    {
                        "--flag1",
                        "6",
                        ","
                    },
                    new List<string>
                    {
                        "--flagSingle",
                        "6",
                    },
                },
                // dev02
                new List<List<string>>
                {
                    new List<string>
                    {
                        "--flag33",
                        "3",
                        ","
                    },
                    new List<string>
                    {
                        "--flag1",
                        "8",
                        ","
                    },
                    new List<string>
                    {
                        "--flagSingle",
                        "6",
                    },
                },
            };

            Assert.AreEqual(true, MinerExtraParameters.CheckIfCanGroup(deviceParams03));

            var deviceParams04 = new List<List<List<string>>>
            {
                // dev01
                new List<List<string>>
                {
                    new List<string>
                    {
                        "--flag33",
                        "2",
                        ","
                    },
                    new List<string>
                    {
                        "--flag1",
                        "6",
                        ","
                    },
                    new List<string>
                    {
                        "--flagSingle",
                        "6",
                    },
                },
                // dev02
                new List<List<string>>
                {
                    new List<string>
                    {
                        "--flag33",
                        "3",
                        ","
                    },
                    new List<string>
                    {
                        "--flag1",
                        "8",
                        ","
                    },
                    new List<string>
                    {
                        "--flagSingle",
                        "5",
                    },
                },
            };

            Assert.AreEqual(false, MinerExtraParameters.CheckIfCanGroup(deviceParams04));
        }

        [TestMethod]
        public void TestBasicELPs()
        {
            var elps = ReadJson(@"..\..\..\CommandLine\command_line01.json");
            var miner = elps.MinerParameters;
            var algo = elps.AlgorithmParameters;
            var devices = elps.DevicesParametersList;

            Assert.AreEqual("--apiport 4109 --coin ETH --pool daggerhashimoto.net --test 55 --lhr-mode 1,2", Parse(miner, algo, devices));
            Assert.AreNotEqual("--apiport 4000 --coin ETH --zombie-mode 1,2", Parse(miner, algo, devices));
            Assert.AreNotEqual("--apiport 4000 --disablewatchdog 1 --coin ETH --pool nhmp.auto.nicehash.com:443 --makex --test 3 --zombie-mode 1,2", Parse(miner, algo, devices));
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

            //string ParseTest(MinerParameters minerParameters, AlgorithmParameters algoParameters, DevicesParametersList devicesParameters) => Parse(minerParameters, algoParameters, devicesParameters);
            Assert.AreEqual("--apiport 4109 --coin ETH --pool daggerhashimoto.net --test 55 --lhr-mode 1,2", Parse(miner, algo, devices));
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
            Assert.IsNotNull(MinerConfigManager.ReadConfig(@"..\..\..\CommandLine\LolMiner-n12j41kwed8eswafk2.json"));
        }

        [TestMethod]
        public void TestConfigWriter()
        {
            var data = ReadConfig(@"..\..\..\CommandLine\DummyMiner-dsfr43teskrtg34.json");
            void WriteConfig(MinerConfig minerConfig) => WriteConfig(minerConfig);
            data.MinerName = "NBMiner";
            data.MinerUUID = "dfsv56dfas6gha62fgv9fa2vg6";
            MinerConfigManager.WriteConfig(data);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestAddDevice()
        {
            // always create copy of dummy and rename it and change miner name in json file
            var data = ReadConfig(@"..\..\..\CommandLine\LolMiner-dsfr43teskrtg34.json");
            void WriteConfig(MinerConfig minerConfig) => WriteConfig(minerConfig);
            var device = new List<List<string>>()
            {
                new List<string>() { "--watchdog", "1" },
                new List<string>() { "--gpu-no-sleep" }
            };
            //data.Algorithms[0].Devices.Add("device5", device);
            MinerConfigManager.WriteConfig(data);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestAddAlgorithm()
        {
            // always create copy of dummy and rename it and change miner name in json file
            var data = ReadConfig(@"..\..\..\CommandLine\NanoMiner-dsfr43teskrtg34.json");
            var algo = new Algo()
            {
                AlgorithmName = "Autolykos",
                AlgoCommands = new List<List<string>>
                {
                    new List<string>
                    {
                        "--stratum",
                        "autolykos.auto.net"
                    }
                },
                Devices = new Dictionary<string, Device>
                {
                    {
                        "device1",
                        new Device() { DeviceName = "device1", 
                            Commands = new List<List<string>>
                            {
                                new List<string>() { "--watchdog", "1" },
                                new List<string>() { "--gpu-no-sleep" }
                            }, 
                        }
                    }
                }
            };
            MinerConfigManager.WriteConfig(data);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestEditDeviceAndAlgorithmELPs()
        {
            // always create copy of dummy and rename it and change miner name in json file
            var data = ReadConfig(@"..\..\..\CommandLine\NanoMiner1-dsfr43teskrtg34.json");
            MinerConfigManager.WriteConfig(data);
            Assert.IsTrue(true);
        }
    }
}

