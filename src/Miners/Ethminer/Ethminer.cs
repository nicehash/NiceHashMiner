using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ClaymoreCommon;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethminer
{
    public class Ethminer : MinerBase, IAfterStartMining
    {
        private string _cudaDevices = "";
        private string _openCLDevices = "";
        private int _apiPort;
        private DateTime _started;

        public Ethminer(string uuid) : base(uuid)
        {
            _started = DateTime.UtcNow;
        }

#warning API doesn't work Read stream blocks
        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var elapsedSeconds = DateTime.UtcNow.Subtract(_started).Seconds;
            if (elapsedSeconds < 5)
            {
                return new ApiData();
            }
            var miningDevices = _miningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = new AlgorithmType[] { _algorithmType };
            // multiply dagger API data 
            var ad = await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, 0.0, 0.0, algorithmTypes);
            var totalCount = ad.AlgorithmSpeedsTotal?.Count ?? 0;
            for (var i = 0; i < totalCount; i++)
            {
                ad.AlgorithmSpeedsTotal[i].Speed *= 1000; // speed is in khs
            }
            var keys = ad.AlgorithmSpeedsPerDevice.Keys.ToArray();
            foreach (var key in keys)
            {
                var devSpeedtotalCount = (ad.AlgorithmSpeedsPerDevice[key])?.Count ?? 0;
                for (var i = 0; i < devSpeedtotalCount; i++)
                {
                    ad.AlgorithmSpeedsPerDevice[key][i].Speed *= 1000; // speed is in khs
                }
            }
            return ad;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 30, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds
            
            var commandLine = $"--benchmark 200 {_cudaDevices} {_openCLDevices} {_extraLaunchParameters}";
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..

            var lookFor = _miningPairs.Any(mp => mp.Device.DeviceType == DeviceType.AMD) ? "cl0" : "cu0";

            bp.CheckData = (string data) =>
            {
                double hashrate = 0d;
                bool found = false;
                for (int i = 0; i < 10; i++)
                {
                    var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, $"A{i}");
                    hashrate = hashrateFoundPair.Item1;
                    found = hashrateFoundPair.Item2;
                    if (found)
                    {
                        break;
                    }
                }

                if (hashrate > 0 && found && data.Contains(lookFor))
                {
                    benchHashes += hashrate;
                    benchIters++;

                    benchHashResult = (benchHashes / benchIters);
                }
                
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair>{ new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult)},
                    Success = false
                };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override void Init()
        {
            var cudaDevs = _miningPairs.Where(mp => mp.Device.DeviceType == DeviceType.NVIDIA).Select(mp => mp.Device.ID);
            var oclDevs = _miningPairs.Where(mp => mp.Device.DeviceType == DeviceType.AMD).Select(mp => mp.Device.ID);
            if (cudaDevs.Count() > 0)
            {
                _cudaDevices = $"--cu-devices {string.Join(" ", cudaDevs)}";
            }

            if (oclDevs.Count() > 0)
            {
                _openCLDevices = $"--cl-devices {string.Join(" ", oclDevs)}";
            }
        }

        public void AfterStartMining()
        {
            _started = DateTime.UtcNow;
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.NONE);
            var poolWithUsername = $"stratum2+tcp://{_username}@{urlWithPort}";
            // TODO add readonly API port
            var commandLine = $"--pool {poolWithUsername} --api-bind 127.0.0.1:{_apiPort} {_cudaDevices} {_openCLDevices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
