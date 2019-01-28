using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMinerLegacy.Tests.Miners
{
    [TestClass]
    public class BMinerTest
    {
        private static BMiner _zHashBMiner;
        private static BMiner _ethashBMiner;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _zHashBMiner = new BMiner();
            var zhash = new Algorithm(MinerBaseType.BMiner, AlgorithmType.ZHash);
            var dev = new ComputeDevice(0);
            _zHashBMiner.InitBenchmarkSetup(new MiningPair(dev, zhash));

            _ethashBMiner = new BMiner();
            var ethash = new Algorithm(MinerBaseType.BMiner, AlgorithmType.DaggerHashimoto);
            _ethashBMiner.InitBenchmarkSetup(new MiningPair(dev, ethash));
        }

        [TestMethod]
        public void ApiDataShouldParse()
        {
            var hashrate = _zHashBMiner.ParseApi(ApiData1);
            Assert.AreEqual(677.53, hashrate);

            hashrate = _zHashBMiner.ParseApi(ApiData2);
            Assert.AreEqual(64.2 + 56.4, hashrate);

            hashrate = _ethashBMiner.ParseApi(ApiData3);
            Assert.AreEqual(32382279.85 + 31186856.53, hashrate);
        }

        private const string ApiData1 = @"{
  ""devices"": {
    ""0"": {
      ""solvers"": [
        {
          ""algorithm"": ""equihash"",
          ""speed_info"": {
            ""nonce_rate"": 359.27,
            ""solution_rate"": 677.53
          }
        }
      ]
    }
  }
}";

        private const string ApiData2 = @"
{""devices"":{""0"":{""solvers"":[{""algorithm"":""equihash1445"",""speed_info"":{""nonce_rate"":29.6,""solution_rate"":64.2}}]},""1"":{""solvers"":[{""algorithm"":""equihash1445"",""speed_info"":{""nonce_rate"":29.6,""solution_rate"":56.4}}]}}}";

        private const string ApiData3 = @"
{""devices"":{""0"":{""solvers"":[{""algorithm"":""ethash"",""speed_info"":{""hash_rate"":32382279.85}}]},""1"":{""solvers"":[{""algorithm"":""ethash"",""speed_info"":{""hash_rate"":31186856.53}}]}}}";
    }
}
