using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Benchmarking.BenchHelpers;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NHM.Common;
using NHM.Common.Enums;
using NiceHashMiner.Mining.Plugins;

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

        public ComputeDevice Device { get; }

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

        public void Start()
        {
            Task.Run(async () => await Benchmark());
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
                    currentAlgorithm.IsReBenchmark = false;
                    BenchmarkManager.StepUpBenchmarkStepProgress();
                    ConfigManager.CommitBenchmarksForDevice(Device);
                }
                catch (Exception e)
                {
                    Logger.Error("BenchmarkHandler", $"Exception occurred in benchmark task: {e.Message}");
                }
            }
            currentAlgorithm?.ClearBenchmarkPending();
            var cancel = _stopBenchmark.IsCancellationRequested;
            // don't show unbenchmarked algos if user canceled
            var showFailed = _benchmarkFailedAlgo.Count > 0 && !cancel;
            var startMining = _startMiningAfterBenchmark && !cancel;
            BenchmarkManager.EndBenchmarkForDevice(Device, showFailed, startMining);
        }

        private async Task BenchmarkAlgorithm(Algorithm algo)
        {
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
                Device = Device.BaseDevice,
                Algorithm = algo.BaseAlgo
            };
            // check ethlargement
            var miningPairs = new List<MinerPlugin.MiningPair> { miningPair };
            EthlargementIntegratedPlugin.Instance.Start(miningPairs);
            miner.InitMiningPairs(miningPairs);
            // fill service since the benchmark might be online. DemoUser.BTC must be used
            miner.InitMiningLocationAndUsername(StratumService.SelectedServiceLocation, DemoUser.BTC);
            _powerHelper.Start();
            var result = await miner.StartBenchmark(_stopBenchmark.Token, _performanceType);
            //EthlargementIntegratedPlugin.Instance.Stop(miningPairs); // TODO check stopping
            var power = _powerHelper.Stop();
            if (result.Success || result.AlgorithmTypeSpeeds?.Count > 0)
            {
                var ids = result.AlgorithmTypeSpeeds.Select(ats => ats.AlgorithmType).ToList();
                var speeds = result.AlgorithmTypeSpeeds.Select(ats => ats.Speed).ToList();
                algo.Speeds = speeds;
                algo.PowerUsage = power;
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
