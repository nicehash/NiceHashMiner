using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using NiceHashMinerLegacy.Common.Device;
using System.Collections.Generic;
using System.Globalization;
using NiceHashMinerLegacy.Common;
using System.IO;

namespace CPUMinerBase
{
    public class CpuMiner : MinerBase, IAfterStartMining
    {
        // cpuminer can mine only one algorithm at a given time
        private AlgorithmType _algorithmType;

        // command line parts
        private ulong _affinityMask = 0;
        private string _extraLaunchParameters = "";
        private int _apiPort;

        public CpuMiner(string uuid) : base(uuid)
        {}

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Lyra2Z: return "lyra2z";
            }
            // TODO throw exception
            return "";
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var summaryApiResult = await ApiDataHelpers.GetApiDataAsync(_apiPort, ApiDataHelpers.GetHttpRequestNhmAgentString("summary"), _logGroup);
            double totalSpeed = 0;
            int totalPower = 0;
            if (!string.IsNullOrEmpty(summaryApiResult))
            {
                // TODO return empty
                try
                {
                    var summaryOptvals = summaryApiResult.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var optvalPairs in summaryOptvals)
                    {
                        var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pair.Length != 2) continue;
                        if (pair[0] == "KHS")
                        {
                            totalSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
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
            ad.PowerUsageTotal = totalPower;
            // cpuMiner is single device so no need for API

            return ad;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = 20; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 20;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    break;
            }

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo={algo} --benchmark --time-limit {benchmarkTime} {_extraLaunchParameters}";

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());
            // TODO benchmark process add after benchmark

            // make sure this is culture invariant
            // TODO implement fallback average, final benchmark 
            bp.CheckData = (string data) => {
                if (double.TryParse(data, out var parsedSpeed))
                    return new BenchmarkResult {AlgorithmTypeSpeeds= new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, parsedSpeed) } ,Success = true };
                return new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, 0d) }, Success = false };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 5);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "cpuminer.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo={algo} --url={url} --user={_username} --api-bind={_apiPort} {_extraLaunchParameters}";
            return commandLine;
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok)
            {
                Logger.Info(_logGroup, "Initialization of miner failed. Algorithm not found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }

            var cpuDevice = _miningPairs.Select(kvp => kvp.Device).FirstOrDefault();
            if (cpuDevice is CPUDevice cpu)
            {
                // TODO affinity mask stuff
                //_affinityMask
            }
            var miningPairsList = _miningPairs.ToList();
            if (MinerOptionsPackage != null)
            {
                var generalParams = Parser.Parse(miningPairsList, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(miningPairsList, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public void AfterStartMining()
        {
            int pid = _miningProcess?.Id  ?? - 1;
            // TODO C# can have this shorter
            if (_affinityMask != 0 && pid != -1)
            {
                var okMsg = ProcessHelpers.AdjustAffinity(pid, _affinityMask);
                Logger.Info(_logGroup, $"Adjust Affinity returned: {okMsg.Item2}");
            }
        }
    }
}
