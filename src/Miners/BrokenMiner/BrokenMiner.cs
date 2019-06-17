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
        async Task<ApiData> IMiner.GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            api.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(AlgorithmType.ZHash, 1) };
            api.PowerUsageTotal = 1;
            var speedDev = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
            speedDev.Add("GPU-d97bdb7c-4155-9124-31b7-4743e16d3ac0", new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(AlgorithmType.ZHash, 1) });
            api.AlgorithmSpeedsPerDevice = speedDev;
            var powerDev = new Dictionary<string, int>();
            powerDev.Add("GPU-d97bdb7c-4155-9124-31b7-4743e16d3ac0", 1);
            api.PowerUsagePerDevice = powerDev;
            return GetValueOrErrorSettings.GetValueOrError("GetMinerStatsDataAsync", api);
        }

        void IMiner.InitMiningLocationAndUsername(string miningLocation, string username, string password) => GetValueOrErrorSettings.SetError("InitMiningLocationAndUsername");

        void IMiner.InitMiningPairs(IEnumerable<MiningPair> miningPairs) => GetValueOrErrorSettings.SetError("InitMiningPairs");


        async Task<BenchmarkResult> IMiner.StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType)
        {
            var bp = new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(AlgorithmType.ZHash, 12) }, Success = true };
            return GetValueOrErrorSettings.GetValueOrError("StartBenchmark", bp);
        }

        void IMiner.StartMining() => GetValueOrErrorSettings.SetError("StartMining");

        void IMiner.StopMining() => GetValueOrErrorSettings.SetError("StopMining");
    }
}
