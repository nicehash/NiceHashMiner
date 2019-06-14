using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Globalization;
using System.IO;
using NiceHashMinerLegacy.Common;
using System.Collections.Generic;

namespace BrokenMiner
{
    internal class BrokenMiner : IMiner
    {
        Task<ApiData> IMiner.GetMinerStatsDataAsync()
        {
            throw new NotImplementedException();
        }

        void IMiner.InitMiningLocationAndUsername(string miningLocation, string username, string password)
        {
            throw new NotImplementedException();
        }

        void IMiner.InitMiningPairs(IEnumerable<MiningPair> miningPairs)
        {
            throw new NotImplementedException();
        }

        Task<BenchmarkResult> IMiner.StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType)
        {
            throw new NotImplementedException();
        }

        void IMiner.StartMining()
        {
            throw new NotImplementedException();
        }

        void IMiner.StopMining()
        {
            throw new NotImplementedException();
        }
    }
}
