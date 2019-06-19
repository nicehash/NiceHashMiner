using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ClaymoreCommon;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClaymoreDual14
{
    public class ClaymoreDual14 : ClaymoreBase, IAfterStartMining
    {
        public ClaymoreDual14(string uuid, Dictionary<string, int> mappedIDs) : base(uuid, mappedIDs)
        {
            _started = DateTime.UtcNow;
        }


        // TODO figure out how to fix API workaround without this started time
        private DateTime _started;

        protected override Dictionary<string, string> GetEnvironmentVariables()
        {
            if (MinerSystemEnvironmentVariables != null)
            {
                return MinerSystemEnvironmentVariables.DefaultSystemEnvironmentVariables;
            }
            return null;
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", "Claymore's Dual Ethereum AMD+NVIDIA GPU Miner v14.7");
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

            var deviceIDs = string.Join("", _miningPairs.Select(pair => $"{pair.Device.DeviceType.ToString()}{pair.Device.ID}"));
            var algoID = IsDual() ? $"{SingleAlgoName}{DualAlgoName}" : SingleAlgoName;
            var logfileName = $"noappend_{deviceIDs}_{algoID}_bench.txt";
            var commandLine = CreateCommandLine(MinerToolkit.DemoUserBTC) + " -dbg 1 -logfile " + logfileName;
            var binPathBinCwdPair = GetBinAndCwdPaths();
            var binPath = binPathBinCwdPair.Item1;
            var binCwd = binPathBinCwdPair.Item2;
            Logger.Info(_logGroup, $"Benchmarking started with command: {commandLine}");
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine, GetEnvironmentVariables());
            bp.CheckData = (string data) =>
            {
                // we can't read from stdout or stderr, read from logs later
                return new BenchmarkResult();
            };


            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = await MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            // look for log file and parse that
            try
            {
                var benchHashesFirstSum = 0d;
                var benchItersFirst = 0;
                var benchHashesSecondSum = 0d;
                var benchItersSecond = 0;

                //var afterSingle = $"{SingleAlgoName.ToUpper()} - Total Speed:";
                var firstAlgoLineMustContain = SingleAlgoName.ToUpper();
                var secondAlgoLineMustContain = DualAlgoName.ToUpper();
                var singleLineMustContain = SingleAlgoName.ToUpper();
                var gpuAfter = $"GPU0"; // for single device we always have GPU0
                var afterDual = $"{DualAlgoName.ToUpper()}: {DualAlgoName.ToUpper()} - Total Speed:";

                var logFullPath = Path.Combine(binCwd, logfileName);
                var lines = File.ReadLines(logFullPath);
                foreach (var line in lines)
                {
                    var hashrateFoundPair = MinerToolkit.TryGetHashrateAfter(line, gpuAfter);
                    var hashrate = hashrateFoundPair.Item1;
                    var found = hashrateFoundPair.Item2;
                    if (!found || hashrate == 0) continue;

                    if (line.Contains(firstAlgoLineMustContain))
                    {
                        benchHashesFirstSum += hashrate;
                        benchItersFirst++;
                    }
                    else if(line.Contains(secondAlgoLineMustContain))
                    {
                        benchHashesSecondSum += hashrate;
                        benchItersSecond++;
                    }
                }
                var benchHashResultFirst = benchItersFirst == 0 ? 0d : benchHashesFirstSum / benchItersFirst;
                var benchHashResultSecond = benchItersSecond == 0 ? 0d : benchHashesSecondSum / benchItersSecond;
                var success = benchHashResultFirst > 0d;
                var speeds = new List<AlgorithmTypeSpeedPair> { new AlgorithmTypeSpeedPair(_algorithmFirstType, benchHashResultFirst * (1 - DevFee * 0.01)) };
                if (IsDual())
                {
                    speeds.Add(new AlgorithmTypeSpeedPair(_algorithmSecondType, benchHashResultSecond * (1 - DualDevFee * 0.01)));
                }
                // return
                return new BenchmarkResult
                {
                    AlgorithmTypeSpeeds = speeds,
                    Success = success
                };
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Benchmarking failed: {e.Message}");
            }
            return t;
        }

        public void AfterStartMining()
        {
            _started = DateTime.UtcNow;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiData();
            var elapsedSeconds = DateTime.UtcNow.Subtract(_started).Seconds;
            if (elapsedSeconds < 15)
            {
                return api;
            }

            var miningDevices = _orderedMiningPairs.Select(pair => pair.Device).ToList();
            var algorithmTypes = IsDual() ? new AlgorithmType[] { _algorithmFirstType, _algorithmSecondType } : new AlgorithmType[] { _algorithmFirstType };
            // multiply dagger API data 
            var ad = await ClaymoreAPIHelpers.GetMinerStatsDataAsync(_apiPort, miningDevices, _logGroup, DevFee, DualDevFee, algorithmTypes);
            var totalCount = ad.AlgorithmSpeedsTotal?.Count ?? 0;
            for (var i = 0; i < totalCount; i++)
            {
                ad.AlgorithmSpeedsTotal[i].Speed *= 1000; // speed is in khs
            }
            var keys = ad.AlgorithmSpeedsPerDevice.Keys.ToArray();
            foreach (var key in keys)
            {
                var devSpeedtotalCount = (ad.AlgorithmSpeedsPerDevice[key])?.Count ?? 0;
                for (var i = 0; i < devSpeedtotalCount; i++)
                {
                    ad.AlgorithmSpeedsPerDevice[key][i].Speed *= 1000; // speed is in khs
                }
            }
            return ad;
        }
    }
}
