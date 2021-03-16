using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHMCore.Configs;
using NHMCore.Mining.Benchmarking;
using NHMCore.Mining.Plugins;
using NHMCore.Notifications;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Mining.Grouping
{
    internal class BenchmarkingDevice
    {
        public BenchmarkingDevice(ComputeDevice device)
        {
            Device = device;
            _algorithms = device.AlgorithmsForBenchmark().ToArray();
        }


        public Task BenchmarkTask { get; private set; }
        public ComputeDevice Device { get; }
        private IReadOnlyList<AlgorithmContainer> _algorithms { get; set; }

        CancellationTokenSource _stopBenchmark;
        CancellationTokenSource _stopCurrentAlgorithmBenchmark;

        private readonly TrivialChannel<IReadOnlyList<AlgorithmContainer>> _commandQueue = new TrivialChannel<IReadOnlyList<AlgorithmContainer>>();

        private async Task<object> GetCommand(CancellationToken stop)
        {
            var ret = await _commandQueue.ReadAsync(TimeSpan.FromDays(1), stop);
            return ret.t;
        }

        public BenchmarkPerformanceType PerformanceType { get; set; } = BenchmarkPerformanceType.Standard; // TODO TEMP

        private async Task<object> BenchmarkAlgorithm(AlgorithmContainer algo, CancellationToken stop)
        {
            if (algo == null)
            {
                Logger.Error("BenchmarkingComputeDeviceHandler.BenchmarkNextAlgorithm", $"TakeNextAlgorithm returned null");
                return false;
            }

            // TODO here you got shity state
            var miningLocation = StratumService.Instance.SelectedOrFallbackServiceLocationCode().miningLocationCode;
            // TODO hidden issue here if our market is not available we will not be able to execute benchmarks
            // unable to benchmark service locations are not operational
            if (miningLocation == null) return false;


            bool ret = false;
            var miningPairs = new List<MiningPair> { algo.ToMiningPair() };
            try
            {
                algo.IsBenchmarking = true;
                using (var powerHelper = new PowerHelper(algo.ComputeDevice))
                {
                    var plugin = algo.PluginContainer;
                    var miner = plugin.CreateMiner();

                    // check ethlargement
                    EthlargementIntegratedPlugin.Instance.Start(miningPairs);
                    miner.InitMiningPairs(miningPairs);
                    // fill service since the benchmark might be online. DemoUser.BTC must be used
                    miner.InitMiningLocationAndUsername(miningLocation, DemoUser.BTC);
                    powerHelper.Start();
                    algo.ComputeDevice.State = DeviceState.Benchmarking;
                    var result = await miner.StartBenchmark(stop, PerformanceType);
                    EthlargementIntegratedPlugin.Instance.Stop(miningPairs);
                    if (stop.IsCancellationRequested) return false;

                    algo.IsReBenchmark = false;
                    var power = powerHelper.Stop();
                    ret = result.Success || result.AlgorithmTypeSpeeds?.Count > 0;
                    if (ret)
                    {
                        algo.Speeds = result.AlgorithmTypeSpeeds.Select(ats => ats.speed).ToList();
                        algo.PowerUsage = power;
                        ConfigManager.CommitBenchmarksForDevice(algo.ComputeDevice);
                    }
                    else
                    {
                        // mark it as failed
                        algo.LastBenchmarkingFailed = true;
                        algo.SetBenchmarkError(result.ErrorMessage);
                    }
                }
            }
            finally
            {
                EthlargementIntegratedPlugin.Instance.Stop(miningPairs);
                algo.ClearBenchmarkPending();
                algo.IsBenchmarking = false;
            }

            return ret;
        }

        public void StartBenchmark()
        {
            BenchmarkTask = Task.Run(async () => await Benchmark());
        }

        private async Task Benchmark()
        {
            bool showFailed = false;
            bool startMining = false;
            _stopBenchmark = CancellationTokenSource.CreateLinkedTokenSource(ApplicationStateManager.ExitApplication.Token);
            try
            {
                foreach (var a in _algorithms) a.SetBenchmarkPending();
                var benchAlgos = new Queue<AlgorithmContainer>(_algorithms);
                //BenchmarkingHandlers.TryAdd(Device, this);
                // whole benchmark scope
                var commandTask = GetCommand(_stopBenchmark.Token);
                // Until container is not empty
                while (benchAlgos.Any() && !_stopBenchmark.IsCancellationRequested)
                {
                    // per algo benchmark scope
                    bool? benchmarkSuccess = null;
                    _stopCurrentAlgorithmBenchmark = CancellationTokenSource.CreateLinkedTokenSource(_stopBenchmark.Token);
                    try
                    {
                        var nextAlgo = benchAlgos.Dequeue();
                        var benchmark = BenchmarkAlgorithm(nextAlgo, _stopCurrentAlgorithmBenchmark.Token);
                        var firstFinished = await Task.WhenAny(new Task<object>[] { commandTask, benchmark });
                        var ret = await firstFinished;
                        if (ret is IReadOnlyList<AlgorithmContainer> updatedAlgorithms)
                        {
                            commandTask = GetCommand(_stopBenchmark.Token);
                            var stop = _algorithms.Except(updatedAlgorithms).Any(algo => algo == nextAlgo);
                            var restAlgorithms = updatedAlgorithms.Where(algo => algo != nextAlgo).ToArray();
                            // update Algorithms
                            _algorithms = restAlgorithms;
                            foreach (var a in _algorithms) a.SetBenchmarkPending();
                            // updated 
                            benchAlgos = new Queue<AlgorithmContainer>(_algorithms);
                            if (stop)
                            {
                                // THIS Throws
                                _stopCurrentAlgorithmBenchmark.Cancel();
                            }
                            else
                            {
                                // wait for the current benchmark to finish
                                ret = await benchmark;
                            }
                        }
                        if (ret is bool success)
                        {
                            benchmarkSuccess = success;
                        }
                        // this will drain the container
                        await Task.Delay(MiningSettings.Instance.MinerRestartDelayMS, _stopCurrentAlgorithmBenchmark.Token);
                    }
                    catch (TaskCanceledException e)
                    {
                        Logger.Debug("BenchmarkingDevice", $"TaskCanceledException occurred in benchmark task: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        Logger.Error("BenchmarkingDevice", $"Exception occurred in benchmark task: {e.Message}");
                    }
                    finally
                    {
                        _stopCurrentAlgorithmBenchmark.Dispose();
                        if (!_stopCurrentAlgorithmBenchmark.IsCancellationRequested && benchmarkSuccess.HasValue && !benchmarkSuccess.Value) showFailed = true;
                    }
                }
                startMining = !_stopBenchmark.IsCancellationRequested;
                // clear what we didn't benchmark
                while (benchAlgos.Any())
                {
                    var nextAlgo = benchAlgos.Dequeue();
                    nextAlgo.ClearBenchmarkPending();
                }
                if (showFailed) AvailableNotifications.CreateFailedBenchmarksInfo(Device);
            }
            catch (Exception ex)
            {
                Logger.Error("BenchmarkingDevice2", $"Exception occurred in benchmark task: {ex.Message}");
            }
            finally
            {
                _stopBenchmark.Dispose();
                if (startMining)
                {
                    _ = MiningManager.StartDevice(Device); // important do not await
                }
                else
                {
                    Device.State = DeviceState.Stopped;
                }
            }
        }

        public void Update()
        {
            _commandQueue.Enqueue(Device.AlgorithmsForBenchmark().ToArray());
        }

        public async Task StopBenchmark()
        {
            try
            {
                _stopBenchmark?.Cancel();
                await BenchmarkTask;
            }
            catch { }
        }
    }

    
}
