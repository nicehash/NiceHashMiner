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
using NiceHashMiner.Switching;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMinerLegacy.Tests.Miners
{
    [TestClass]
    public class BMinerTest
    {
        private class TestBMiner : BMiner
        {
            public TestBMiner(AlgorithmType algo)
                : base(algo)
            { }

            public bool ParseBenchLine(string line)
            {
                return BenchmarkParseLine(line);
            }

            public string GetBenchCommandLine(Algorithm algo, int time)
            {
                return BenchmarkCreateCommandLine(algo, time);
            }

            public void FinishBenchmark()
            {
                BenchmarkThreadRoutineFinish();
            }
        }

        private static TestBMiner _zHashBMiner;
        private static TestBMiner _ethashBMiner;

        private static Algorithm _zHashAlgo;
        private static Algorithm _etHashAlgo;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _zHashBMiner = new TestBMiner(AlgorithmType.ZHash);
            _zHashAlgo = new Algorithm(MinerBaseType.BMiner, AlgorithmType.ZHash);
            var dev = new ComputeDevice(0);
            _zHashBMiner.InitBenchmarkSetup(new MiningPair(dev, _zHashAlgo));

            _ethashBMiner = new TestBMiner(AlgorithmType.DaggerHashimoto);
            _etHashAlgo = new Algorithm(MinerBaseType.BMiner, AlgorithmType.DaggerHashimoto);
            _ethashBMiner.InitBenchmarkSetup(new MiningPair(dev, _etHashAlgo));

            NHSmaData.Initialize();
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

        [TestMethod]
        public void ZHashBenchShouldParse()
        {
            var benchLines = BenchOutputZhash.Split('\n');
            var command = _zHashBMiner.GetBenchCommandLine(_zHashAlgo, 60);

            for (var i = 0; i < benchLines.Length; i++)
            {
                var done = _zHashBMiner.ParseBenchLine(benchLines[i]);
                Assert.AreEqual(i == 18, done);
            }

            _zHashBMiner.FinishBenchmark();
            const double expected = (57.47 + 57.80) / 2 * 0.98;
            Assert.AreEqual(expected, _zHashAlgo.BenchmarkSpeed);
        }

        [TestMethod]
        public void EthBenchShouldParse()
        {
            var benchLines = BenchOutputEth.Split('\n');
            var command = _ethashBMiner.GetBenchCommandLine(_etHashAlgo, 20);

            for (var i = 0; i < benchLines.Length; i++)
            {
                var done = _ethashBMiner.ParseBenchLine(benchLines[i]);
                Assert.AreEqual(i == 18, done);
            }

            _ethashBMiner.FinishBenchmark();
            const double expected = 32.3 * 1000000 * 0.9935;
            Assert.AreEqual(expected, _etHashAlgo.BenchmarkSpeed);
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

        private const string BenchOutputZhash = @"[INFO] [2019-01-28T16:10:27-05:00] Bminer: When Crypto-mining Made Fast (v14.0.0-41bef22)
[INFO] [2019-01-28T16:10:27-05:00] Watchdog has started.
[INFO] [2019-01-28T16:10:27-05:00] Starting miner on 1 devices
[INFO] [2019-01-28T16:10:27-05:00] Starting the management API at 127.0.0.1:8085
[INFO] [2019-01-28T16:10:27-05:00] Starting miner on device 0...
[INFO] [2019-01-28T16:10:27-05:00] Connected to zhash.eu.nicehash.com:3369
[INFO] [2019-01-28T16:10:27-05:00] Started miner on device 0
[INFO] [2019-01-28T16:10:28-05:00] Subscribed to stratum server
[INFO] [2019-01-28T16:10:28-05:00] Set nonce to 0708000000000000ce
[INFO] [2019-01-28T16:10:28-05:00] Authorized
[INFO] [2019-01-28T16:10:28-05:00] Set target to 0000000000000000000000000000000000000000000000000000001e1e1e1e00
[INFO] [2019-01-28T16:10:28-05:00] Received new job 000001839cde49dd
[INFO] [2019-01-28T16:10:55-05:00] Received new job 000001839cde578e
[INFO] [2019-01-28T16:10:57-05:00] [GPU 0] Speed: 57.80 Sol/s 29.80 Nonce/s Temp: 52C Fan: 35% Power: 81W 0.71 Sol/J
[INFO] [2019-01-28T16:11:00-05:00] Total 57.80 Sol/s 29.80 Nonce/s Accepted shares 0 Rejected shares 0
[INFO] [2019-01-28T16:11:25-05:00] Accepted share #5
[INFO] [2019-01-28T16:11:25-05:00] Received new job 000001839cde680f
[INFO] [2019-01-28T16:11:27-05:00] [GPU 0] Speed: 57.47 Sol/s 29.80 Nonce/s Temp: 75C Fan: 57% Power: 287W 0.20 Sol/J
[INFO] [2019-01-28T16:11:30-05:00] Total 57.47 Sol/s 29.80 Nonce/s Accepted shares 1 Rejected shares 0
[INFO] [2019-01-28T16:11:32-05:00] Received new job 000001839cde6d8b";

        private const string BenchOutputEth =
            @"[INFO] [2019-01-28T16:42:40-05:00] Bminer: When Crypto-mining Made Fast (v14.0.0-41bef22)
[INFO] [2019-01-28T16:42:40-05:00] Watchdog has started.
[INFO] [2019-01-28T16:42:40-05:00] Starting miner on 1 devices
[INFO] [2019-01-28T16:42:40-05:00] Starting the management API at 127.0.0.1:8085
[INFO] [2019-01-28T16:42:40-05:00] Starting miner on device 0...
[INFO] [2019-01-28T16:42:40-05:00] Connected to daggerhashimoto.eu.nicehash.com:3353
[INFO] [2019-01-28T16:42:40-05:00] Started miner on device 0
[INFO] [2019-01-28T16:42:40-05:00] Subscribed to stratum server
[INFO] [2019-01-28T16:42:40-05:00] Set nonce to 11f558
[INFO] [2019-01-28T16:42:40-05:00] Authorized
[INFO] [2019-01-28T16:42:40-05:00] Set diff to 2.00 (target to 000000007fff8000)
[INFO] [2019-01-28T16:42:40-05:00] Received new job 000000f5473ca2b0
[INFO] [2019-01-28T16:42:40-05:00] [D0] Creating DAG
[INFO] [2019-01-28T16:42:42-05:00] Received new job 000000f5473d20e6
[INFO] [2019-01-28T16:42:52-05:00] Received new job 000000f5473dbab2
[INFO] [2019-01-28T16:42:52-05:00] [D0] DAG has been created
[INFO] [2019-01-28T16:43:02-05:00] Received new job 000000f5473e77ca
[INFO] [2019-01-28T16:43:10-05:00] [GPU 0] Speed: 32.30 MH/s Temp: 70C Fan: 71% Power: 65W 0.50 MH/J
[INFO] [2019-01-28T16:43:10-05:00] Total 32.30 MH/s Accepted shares 0 Rejected shares 0
[INFO] [2019-01-28T16:43:12-05:00] Received new job 000000f5473f23ce
[INFO] [2019-01-28T16:43:22-05:00] Received new job 000000f547401046
[INFO] [2019-01-28T16:43:29-05:00] Accepted share #5
[INFO] [2019-01-28T16:43:32-05:00] Received new job 000000f54740e409
[INFO] [2019-01-28T16:43:33-05:00] Received new job 000000f547416c8b";
    }
}
