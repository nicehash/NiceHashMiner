using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Benchmarking
{
    public static class BenchmarkManager
    {
        public static bool InBenchmark { get; private set; }

        public static AlgorithmBenchmarkSettingsType _algorithmOption =
            AlgorithmBenchmarkSettingsType.SelectedUnbenchmarkedAlgorithms;

        private static int _benchmarkCurrentIndex;

        private static bool _hasFailedAlgorithms;
        private static List<BenchmarkHandler> _runningBenchmarkThreads = new List<BenchmarkHandler>();

        private static int _benchAlgoCount;
        public static Dictionary<string, BenchmarkSettingsStatus> _benchDevAlgoStatus { get; private set; }
        public static List<Tuple<ComputeDevice, Queue<Algorithm>>> _benchDevAlgoQueue { get; private set; }

        private static Dictionary<ComputeDevice, Algorithm> _statusCheckAlgos;

        private static IBenchmarkForm _benchForm;

        public static event EventHandler<StepUpEventArgs> OnStepUp;
        public static event EventHandler<AlgoStatusEventArgs> OnAlgoStatusUpdate;

        public static bool HasWork => _benchDevAlgoStatus.Values.Any(s => s == BenchmarkSettingsStatus.TODO);

        public static int CalcBenchDevAlgoQueue()
        {
            _benchAlgoCount = 0;
            _benchDevAlgoStatus = new Dictionary<string, BenchmarkSettingsStatus>();
            _benchDevAlgoQueue = new List<Tuple<ComputeDevice, Queue<Algorithm>>>();
            foreach (var cDev in ComputeDeviceManager.Available.Devices)
            {
                var algorithmQueue = new Queue<Algorithm>();
                foreach (var algo in cDev.GetAlgorithmSettings())
                    if (ShouldBenchmark(algo))
                    {
                        algorithmQueue.Enqueue(algo);
                        algo.SetBenchmarkPendingNoMsg();
                    }
                    else
                    {
                        algo.ClearBenchmarkPending();
                    }


                BenchmarkSettingsStatus status;
                if (cDev.Enabled)
                {
                    _benchAlgoCount += algorithmQueue.Count;
                    status = algorithmQueue.Count == 0 ? BenchmarkSettingsStatus.NONE : BenchmarkSettingsStatus.TODO;
                    _benchDevAlgoQueue.Add(
                        new Tuple<ComputeDevice, Queue<Algorithm>>(cDev, algorithmQueue)
                    );
                }
                else
                {
                    status = algorithmQueue.Count == 0
                        ? BenchmarkSettingsStatus.DISABLED_NONE
                        : BenchmarkSettingsStatus.DISABLED_TODO;
                }

                _benchDevAlgoStatus[cDev.Uuid] = status;
            }

            _benchmarkCurrentIndex = 0;

            return _benchAlgoCount;
        }

        public static IEnumerable<Tuple<ComputeDevice, Algorithm>> StatusCheckAlgos()
        {
            if (!InBenchmark) yield break;

            foreach (var kvp in _statusCheckAlgos)
            {
                yield return new Tuple<ComputeDevice, Algorithm>(kvp.Key, kvp.Value);
            }
        }

        public static void Start(BenchmarkPerformanceType perfType, IBenchmarkForm form)
        {
            _benchForm = form;
            _hasFailedAlgorithms = false;
            _statusCheckAlgos = new Dictionary<ComputeDevice, Algorithm>();
            lock (_runningBenchmarkThreads)
            {
                _runningBenchmarkThreads = new List<BenchmarkHandler>();

                foreach (var pair in _benchDevAlgoQueue)
                {
                    var handler = new BenchmarkHandler(pair.Item1, pair.Item2, this, perfType);
                    _runningBenchmarkThreads.Add(handler);
                }
                // Don't start until list is populated
                foreach (var thread in _runningBenchmarkThreads)
                {
                    thread.Start();
                }
            }

            InBenchmark = true;


        }

        public static void Stop()
        {
            InBenchmark = false;

            lock (_runningBenchmarkThreads)
            {
                foreach (var handler in _runningBenchmarkThreads) handler.InvokeQuit();
            }
        }

        public static void End()
        {
            InBenchmark = false;
            Ethlargement.Stop();
            _benchForm?.EndBenchmark(_hasFailedAlgorithms);
        }

        public static void DisableTodoAlgos()
        {
            CalcBenchDevAlgoQueue();
            foreach (var q in _benchDevAlgoQueue.Select(q => q.Item2))
            foreach (var a in q)
                a.Enabled = false;
        }

        private static bool ShouldBenchmark(Algorithm algorithm)
        {
            var isBenchmarked = !algorithm.BenchmarkNeeded;
            switch (_algorithmOption)
            {
                case AlgorithmBenchmarkSettingsType.SelectedUnbenchmarkedAlgorithms when !isBenchmarked &&
                                                                                         algorithm.Enabled:
                    return true;
                case AlgorithmBenchmarkSettingsType.UnbenchmarkedAlgorithms when !isBenchmarked:
                    return true;
                case AlgorithmBenchmarkSettingsType.ReBecnhSelectedAlgorithms when algorithm.Enabled:
                    return true;
                case AlgorithmBenchmarkSettingsType.AllAlgorithms:
                    return true;
            }

            return false;
        }



        public static void AddToStatusCheck(ComputeDevice device, Algorithm algorithm)
        {
            _statusCheckAlgos[device] = algorithm;
        }

        public static void RemoveFromStatusCheck(ComputeDevice device, Algorithm algorithm)
        {
            _statusCheckAlgos.Remove(device);
        }

        public static void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos)
        {
            _hasFailedAlgorithms = failedAlgos || _hasFailedAlgorithms;
            lock (_runningBenchmarkThreads)
            {
                _runningBenchmarkThreads.RemoveAll(x => x.Device == device);

                if (_runningBenchmarkThreads.Count <= 0)
                    End();
            }
        }

        public static void SetCurrentStatus(ComputeDevice dev, Algorithm algo, string status)
        {
            var args = new AlgoStatusEventArgs(dev, algo, status);
            OnAlgoStatusUpdate?.Invoke(null, args);
        }

        public static void StepUpBenchmarkStepProgress()
        {
            var args = new StepUpEventArgs(++_benchmarkCurrentIndex, _benchAlgoCount);
            OnStepUp?.Invoke(null, args);
        }
    }

    public enum BenchmarkSettingsStatus
    {
        NONE = 0,
        TODO,
        DISABLED_NONE,
        DISABLED_TODO
    }

    public class StepUpEventArgs : EventArgs
    {
        public readonly int CurrentIndex;
        public readonly int AlgorithmCount;

        public StepUpEventArgs(int index, int count)
        {
            CurrentIndex = index;
            AlgorithmCount = count;
        }
    }

    public class AlgoStatusEventArgs : EventArgs
    {
        public readonly ComputeDevice Device;
        public readonly Algorithm Algorithm;
        public readonly string Status;

        public AlgoStatusEventArgs(ComputeDevice dev, Algorithm algo, string status)
        {
            Device = dev;
            Algorithm = algo;
            Status = status;
        }
    }
}
