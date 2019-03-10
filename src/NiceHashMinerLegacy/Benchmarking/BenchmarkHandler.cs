using NiceHashMiner.Algorithms;
using NiceHashMiner.Benchmarking.BenchHelpers;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.Grouping;
using System.Collections.Generic;
using System.Threading;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Benchmarking
{
    public class BenchmarkHandler : IBenchmarkComunicator
    {
        private readonly Queue<Algorithm> _benchmarkAlgorithmQueue;
        private readonly int _benchmarkAlgorithmsCount;
        private int _benchmarkCurrentIndex = -1;
        private readonly List<string> _benchmarkFailedAlgo = new List<string>();
        private readonly IBenchmarkForm _benchmarkForm;
        private Algorithm _currentAlgorithm;
        private Miner _currentMiner;
        private readonly BenchmarkPerformanceType _performanceType;

        private PowerHelper _powerHelper;

        public BenchmarkHandler(ComputeDevice device, Queue<Algorithm> algorithms, IBenchmarkForm form,
            BenchmarkPerformanceType performance)
        {
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
            var thread = new Thread(NextBenchmark);
            if (thread.Name == null)
                thread.Name = $"dev_{Device.DeviceType}-{Device.ID}_benchmark";
            thread.Start();
        }

        public void OnBenchmarkComplete(bool success, string status)
        {
            if (!_benchmarkForm.InBenchmark) return;

            var rebenchSame = false;
            var power = _powerHelper.Stop();

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

            if (!success && !rebenchSame)
            {
                // add new failed list
                _benchmarkFailedAlgo.Add(_currentAlgorithm.AlgorithmName);
                _benchmarkForm.SetCurrentStatus(Device, _currentAlgorithm, status);
            }
            else if (!rebenchSame)
            {
                // set status to empty string it will return speed
                _currentAlgorithm.ClearBenchmarkPending();
                _benchmarkForm.SetCurrentStatus(Device, _currentAlgorithm, "");
            }

            if (rebenchSame)
            {
                _powerHelper.Start();

                if (dualAlgo != null && dualAlgo.TuningEnabled)
                {
                    var time = ConfigManager.GeneralConfig.BenchmarkTimeLimits
                        .GetBenchamrktime(_performanceType, Device.DeviceGroupType);
                    _currentMiner.BenchmarkStart(time, this);
                }
            }
            else
            {
                NextBenchmark();
            }
        }

        private void NextBenchmark()
        {
            ++_benchmarkCurrentIndex;
            if (_benchmarkCurrentIndex > 0) _benchmarkForm.StepUpBenchmarkStepProgress();
            if (_benchmarkCurrentIndex >= _benchmarkAlgorithmsCount)
            {
                EndBenchmark();
                return;
            }

            if (_benchmarkAlgorithmQueue.Count > 0)
                _currentAlgorithm = _benchmarkAlgorithmQueue.Dequeue();

            if (Device != null && _currentAlgorithm != null)
            {
                _currentMiner = MinerFactory.CreateMiner(Device, _currentAlgorithm);
                if (_currentAlgorithm is DualAlgorithm dualAlgo && dualAlgo.TuningEnabled) dualAlgo.StartTuning();
            }

            if (_currentMiner != null && _currentAlgorithm != null && Device != null)
            {
                _currentMiner.InitBenchmarkSetup(new MiningPair(Device, _currentAlgorithm));

                var time = ConfigManager.GeneralConfig.BenchmarkTimeLimits
                    .GetBenchamrktime(_performanceType, Device.DeviceGroupType);
                //currentConfig.TimeLimit = time;

                // dagger about 4 minutes
                //var showWaitTime = _currentAlgorithm.NiceHashID == AlgorithmType.DaggerHashimoto ? 4 * 60 : time;

                _benchmarkForm.AddToStatusCheck(Device, _currentAlgorithm);

                _currentMiner.BenchmarkStart(time, this);
                _powerHelper.Start();
            }
            else
            {
                NextBenchmark();
            }
        }

        private void EndBenchmark()
        {
            _currentAlgorithm?.ClearBenchmarkPending();
            _benchmarkForm.EndBenchmarkForDevice(Device, _benchmarkFailedAlgo.Count > 0);
        }

        public void InvokeQuit()
        {
            // clear benchmark pending status
            _currentAlgorithm?.ClearBenchmarkPending();
            if (_currentMiner != null)
            {
                _currentMiner.BenchmarkSignalQuit = true;
                _currentMiner.InvokeBenchmarkSignalQuit();
            }

            _currentMiner = null;
        }
    }
}
