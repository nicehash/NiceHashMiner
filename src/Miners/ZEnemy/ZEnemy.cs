using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.CCMinerCommon;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.Common.Enums;
using static NHM.Common.StratumServiceHelpers;
using System.IO;
using NHM.Common;
using MinerPluginToolkitV1.Configs;

namespace ZEnemy
{
    public class ZEnemy : MinerBase
    {
        private string _devices;

        private string _extraLaunchParameters = "";

        private int _apiPort;

        private AlgorithmType _algorithmType;

        public ZEnemy(string uuid) : base(uuid)
        {}

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.X16R: return "x16r";
            }
            return "";
        }

        private double DevFee
        {
            get
            {
                return 1.0;
            }
        }

        public override Task<ApiData> GetMinerStatsDataAsync()
        {
            return CCMinerAPIHelpers.GetMinerStatsDataAsync(_apiPort, _algorithmType, _miningPairs, _logGroup, DevFee);
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            // determine benchmark time 
            // settup times
            var benchmarkTime = MinerBenchmarkTimeSettings.ParseBenchmarkTime(new List<int> { 40, 60, 120 }, MinerBenchmarkTimeSettings, _miningPairs, benchmarkType); // in seconds

            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url={urlWithPort} --user {MinerToolkit.DemoUserBTC} --devices {_devices} {_extraLaunchParameters}";
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
            var after = $"GPU#"; //if multiple benchmark add gpu cuda id
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                var hasHashRate = data.Contains(after) && data.Contains("-");

                if (!hasHashRate) return new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) }, Success = false };

                var hashrateFoundPair = data.TryGetHashrateAfter("-");
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;

                benchHashes += hashrate;
                benchIters++;

                benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);

                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, benchHashResult) },
                    Success = benchIters >= targetBenchIters
                };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "z-enemy.exe");
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

            // init command line params parts
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
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url={urlWithPort} --user {_username} --api-bind={_apiPort} --devices {_devices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
