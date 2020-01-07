using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.SgminerCommon;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SgminerAvemore
{
    public class SgminerAvemore : SGMinerBase
    {
        public SgminerAvemore(string uuid) : base(uuid)
        { }

        protected override string AlgoName => PluginSupportedAlgorithms.AlgorithmName(_algorithmType);

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
    }
}
