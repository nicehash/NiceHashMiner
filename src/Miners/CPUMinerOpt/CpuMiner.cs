using MinerPlugin;
using MinerPluginToolkitV1;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CpuMinerOpt
{
#warning This implementation doesn't support mutiple CPU sockets
    public class CpuMiner : MinerBase
    {
        // command line parts
        //private ulong _affinityMask = 0; implement and use after MinerPluginToolkit supports start mining override
        private int _apiPort;
        private double DevFee = 0d;

        public CpuMiner(string uuid) : base(uuid)
        {
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var summaryApiResult = await ApiDataHelpers.GetApiDataAsync(_apiPort, ApiDataHelpers.GetHttpRequestNhmAgentString("summary"), _logGroup);
            double totalSpeed = 0;
            int totalPower = 0;
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<AlgorithmTypeSpeedPair>>();
            var perDevicePowerInfo = new Dictionary<string, int>();

            if (!string.IsNullOrEmpty(summaryApiResult))
            {
                try
                {
                    var summaryOptvals = summaryApiResult.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var optvalPairs in summaryOptvals)
                    {
                        var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pair.Length != 2) continue;
                        if (pair[0] == "KHS")
                        {
                            var currentSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
                            totalSpeed += currentSpeed;
                            perDeviceSpeedInfo.Add(_miningPairs.FirstOrDefault()?.Device.UUID, new List<AlgorithmTypeSpeedPair>() { new AlgorithmTypeSpeedPair(_algorithmType, currentSpeed * (1 - DevFee * 0.01)) });
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                }
            }
            var ad = new ApiData();
            ad.AlgorithmSpeedsTotal = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, totalSpeed) };
            ad.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            ad.PowerUsageTotal = totalPower;
            ad.PowerUsagePerDevice = perDevicePowerInfo;

            return ad;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {

            var benchmarkTime = MinerPluginToolkitV1.Configs.MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 20, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            var algo = PluginSupportedAlgorithms.AlgorithmName(_algorithmType);
            var commandLine = $"--algo={algo} --benchmark --time-limit {benchmarkTime} {_extraLaunchParameters}";

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            double benchHashesSum = 0;
            double benchHashResult = 0;
            int benchIters = 0;
            var foundBench = false;

            bp.CheckData = (string data) => {
                if(!data.Contains("Total:")) return new BenchmarkResult{AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, 0) }, Success = false };
                var hashrateFoundPairAvg = MinerToolkit.TryGetHashrateAfter(data, "H, ");
                var hashrateAvg = hashrateFoundPairAvg.Item1;
                var foundAvg = hashrateFoundPairAvg.Item2;
                if (!foundAvg)
                {
                    var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, "Benchmark: ");
                    var hashrate = hashrateFoundPair.Item1;
                    foundBench = hashrateFoundPair.Item2;
                    if (foundBench && hashrate != 0)
                    {
                        benchHashResult = hashrate * (1 - DevFee * 0.01);

                        return new BenchmarkResult
                        {
                            AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                            Success = benchHashResult != 0
                        };
                    }                
                }

                // sum and return
                benchHashesSum += hashrateAvg;
                benchIters++;

                benchHashResult = (benchHashesSum / benchIters) * (1 - DevFee * 0.01);

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = foundBench
                };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override string MiningCreateCommandLine()
        {
            return CreateCommandLine(_username);
        }

        private string CreateCommandLine(string username)
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = PluginSupportedAlgorithms.AlgorithmName(_algorithmType);

            var commandLine = $"--algo={algo} --url={url} --user={username} --api-bind={_apiPort} {_extraLaunchParameters}";
            return commandLine;
        }

        protected override void Init()
        {
            var cpuDevice = _miningPairs.Select(kvp => kvp.Device).FirstOrDefault();
            //if (cpuDevice is CPUDevice cpu)
            //{
            //    // TODO affinity mask stuff
            //    //_affinityMask
            //}
        }

        //public void AfterStartMining()
        //{
        //    int pid = _miningProcess?.Id  ?? - 1;
        //    if (_affinityMask != 0 && pid != -1)
        //    {
        //        var okMsg = ProcessHelpers.AdjustAffinity(pid, _affinityMask);
        //        Logger.Info(_logGroup, $"Adjust Affinity returned: {okMsg.Item2}");
        //    }
        //}
    }
}
