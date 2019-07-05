using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.IO;
using NiceHashMinerLegacy.Common;
using System.Collections.Generic;

namespace CryptoDredge
{
    public class CryptoDredge : MinerBase
    {
        private string _devices;
        private string _extraLaunchParameters = "";
        private int _apiPort;
        
        private AlgorithmType _algorithmType;

        public CryptoDredge(string uuid) : base(uuid)
        {}

        protected virtual string AlgorithmName(AlgorithmType algorithmType)
        {
            switch (algorithmType)
            {
                case AlgorithmType.Lyra2REv3: return "lyra2v3";
                case AlgorithmType.X16R: return "x16r";
                case AlgorithmType.MTP: return "mtp";
                default: return "";
            }
        }

        private double DevFee
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.Lyra2REv3:
                    case AlgorithmType.X16R: return 1.0;
                    case AlgorithmType.MTP: return 2.0;
                    default: return 1.0;
                }
            }
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            throw new NotImplementedException();
        }

        private struct HashFound
        {
            public double hashrate;
            public bool found;
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var numOfGpus = 2; //MUST BE SET CORRECTLY OTHERWISE BENCHMARKING WON't WORK (all cards are combined currently)
            var avgRet = 0.0;
            var counter = 0;
            var maxCheck = 0;
            var after = "Avr";

            // determine benchmark time 
            // settup times
            var benchmarkTime = 20; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 20;
                    maxCheck = 1 * numOfGpus;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 60;
                    maxCheck = 2 * numOfGpus;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 120;
                    maxCheck = 3 * numOfGpus;
                    break;
            }

            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url {url} --user {_username} --api-bind 127.0.0.1:{_apiPort} --no-watchdog --device {_devices} {_extraLaunchParameters}";

            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            bp.CheckData = (string data) =>
            {
                var s = data;
                var ret = new HashFound();
                ret.hashrate = default(double);
                ret.found = false;

                if (s.Contains(after))
                {
                    var afterString = s.GetStringAfter(after).ToLower();
                    var afterStringArray = afterString.Split(' ');
                    var hashRate = afterStringArray[1];
                    var numString = new string(hashRate
                        .ToCharArray()
                        .SkipWhile(c => !char.IsDigit(c))
                        .TakeWhile(c => char.IsDigit(c) || c == ',')
                        .ToArray());

                    numString.Replace(',', '.');
                    if (!double.TryParse(numString, NumberStyles.Float, CultureInfo.InvariantCulture, out var hash))
                    {
                        return new BenchmarkResult {AlgorithmTypeSpeeds= new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, ret.hashrate) }, Success = ret.found};
                    }

                    counter++;
                    if (hashRate.Contains("kh"))
                        avgRet += hash * 1000;
                    else if (hashRate.Contains("mh"))
                        avgRet += hash * 1000000;
                    else if (hashRate.Contains("gh"))
                        avgRet += hash * 1000000000;
                    else
                        avgRet += hash;

                    maxCheck--;
                    if (maxCheck == 0)
                    {
                        ret.hashrate = avgRet / counter;
                        ret.found = true;
                    }
                }
                return new BenchmarkResult { AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, ret.hashrate) }, Success = ret.found };
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(300);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", "CryptoDredge_0.20.1");
            var binPath = Path.Combine(pluginRootBins, "CryptoDredge.exe");
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
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var url = GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url {url} --user {_username} --api-bind 127.0.0.1:{_apiPort} --device {_devices} --no-watchdog {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
