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
    public class GMinerTest
    {
        private class TestGMiner : GMiner
        {
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

        private static TestGMiner _gMiner;
        private static Algorithm _beamAlgo;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _gMiner = new TestGMiner();
            _beamAlgo = new Algorithm(MinerBaseType.GMiner, AlgorithmType.Beam);
            var dev = new ComputeDevice(0);
            var pair = new MiningPair(dev, _beamAlgo);
            _gMiner.InitBenchmarkSetup(pair);
            NHSmaData.Initialize();
        }

        [TestMethod]
        public void BenchmarkShouldParse()
        {
            var benchLines = BenchmarkOutput.Split('\n');

            var command = _gMiner.GetBenchCommandLine(_beamAlgo, 30);

            for (var i = 0; i < benchLines.Length; i++)
            {
                var done = _gMiner.ParseBenchLine(benchLines[i]);
                // Benchmark should be completed on 35th line
                Assert.AreEqual(i == 34, done);
            }

            _gMiner.FinishBenchmark();

            // Minus 2% devfee
            const double expected = 31 * 0.98;

            Assert.AreEqual(expected, _beamAlgo.BenchmarkSpeed);
        }

        private const string BenchmarkOutput = @"+----------------------------------------------------------------+
|                  GMiner Equihash Miner v1.18                   |
+----------------------------------------------------------------+
Algorithm:          Equihash 150,5
Stratum server:     
  host:             stratum://beam.usa.nicehash.com:3370
  user:             3KpWmp49Cdbswr23KhjagNbwqiwcFh8Br2.main
  password:         x
Power calculator:   on
Color output:       on
Watchdog:           off
API:                http://127.0.0.1:8086
Log to file:        off
Selected devices:   GPU1
Temperature limits: 90C 
------------------------------------------------------------------
13:04:24 Connected to beam.usa.nicehash.com:3370
13:04:24 Authorized on Stratum Server
13:04:24 New Job: 1720516766067 Difficulty: 117440512
13:04:24 Started Mining on GPU1: EVGA GeForce GTX 1080 Ti 11GB
13:04:26 Share Accepted
13:04:29 Share Accepted
13:04:32 New Job: 1720516766069 Difficulty: 117440512
13:04:34 Share Accepted
13:04:35 Share Accepted
13:04:43 Share Accepted
13:04:43 New Job: 1720516766077 Difficulty: 117440512
13:04:44 New Job: 1720516766084 Difficulty: 117440512
13:04:46 Share Accepted
13:04:46 Share Accepted
13:04:50 Share Accepted
13:04:54 Temperature: GPU1 67C
13:04:54 Speed: GPU1 31 Sol/s
13:04:54 Power: GPU1 275W 0.11 Sol/W
13:04:54 Total Speed: 31 Sol/s Shares Accepted: 8 Rejected: 0 Power: 275W 0.11 Sol/W
13:04:54 Uptime: 0d 00:00:30 Electricity: 0.002kWh
";
    }
}
