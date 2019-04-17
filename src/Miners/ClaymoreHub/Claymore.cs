using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ClaymoreCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClaymoreHub
{
    public class Claymore : ClaymoreBase
    {
        public Claymore(string uuid) : base(uuid)
        {
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "EthDcrMiner64.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
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

            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC);
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());

            var benchHashesFirst = 0d;
            var benchIters = 0;
            var benchHashResultFirst = 0d;
            var benchHashesSecond = 0d;
            var benchHashResultSecond = 0d;
            //var afterSingle = $"{SingleAlgoName.ToUpper()} - Total Speed:";
            var afterSingle = $"GPU{_devices}";
            var afterDual = $"{DualAlgoName.ToUpper()} - Total Speed:";
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                // if (_algorithmDualType == AlgorithmType.NONE)
                // {
                Console.WriteLine("Data za benchmark: ", data);
                var hashrateFoundPairFirst = data.TryGetHashrateAfter(afterSingle);
                var hashrateFirst = hashrateFoundPairFirst.Item1;
                var foundFirst = hashrateFoundPairFirst.Item2;
                benchHashesFirst += hashrateFirst;
                benchIters++;

                benchHashResultFirst = (benchHashesFirst / benchIters) * (1 - DevFee * 0.01);
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmSingleType, benchHashResultFirst) },
                    Success = benchIters >= targetBenchIters
                };
                /*} else
                {
                    var hashrateFoundPairFirst = data.TryGetHashrateAfter(afterSingle);
                    var hashrateFirst = hashrateFoundPairFirst.Item1;
                    var foundFirst = hashrateFoundPairFirst.Item2;
                    benchHashesFirst += hashrateFirst;
                    benchIters++;

                    benchHashResultFirst = (benchHashesFirst / benchIters) * (1 - DevFee * 0.01);

                    var hashrateFoundPairSecond = data.TryGetHashrateAfter(afterDual);
                    var hashrateSecond = hashrateFoundPairSecond.Item1;
                    var foundSecond = hashrateFoundPairSecond.Item2;
                    benchHashesSecond += hashrateSecond;
                    benchIters++;

                    benchHashResultSecond = (benchHashesSecond / benchIters) * (1 - DevFee * 0.01);
                    return new BenchmarkResult
                    {
                        AlgorithmTypeSpeeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmSingleType, benchHashResultFirst), new AlgorithmTypeSpeedPair(_algorithmDualType, benchHashResultSecond) },
                        Success = benchIters >= targetBenchIters
                    };
                }*/
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        public override Task<ApiData> GetMinerStatsDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}
