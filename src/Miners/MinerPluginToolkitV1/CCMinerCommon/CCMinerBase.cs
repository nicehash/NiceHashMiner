using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MinerPlugin;
using NHM.Common.Enums;
using static NHM.Common.StratumServiceHelpers;
using System.IO;
using NHM.Common;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Configs;

namespace MinerPluginToolkitV1.CCMinerCommon
{
    public abstract class CCMinerBase : MinerBase
    {
        // ccminer can mine only one algorithm at a given time
        protected AlgorithmType _algorithmType;
        // command line parts
        protected string _devices;
        protected string _extraLaunchParameters = "";
        protected int _apiPort;

        protected bool _noTimeLimitOption = false;

        public CCMinerBase(string uuid) : base(uuid)
        {}

        private double DevFee
        {
            get
            {
                return 0.0;
            }
        }

        protected abstract string AlgorithmName(AlgorithmType algorithmType);

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ret = await CCMinerAPIHelpers.GetMinerStatsDataAsync(_apiPort, _algorithmType, _miningPairs, _logGroup, DevFee);
            return ret;
        }

        protected virtual string CreateBenchmarkCommandLine(int benchmarkTime) //this is obsolete?
        {
            var algo = AlgorithmName(_algorithmType);
            return $"--algo={algo} --benchmark --time-limit {benchmarkTime} --devices {_devices} {_extraLaunchParameters}";
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 20, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            var algo = AlgorithmName(_algorithmType);
            var timeLimit = _noTimeLimitOption ? "" : $"--time-limit {benchmarkTime}";
            var commandLine = $"--algo={algo} --benchmark {timeLimit} --devices {_devices} {_extraLaunchParameters}";

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);


            var errorList = new List<string> { "Unknown algo parameter", "Cuda error", "Non-existant CUDA device" };
            var errorFound = false;
            var errorMsg = "";

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
            // TODO fix this tick based system
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));
            // TODO implement fallback average, final benchmark 
            bp.CheckData = (string data) => {
                // check if error
                foreach (var err in errorList)
                {
                    if (data.Contains(err))
                    {
                        bp.TryExit();
                        errorFound = true;
                        errorMsg = data;
                        return new BenchmarkResult { Success = false, ErrorMessage = errorMsg };
                    }
                }

                //return MinerToolkit.TryGetHashrateAfter(data, "Benchmark:"); // TODO add option to read totals
                var hashrateFinalFoundPair = MinerToolkit.TryGetHashrateAfter(data, "Benchmark:"); // TODO add option to read totals
                var hashrateFinal = hashrateFinalFoundPair.Item1;
                var finalFound = hashrateFinalFoundPair.Item2;

                if (finalFound) {
                    return new BenchmarkResult
                    {
                        AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                        Success = true
                    };
                }
                // no final calculate avg speed
                var hashrateTotalFoundPair = MinerToolkit.TryGetHashrateAfter(data, "Total:"); // TODO add option to read totals
                var hashrateTotal = hashrateTotalFoundPair.Item1;
                var totalFound = hashrateTotalFoundPair.Item2;
                if (totalFound)
                {
                    benchHashes += hashrateTotal;
                    benchIters++;
                    benchHashResult = (benchHashes / benchIters); // * (1 - DevFee * 0.01);
                }

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = benchIters >= targetBenchIters
                };
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
            var binPath = Path.Combine(pluginRootBins, "ccminer.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
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
            // all good continue on

            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            _devices = string.Join(",", orderedMiningPairs.Select(p => p.Device.ID));
            if (MinerOptionsPackage != null)
            {
                var ignoreDefaults = MinerOptionsPackage.IgnoreDefaultValueOptions;
                var generalParams = ExtraLaunchParametersParser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions, ignoreDefaults);
                var temperatureParams = ExtraLaunchParametersParser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions, ignoreDefaults);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        protected override string MiningCreateCommandLine()
        {
            // TODO _miningPairs must not be null or count 0
            //if (_miningPairs == null)
            //throw new NotImplementedException();

            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo={algo} --url={url} --user={_username} --api-bind={_apiPort} --devices {_devices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
