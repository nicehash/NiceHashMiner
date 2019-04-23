using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ClaymoreCommon;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Phoenix
{
    public class Phoenix : ClaymoreBase
    {
        public Phoenix(string uuid) : base(uuid)
        {
        }

        public new double DevFee
        {
            get
            {
                return 0.65;
            }
        }

        protected override void Init()
        {
            var singleType = MinerToolkit.GetAlgorithmSingleType(_miningPairs);
            _algorithmFirstType = singleType.Item1;
            bool ok = singleType.Item2;
            if (!ok)
            {
                Logger.Info(_logGroup, "Initialization of miner failed. Algorithm not found!");
                throw new InvalidOperationException("Invalid mining initialization");
            }

            var dualType = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            _algorithmSecondType = dualType.Item1;
            ok = dualType.Item2;
            if (!ok) _algorithmSecondType = AlgorithmType.NONE;
            // all good continue on

            // Order pairs and parse ELP
            _orderedMiningPairs = _miningPairs.ToList();
            _orderedMiningPairs.Sort((a, b) => {
                var aGpu = a.Device as IGpuDevice;
                var bGpu = b.Device as IGpuDevice;
                return aGpu.PCIeBusID.CompareTo(bGpu.PCIeBusID);
            });
            //_devices = string.Join("", _orderedMiningPairs.Select(p => _mappedIDs[p.Device.]));
            _devices = string.Join("", _orderedMiningPairs.Select(p => {
                var pGpu = p.Device as IGpuDevice;
                return Shared.MappedCudaIds[pGpu.UUID];
            }));
            var deviceTypes = _orderedMiningPairs.Select(pair => pair.Device.DeviceType);
            _platform = $"{ClaymoreHelpers.GetPlatformIDForType(deviceTypes)}";

            if (MinerOptionsPackage != null)
            {
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(_orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(_orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var miningDevices = _orderedMiningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = new AlgorithmType[] { _algorithmFirstType };
            return await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, algorithmTypes, _logGroup);
        }

        public async override Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var benchmarkTime = 90; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 90;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 180;
                    break;
            }

            // local benchmark
            // TODO hardcoded epoch
            var commandLine = $"-di {_devices} -platform {_platform} {_extraLaunchParameters} -benchmark 200 -wd 0";
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            Logger.Debug(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                var hashrateFoundPairFirst = data.TryGetHashrateAfter("Eth speed:");
                var hashrateFirst = hashrateFoundPairFirst.Item1;
                var foundFirst = hashrateFoundPairFirst.Item2;

                if (foundFirst)
                {
                    benchHashes += hashrateFirst;
                    benchIters++;

                    benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);
                }

                
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmFirstType, benchHashResult) }
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
            var binPath = Path.Combine(pluginRootBins, "PhoenixMiner.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }
    }
}
