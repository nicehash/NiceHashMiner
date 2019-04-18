// TESTNET
#if TESTNET || TESTNETDEV
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Benchmarking.BenchHelpers;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMiner.Plugin;

namespace NiceHashMiner.Benchmarking
{
    public class BenchmarkHandler
    {
        CancellationTokenSource _stopBenchmark;

        private bool _startMiningAfterBenchmark;
        private readonly Queue<Algorithm> _benchmarkAlgorithmQueue;
        private readonly int _benchmarkAlgorithmsCount;
        private readonly List<string> _benchmarkFailedAlgo = new List<string>();
        private readonly BenchmarkPerformanceType _performanceType;

        private readonly PowerHelper _powerHelper;

        public BenchmarkHandler(ComputeDevice device, Queue<Algorithm> algorithms, BenchmarkPerformanceType performance, bool startMiningAfterBenchmark = false)
        {
            _stopBenchmark = new CancellationTokenSource();
            _startMiningAfterBenchmark = startMiningAfterBenchmark;
            Device = device;
            // dirty quick fix
            Device.State = DeviceState.Benchmarking;
            _benchmarkAlgorithmQueue = algorithms;
            _performanceType = performance;

            _benchmarkAlgorithmsCount = _benchmarkAlgorithmQueue.Count;
            _powerHelper = new PowerHelper(device);
        }

        public ComputeDevice Device { get; }

        public void Start()
        {
            // TODO ditch the thread and use Task
            var thread = new Thread(async () => await Benchmark());
            if (thread.Name == null)
                thread.Name = $"dev_{Device.DeviceType}-{Device.ID}_benchmark";
            thread.Start();
        }

        private async Task Benchmark()
        {
            Algorithm currentAlgorithm = null;
            while (_benchmarkAlgorithmQueue.Count > 0)
            {
                try
                {
                    if (_stopBenchmark.IsCancellationRequested) break;
                    currentAlgorithm = _benchmarkAlgorithmQueue.Dequeue();
                    BenchmarkManager.AddToStatusCheck(Device, currentAlgorithm);
                    await BenchmarkAlgorithm(currentAlgorithm);
                    if (_stopBenchmark.IsCancellationRequested) break;
                    BenchmarkManager.StepUpBenchmarkStepProgress();
                    ConfigManager.CommitBenchmarksForDevice(Device);
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint($"BenchmarkHandler-{Device.GetFullName()}", $"Exception {e}");
                }
            }
            currentAlgorithm?.ClearBenchmarkPending();
            // don't show unbenchmarked algos if user canceled
            if (_stopBenchmark.IsCancellationRequested) return;
            BenchmarkManager.EndBenchmarkForDevice(Device, _benchmarkFailedAlgo.Count > 0);
        }

        private async Task BenchmarkAlgorithm(Algorithm algo)
        {
            var currentMiner = MinerFactory.CreateMiner(algo);
            if (currentMiner == null) return;

            BenchmarkManager.AddToStatusCheck(Device, algo);
            if (algo is PluginAlgorithm pAlgo)
            {
                await BenchmarkPluginAlgorithm(pAlgo);
            }
        }

        private async Task BenchmarkPluginAlgorithm(PluginAlgorithm algo)
        {
            var plugin = MinerPluginsManager.GetPluginWithUuid(algo.BaseAlgo.MinerID);
            var miner = plugin.CreateMiner();
            var miningPair = new MinerPlugin.MiningPair
            {
                Device = Device.PluginDevice,
                Algorithm = algo.BaseAlgo
            };
            miner.InitMiningPairs(new List<MinerPlugin.MiningPair> { miningPair });
            // fill service since the benchmark might be online. DemoUser.BTC must be used
            miner.InitMiningLocationAndUsername(StratumService.SelectedServiceLocation, DemoUser.BTC);
            _powerHelper.Start();
            var result = await miner.StartBenchmark(_stopBenchmark.Token, _performanceType);
            var power = _powerHelper.Stop();
            if (result.Success || result.AlgorithmTypeSpeeds?.Count > 0)
            {
                algo.BenchmarkSpeed = result.AlgorithmTypeSpeeds.First().Speed;
                algo.PowerUsage = power;
                if (result.AlgorithmTypeSpeeds.Count > 1)
                {
                    Helpers.ConsolePrint("BenchmarkHandler2", $"Has Second speed {result.AlgorithmTypeSpeeds[1].Speed}");

                }
                // set status to empty string it will return speed
                algo.ClearBenchmarkPending();
                BenchmarkManager.SetCurrentStatus(Device, algo, "");
            }
            else
            {
                // add new failed list
                _benchmarkFailedAlgo.Add(algo.AlgorithmName);
                BenchmarkManager.SetCurrentStatus(Device, algo, result.ErrorMessage);
            }
        }

        public void InvokeQuit()
        {
            _stopBenchmark.Cancel();
        }
    }
}
#endif
