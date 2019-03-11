using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Benchmarking.BenchHelpers;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common.Device;
using CommonAlgorithm = NiceHashMinerLegacy.Common.Algorithm;
using MinerPlugin;
using NiceHashMiner.Plugin;

namespace NiceHashMiner.Benchmarking
{
    public class BenchmarkHandler2
    {
        CancellationTokenSource _stopBenchmark;

        // OLD
        private readonly Queue<Algorithm> _benchmarkAlgorithmQueue;
        private readonly int _benchmarkAlgorithmsCount;
        private readonly List<string> _benchmarkFailedAlgo = new List<string>();
        private readonly IBenchmarkForm _benchmarkForm;
        private Algorithm _currentAlgorithm;
        private readonly BenchmarkPerformanceType _performanceType;


        //private ClaymoreZcashBenchHelper _claymoreZcashStatus;
        //private CpuBenchHelper _cpuBenchmarkStatus;
        private PowerHelper _powerHelper;

        // plugin stuff
        private IMiner _minerFromPlugin;

        public BenchmarkHandler2(ComputeDevice device, Queue<Algorithm> algorithms, IBenchmarkForm form,
            BenchmarkPerformanceType performance)
        {
            _stopBenchmark = new CancellationTokenSource();
            Device = device;
            _benchmarkAlgorithmQueue = algorithms;
            _benchmarkForm = form;
            _performanceType = performance;

            _benchmarkAlgorithmsCount = _benchmarkAlgorithmQueue.Count;
            _powerHelper = new PowerHelper(device);
        }

        public ComputeDevice Device { get; }

        public void Start()
        {
            var thread = new Thread(Benchmark);
            if (thread.Name == null)
                thread.Name = $"dev_{Device.DeviceType}-{Device.ID}_benchmark";
            thread.Start();
        }

        private async void Benchmark()
        {
            while (_benchmarkAlgorithmQueue.Count > 0)
            {
                if (_stopBenchmark.IsCancellationRequested)
                {
                    break;
                }

                _currentAlgorithm = _benchmarkAlgorithmQueue.Dequeue();
                if (_currentAlgorithm is PluginAlgorithm pAlgo)
                {
                    await BenchmarkPluginAlgorithm();
                }
                else
                {
                    await BenchmarkAlgorithm();
                }
                _benchmarkForm.StepUpBenchmarkStepProgress();
            }
            // if we get here we are finished
            EndBenchmark();
        }

        private async Task BenchmarkAlgorithm(/*Algorithm algo*/)
        {
            // fix naming
            var _currentMiner = MinerFactory.CreateMiner(Device, _currentAlgorithm);
            if (_currentMiner == null)
            {
                return;
            }
            // well lets just assume it is not null
            // actual benchmarking scope?
            {
                _benchmarkForm.AddToStatusCheck(Device, _currentAlgorithm);
                // TODO add the multiple benchmark loop


                _currentMiner.InitBenchmarkSetup(new MiningPair(Device, _currentAlgorithm));
                var time = ConfigManager.GeneralConfig.BenchmarkTimeLimits
                    .GetBenchamrktime(_performanceType, Device.DeviceGroupType);
                ////currentConfig.TimeLimit = time;
                //if (_cpuBenchmarkStatus != null) _cpuBenchmarkStatus.Time = time;
                //if (_claymoreZcashStatus != null) _claymoreZcashStatus.Time = time;

                // dagger about 4 minutes
                //var showWaitTime = _currentAlgorithm.NiceHashID == AlgorithmType.DaggerHashimoto ? 4 * 60 : time;



                var benchTaskResult = _currentMiner.BenchmarkStartAsync(time, _stopBenchmark.Token);
                _powerHelper.Start();
                var result = await benchTaskResult;
                var power = _powerHelper.Stop();
                var rebenchSame = false; // TODO get rid of this, but keep for DualAlgorithm's for now

                var dualAlgo = _currentAlgorithm as DualAlgorithm;
                if (dualAlgo != null && dualAlgo.TuningEnabled)
                {
                    dualAlgo.SetPowerForCurrent(power);

                    if (dualAlgo.IncrementToNextEmptyIntensity())
                        rebenchSame = true;
                }
                else
                {
                    _currentAlgorithm.PowerUsage = power;
                }

                if (!rebenchSame) _benchmarkForm.RemoveFromStatusCheck(Device, _currentAlgorithm);

                if (!result.Success && !rebenchSame)
                {
                    // add new failed list
                    _benchmarkFailedAlgo.Add(_currentAlgorithm.AlgorithmName);
                    _benchmarkForm.SetCurrentStatus(Device, _currentAlgorithm, result.Status);
                }
                else if (!rebenchSame)
                {
                    // set status to empty string it will return speed
                    _currentAlgorithm.ClearBenchmarkPending();
                    _benchmarkForm.SetCurrentStatus(Device, _currentAlgorithm, "");
                }
            }
        }

        private async Task BenchmarkPluginAlgorithm(/*PluginAlgorithm algo*/)
        {
            var algo = (PluginAlgorithm)_currentAlgorithm;
            var plugin = MinerPluginsManager.GetPluginWithUuid(algo.BaseAlgo.MinerID);
            var miner = plugin.CreateMiner();
            miner.InitMiningPairs(new List<(BaseDevice, CommonAlgorithm.Algorithm)>{ (Device.PluginDevice, algo.BaseAlgo) });
            miner.InitMiningLocationAndUsername(Globals.MiningLocation[ConfigManager.GeneralConfig.ServiceLocation], Globals.DemoUser); // maybe it will be online
            var result = await miner.StartBenchmark(_stopBenchmark.Token, _performanceType);
            if (result.ok)
            {
                algo.BenchmarkSpeed = result.speed;
                // set status to empty string it will return speed
                _currentAlgorithm.ClearBenchmarkPending();
                _benchmarkForm.SetCurrentStatus(Device, _currentAlgorithm, "");
            }
        }

        private void EndBenchmark()
        {
            _currentAlgorithm?.ClearBenchmarkPending();
            _benchmarkForm.EndBenchmarkForDevice(Device, _benchmarkFailedAlgo.Count > 0);
        }

        public void InvokeQuit()
        {
            _stopBenchmark.Cancel();
            // clear benchmark pending status
            _currentAlgorithm?.ClearBenchmarkPending();
        }
    }
}
