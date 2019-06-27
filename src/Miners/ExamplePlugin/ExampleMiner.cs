using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Example
{
    public class ExampleMiner : IMiner
    {
        public Task<ApiData> GetMinerStatsDataAsync()
        {
            throw new NotImplementedException();
        }

        public void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x")
        {
            throw new NotImplementedException();
        }

        public void InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            throw new NotImplementedException();
        }

        public Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            throw new NotImplementedException();
        }

        public void StartMining()
        {
            throw new NotImplementedException();
        }

        public void StopMining()
        {
            throw new NotImplementedException();
        }
    }
}
