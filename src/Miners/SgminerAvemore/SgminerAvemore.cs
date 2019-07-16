using NHM.Common.Enums;
using MinerPluginToolkitV1.SgminerCommon;
using System;
using System.Linq;
using System.IO;
using NHM.Common;
using System.Threading.Tasks;
using MinerPlugin;
using System.Threading;
using MinerPluginToolkitV1;
using System.Collections.Generic;

namespace SgminerAvemore
{
    public class SgminerAvemore : SGMinerBase
    {
        public SgminerAvemore(string uuid) : base(uuid)
        { }

        protected override string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    // avemore
                    case AlgorithmType.X16R:
                        return "x16r";
                    default:
                        return "";
                }
            }
        }

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<bool> BuildOpenCLKernels(CancellationToken stop)
        {
            // use demo user and disable colorts so we can read from stdout
            var stopAt = DateTime.Now.ToString("HH:mm");
            var commandLine = $"--sched-stop {stopAt} -T " + CreateCommandLine(MinerToolkit.DemoUserBTC);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var device = _miningPairs.Select(kvp => kvp.Device).FirstOrDefault();
            string currentGPU = $"GPU{device.ID}";
            const string hashrateAfter = "(avg):";

            bp.CheckData = (string data) =>
            {
                var containsHashRate = data.Contains(currentGPU) && data.Contains(hashrateAfter);
                if (containsHashRate == false) return new BenchmarkResult { Success = false };

                var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(data, hashrateAfter);
                var hashrate = hashrateFoundPair.Item1;
                var found = hashrateFoundPair.Item2;
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmType, hashrate) },
                    Success = found
                };
            };

            var benchmarkTimeout = TimeSpan.FromMinutes(10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = await MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            // TODO check kernels
            return true;
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            await _semaphore.WaitAsync();
            try
            {
                await BuildOpenCLKernels(stop);
            }
            finally
            {
                _semaphore.Release();
            }

            return await base.StartBenchmark(stop, benchmarkType);
        }


        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            // avemore is differently packed
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", "avermore-windows");
            var binPath = Path.Combine(pluginRootBins, "sgminer.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }
    }
}
