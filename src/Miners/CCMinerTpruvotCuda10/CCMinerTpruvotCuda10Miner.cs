using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.CCMinerCommon;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Enums;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using System.Globalization;
using System.IO;
using NiceHashMinerLegacy.Common;

namespace CCMinerTpruvotCuda10
{
    public class CCMinerTpruvotCuda10Miner : MinerBase
    {
        // ccminer can mine only one algorithm at a given time
        private AlgorithmType _algorithmType;
        // command line parts
        private string _devices;
        private string _extraLaunchParameters = "";
        private int _apiPort;

        public CCMinerTpruvotCuda10Miner(string uuid) : base(uuid)
        {}

        private double DevFee
        {
            get
            {
                return 0.0;
            }
        }

        string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.NeoScrypt: return "neoscrypt";
                //case AlgorithmType.Lyra2REv2_UNUSED: return "lyra2v2";
                //case AlgorithmType.Decred: return "decred";
                //case AlgorithmType.Lbry_UNUSED: return "lbry";
                //case AlgorithmType.X11Gost_UNUSED: return "sib";
                //case AlgorithmType.Blake2s: return "blake2s";
                //case AlgorithmType.Sia_UNUSED: return "sia";
                case AlgorithmType.Keccak: return "keccak";
                case AlgorithmType.Skunk: return "skunk";
                //case AlgorithmType.Lyra2z_UNUSED: return "lyra2z";
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.Lyra2REv3: return "lyra2v3";
            }
            // TODO throw exception
            return "";
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            return await CCMinerAPIHelpers.GetMinerStatsDataAsync(_apiPort, _algorithmType, _miningPairs, _logGroup, DevFee);
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
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

            var commandLine = $"--algo={algo} --benchmark --time-limit {benchmarkTime} --devices {_devices} {_extraLaunchParameters}";

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());


            var errorList = new List<string> { "Unknown algo parameter", "Cuda error", "Non-existant CUDA device" };
            var errorFound = false;
            var errorMsg = "";

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
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
                        return new BenchmarkResult { Success = false , ErrorMessage = errorMsg};
                    }
                }

                //return MinerToolkit.TryGetHashrateAfter(data, "Benchmark:"); // TODO add option to read totals
                var hashrateFoundPair =  MinerToolkit.TryGetHashrateAfter(data, "Benchmark:"); // TODO add option to read totals
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;

                if (!found) return new BenchmarkResult { Success = false };

                benchHashes += hashrate;
                benchIters++;

                benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);

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
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
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
