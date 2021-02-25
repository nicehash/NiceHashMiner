using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Configs;
using NHMCore.Mining.Plugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Mining.Benchmarking
{
    class BenchmarkingComputeDeviceHandler
    {
        #region STATIC

        private static ConcurrentDictionary<ComputeDevice, BenchmarkingComputeDeviceHandler> BenchmarkingHandlers { get; set; } = new ConcurrentDictionary<ComputeDevice, BenchmarkingComputeDeviceHandler>();

        internal static void BenchmarkDeviceAlgorithms(ComputeDevice computeDevice, IEnumerable<AlgorithmContainer> algorithmContainers, BenchmarkPerformanceType performance, bool startMiningAfterBenchmark = false)
        {
            if (algorithmContainers.Count() == 0) return; // no algorithms skip

            if (BenchmarkingHandlers.TryGetValue(computeDevice, out var benchmarkHandler))
            {
                // update algorithms
                Logger.Debug("BenchmarkingComputeDeviceHandler", $"Already benchmarking");
                // TODO UPDATE algorithms and skip creating new task
                return;
            }
            // create new
            Task.Run(() => CreateBenchmarkingTask(computeDevice, algorithmContainers, performance, startMiningAfterBenchmark));
        }

        private static async Task CreateBenchmarkingTask(ComputeDevice computeDevice, IEnumerable<AlgorithmContainer> algorithmContainers, BenchmarkPerformanceType performance, bool startMiningAfterBenchmark = false)
        {
            // create new
            var newBenchmarkHandler = new BenchmarkingComputeDeviceHandler(computeDevice)
            {
                PerformanceType = performance,
                StartMiningAfterBenchmark = startMiningAfterBenchmark
            };
            newBenchmarkHandler.AppendForBenchmarking(algorithmContainers);
            newBenchmarkHandler.BenchmarkTask = newBenchmarkHandler.Benchmark();
            await newBenchmarkHandler.BenchmarkTask;
        }

        internal static Task StopBenchmarkingDevice(ComputeDevice computeDevice)
        {
            if (BenchmarkingHandlers.TryGetValue(computeDevice, out var benchmarkHandler))
            {
                benchmarkHandler.StopBenchmark();
                // return stopped task
                return benchmarkHandler.BenchmarkTask;
            }
            return null;
        }

        internal static Task StopBenchmarkingAllDevices()
        {
            var removeAllKeys = BenchmarkingHandlers.Keys.ToArray();
            var stoppedTasks = new List<Task>();
            foreach (var computeDevice in removeAllKeys)
            {
                var stoppedTask = StopBenchmarkingDevice(computeDevice);
                if (stoppedTask != null) stoppedTasks.Add(stoppedTask);
            }
            return Task.WhenAll(stoppedTasks);
        }

        internal static bool IsBenchmarking => BenchmarkingHandlers.Count > 0;

        #endregion STATIC

        //private object _lock = new object();
        public Task BenchmarkTask { get; private set; }
        public ComputeDevice Device { get; }
        private readonly ConcurrentDictionary<string, AlgorithmContainer> _benchmarkAlgorithms = new ConcurrentDictionary<string, AlgorithmContainer>();

        CancellationTokenSource _stopBenchmark;
        CancellationTokenSource _stopCurrentAlgorithmBenchmark;

        // TODO 
        public bool StartMiningAfterBenchmark { get; set; } = false;
        public BenchmarkPerformanceType PerformanceType { get; set; } = BenchmarkPerformanceType.Standard;
        private readonly List<string> _benchmarkFailedAlgo = new List<string>();

        private BenchmarkingComputeDeviceHandler(ComputeDevice device)
        {
            Device = device;
        }


        // APPEND, REMOVE, UPDATE (on plugin updates)
        internal bool AppendForBenchmarking(IEnumerable<AlgorithmContainer> algorithmContainers)
        {
            bool allAdded = true;
            foreach (var algo in algorithmContainers)
            {
                var currentAdded = _benchmarkAlgorithms.TryAdd(algo.AlgorithmStringID, algo);
                if (currentAdded) algo.SetBenchmarkPending();
                // TODO make sure not to add the already same algorihtms, use UPDATE instead
                allAdded &= currentAdded;
            }
            return allAdded;
        }

        // TODO IMPLEMENT REMOVE

        //public bool RemoveFromBenchmarking(params string[] removeKeys)
        //{
        //    bool allRemoved = true;
        //    foreach (var key in removeKeys)
        //    {
        //        allRemoved &= _benchmarkAlgorithms.TryRemove(key, out var _);
        //    }
        //    return allRemoved;
        //}

        //// TODO on plugin updates update algorithms and stop benchmarking if the current active algorithm is in the update array
        //public bool UpdateForBenchmarking(params AlgorithmContainer[] algorithmContainers)
        //{
        //    throw new NotImplementedException();
        //    StopCurrentAlgorithmBenchmark();
        //}

        public async Task Benchmark()
        {
            bool showFailed = false;
            bool startMining = false;
            try
            {
                BenchmarkingHandlers.TryAdd(Device, this);
                // whole benchmark scope
                using (_stopBenchmark = CancellationTokenSource.CreateLinkedTokenSource(ApplicationStateManager.ExitApplication.Token))
                {
                    // Until container is not empty
                    while (!_benchmarkAlgorithms.IsEmpty)
                    {
                        if (_stopBenchmark.IsCancellationRequested)
                        {
                            break;
                        }
                        // per algo benchmark scope
                        using (_stopCurrentAlgorithmBenchmark = CancellationTokenSource.CreateLinkedTokenSource(_stopBenchmark.Token))
                        {
                            try
                            {
                                // this will drain the container
                                await BenchmarkNextAlgorithm(_stopCurrentAlgorithmBenchmark.Token);
                                await Task.Delay(MiningSettings.Instance.MinerRestartDelayMS, _stopCurrentAlgorithmBenchmark.Token);
                            }
                            catch (TaskCanceledException e)
                            {
                                Logger.Debug("BenchmarkHandler", $"TaskCanceledException occurred in benchmark task: {e.Message}");
                            }
                            catch (Exception e)
                            {
                                Logger.Error("BenchmarkHandler", $"Exception occurred in benchmark task: {e.Message}");
                            }
                        }
                    }
                    AfterBenchmarkCleanup();
                    // TODO set device to stopped if not mining after benchmark
                    // TODO start mining after benchmark
                    var cancel = _stopBenchmark.IsCancellationRequested;
                    // don't show unbenchmarked algos if user canceled
                    showFailed = _benchmarkFailedAlgo.Count > 0 && !cancel;
                    startMining = StartMiningAfterBenchmark && !cancel;
                }
            }
            finally
            {
                BenchmarkingHandlers.TryRemove(Device, out var _);
                Device.State = DeviceState.Stopped; // TODO should this be PENDING state?
                BenchmarkManager.EndBenchmarkForDevice(Device, showFailed, startMining);
            }
        }

        public void StopBenchmark()
        {
            try
            {
                _stopBenchmark?.Cancel();
            }
            catch { }
        }

        private void StopCurrentAlgorithmBenchmark()
        {
            try
            {
                _stopCurrentAlgorithmBenchmark?.Cancel();
            }
            catch { }
        }

        private void AfterBenchmarkCleanup()
        {
            foreach (var kvp in _benchmarkAlgorithms)
            {
                try
                {
                    kvp.Value.ClearBenchmarkPending();
                }
                catch (Exception e)
                {
                    Logger.Error("BenchmarkHandler", $"Exception occurred in benchmark task: {e.Message}");
                }
            }
            _benchmarkAlgorithms.Clear();
        }

        private async Task BenchmarkNextAlgorithm(CancellationToken stop)
        {
            var nextAlgo = TakeNextAlgorithm();
            if (nextAlgo == null)
            {
                Logger.Error("BenchmarkingComputeDeviceHandler.BenchmarkNextAlgorithm", $"TakeNextAlgorithm returned null");
                return;
            }
            try
            {
                nextAlgo.IsBenchmarking = true;
                await BenchmarkAlgorithm(nextAlgo, stop);
            }
            finally
            {
                nextAlgo.ClearBenchmarkPending();
                nextAlgo.IsBenchmarking = false;
            }
        }

        private async Task BenchmarkAlgorithm(AlgorithmContainer algo, CancellationToken stop)
        {
            var miningLocation = StratumService.Instance.SelectedOrFallbackServiceLocationCode().miningLocationCode;
            // TODO hidden issue here if our market is not available we will not be able to execute benchmarks
            // unable to benchmark service locations are not operational
            if (miningLocation == null) return; 
            using (var powerHelper = new PowerHelper(algo.ComputeDevice))
            {
                var plugin = algo.PluginContainer;
                var miner = plugin.CreateMiner();
                var miningPair = new NHM.MinerPlugin.MiningPair
                {
                    Device = algo.ComputeDevice.BaseDevice,
                    Algorithm = algo.Algorithm
                };
                // check ethlargement
                var miningPairs = new List<NHM.MinerPlugin.MiningPair> { miningPair };
                EthlargementIntegratedPlugin.Instance.Start(miningPairs);
                miner.InitMiningPairs(miningPairs);
                // fill service since the benchmark might be online. DemoUser.BTC must be used
                miner.InitMiningLocationAndUsername(miningLocation, DemoUser.BTC);
                powerHelper.Start();
                algo.ComputeDevice.State = DeviceState.Benchmarking;
                var result = await miner.StartBenchmark(stop, PerformanceType);
                if (stop.IsCancellationRequested) return;

                algo.IsReBenchmark = false;
                //EthlargementIntegratedPlugin.Instance.Stop(miningPairs); // TODO check stopping
                var power = powerHelper.Stop();
                if (result.Success || result.AlgorithmTypeSpeeds?.Count > 0)
                {
                    var ids = result.AlgorithmTypeSpeeds.Select(ats => ats.type).ToList();
                    var speeds = result.AlgorithmTypeSpeeds.Select(ats => ats.speed).ToList();
                    algo.Speeds = speeds;
                    algo.PowerUsage = power;
                    ConfigManager.CommitBenchmarksForDevice(algo.ComputeDevice);
                }
                else
                {
                    // mark it as failed
                    algo.LastBenchmarkingFailed = true;
                    // add new failed list
                    _benchmarkFailedAlgo.Add(algo.AlgorithmName);
                    algo.SetBenchmarkError(result.ErrorMessage);
                }
            }
        }

        // takes and removes the next algorithm from the container
        private AlgorithmContainer TakeNextAlgorithm()
        {
            string removeKey = null;
            // TODO for now just take the first one, but make it ordered in the future
            foreach (var nextPair in _benchmarkAlgorithms)
            {
                removeKey = nextPair.Key;
                break; // break the foreach
            }

            if (removeKey != null && _benchmarkAlgorithms.TryRemove(removeKey, out var ret)) return ret;
            return null;
        }
    }
}
