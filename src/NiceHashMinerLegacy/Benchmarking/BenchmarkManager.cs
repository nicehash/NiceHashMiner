using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using NiceHashMiner.Miners;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMiner.Stats;

namespace NiceHashMiner.Benchmarking
{
    public static class BenchmarkManager
    {
        private static bool _inBenchmark;

        private static int _benchmarkCurrentIndex;

        private static bool _hasFailedAlgorithms;

        private static int _benchAlgoCount;
        private static readonly Dictionary<string, BenchmarkSettingsStatus> _benchDevAlgoStatus;

        private static readonly Dictionary<ComputeDevice, Algorithm> _statusCheckAlgos;

        private static readonly List<BenchmarkHandler> _runningBenchmarkThreads;

        private static IBenchmarkForm _benchForm;

        private static readonly List<Tuple<ComputeDevice, Queue<Algorithm>>> _benchDevAlgoQueue;

        public static event EventHandler<StepUpEventArgs> OnStepUp;
        public static event EventHandler<AlgoStatusEventArgs> OnAlgoStatusUpdate;

        public static BenchmarkSelection Selection { get; set; }

        public static IReadOnlyList<Tuple<ComputeDevice, Queue<Algorithm>>> BenchDevAlgoQueue
        {
            get
            {
                lock (_benchDevAlgoQueue)
                {
                    return _benchDevAlgoQueue;
                }
            }
        }

        public static bool InBenchmark
        {
            get => _inBenchmark;
            private set
            {
                _inBenchmark = value;
                // If starting mining after, don't update for STOPPED status
                if (value || (!_benchForm?.StartMiningOnFinish ?? true))
                {
                    NiceHashStats.StateChanged();
                }
            }
        }

        public static bool HasWork
        {
            get
            {
                lock (_benchDevAlgoStatus)
                { 
                    return _benchDevAlgoStatus.Values.Any(s => s == BenchmarkSettingsStatus.Todo);
                }
            }
        }

        static BenchmarkManager()
        {
            Selection = BenchmarkSelection.SelectedUnbenchmarkedAlgorithms;
            _benchDevAlgoStatus = new Dictionary<string, BenchmarkSettingsStatus>();
            _benchDevAlgoQueue = new List<Tuple<ComputeDevice, Queue<Algorithm>>>();
            _statusCheckAlgos = new Dictionary<ComputeDevice, Algorithm>();
            _runningBenchmarkThreads = new List<BenchmarkHandler>();
        }

        #region Public get helpers

        public static IEnumerable<Tuple<ComputeDevice, Algorithm>> GetStatusCheckAlgos()
        {
            if (!InBenchmark) yield break;

            lock (_statusCheckAlgos)
            {
                foreach (var kvp in _statusCheckAlgos)
                {
                    yield return new Tuple<ComputeDevice, Algorithm>(kvp.Key, kvp.Value);
                }
            }
        }

        public static IEnumerable<int> GetBenchmarkingDevices()
        {
            return BenchDevAlgoQueue.Select(t => t.Item1).Select(d => d.Index);
        }

        #endregion

        #region Calculation methods

        public static int CalcBenchDevAlgoQueue()
        {
            _benchAlgoCount = 0;
            lock (_benchDevAlgoQueue)
            lock (_benchDevAlgoStatus)
            {
                _benchDevAlgoStatus.Clear();
                _benchDevAlgoQueue.Clear();
                foreach (var cDev in ComputeDeviceManager.Available.Devices)
                {
                    var algorithmQueue = new Queue<Algorithm>();
                    foreach (var algo in cDev.GetAlgorithmSettings())
                    {
                        if (ShouldBenchmark(algo))
                        {
                            algorithmQueue.Enqueue(algo);
                            algo.SetBenchmarkPendingNoMsg();
                        }
                        else
                        {
                            algo.ClearBenchmarkPending();
                        }
                    }


                    BenchmarkSettingsStatus status;
                    if (cDev.Enabled)
                    {
                        _benchAlgoCount += algorithmQueue.Count;
                        status = algorithmQueue.Count == 0
                            ? BenchmarkSettingsStatus.None
                            : BenchmarkSettingsStatus.Todo;
                        _benchDevAlgoQueue.Add(
                            new Tuple<ComputeDevice, Queue<Algorithm>>(cDev, algorithmQueue)
                        );
                    }
                    else
                    {
                        status = algorithmQueue.Count == 0
                            ? BenchmarkSettingsStatus.DisabledNone
                            : BenchmarkSettingsStatus.DisabledTodo;
                    }

                    _benchDevAlgoStatus[cDev.Uuid] = status;
                }
            }

            _benchmarkCurrentIndex = 0;

            return _benchAlgoCount;
        }

        private static bool ShouldBenchmark(Algorithm algorithm)
        {
            var isBenchmarked = !algorithm.BenchmarkNeeded;
            switch (Selection)
            {
                case BenchmarkSelection.SelectedUnbenchmarkedAlgorithms when !isBenchmarked &&
                                                                                         algorithm.Enabled:
                    return true;
                case BenchmarkSelection.UnbenchmarkedAlgorithms when !isBenchmarked:
                    return true;
                case BenchmarkSelection.ReBecnhSelectedAlgorithms when algorithm.Enabled:
                    return true;
                case BenchmarkSelection.AllAlgorithms:
                    return true;
            }

            return false;
        }

        #endregion

        #region Start/Stop methods

        public static void Start(BenchmarkPerformanceType perfType, IBenchmarkForm form)
        {
            _benchForm = form;
            _hasFailedAlgorithms = false;
            lock (_runningBenchmarkThreads)
            lock (_statusCheckAlgos)
            {
                _statusCheckAlgos.Clear();
                _runningBenchmarkThreads.Clear();

                foreach (var pair in BenchDevAlgoQueue)
                {
                    var handler = new BenchmarkHandler(pair.Item1, pair.Item2, perfType);
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

        private static void End()
        {
            InBenchmark = false;
            Ethlargement.Stop();
            _benchForm?.EndBenchmark(_hasFailedAlgorithms);
        }

        public static void DisableTodoAlgos()
        {
            CalcBenchDevAlgoQueue();
            foreach (var q in BenchDevAlgoQueue.Select(q => q.Item2))
            {
                foreach (var a in q)
                {
                    a.Enabled = false;
                }
            }
        }

        public static void ClearQueue()
        {
            lock (_benchDevAlgoQueue)
            {
                _benchDevAlgoQueue.Clear();
            }
        }

        #endregion

        #region In-bench status updates

        public static bool IsDevBenchmarked(string uuid)
        {
            if (_benchDevAlgoStatus == null || !_benchDevAlgoStatus.TryGetValue(uuid, out var status)) return true;
            return status == BenchmarkSettingsStatus.Todo || status == BenchmarkSettingsStatus.DisabledTodo;
        }

        public static void AddToStatusCheck(ComputeDevice device, Algorithm algorithm)
        {
            lock (_statusCheckAlgos)
            {
                _statusCheckAlgos[device] = algorithm;
            }
        }

        public static void RemoveFromStatusCheck(ComputeDevice device)
        {
            lock (_statusCheckAlgos)
            {
                _statusCheckAlgos.Remove(device);
            }
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

        #endregion
        
        private enum BenchmarkSettingsStatus
        {
            None = 0,
            Todo,
            DisabledNone,
            DisabledTodo
        }
    }

    public class StepUpEventArgs : EventArgs
    {
        public int CurrentIndex { get; }
        public int AlgorithmCount { get; }

        public StepUpEventArgs(int index, int count)
        {
            CurrentIndex = index;
            AlgorithmCount = count;
        }
    }

    public class AlgoStatusEventArgs : EventArgs
    {
        public ComputeDevice Device { get; }
        public Algorithm Algorithm { get; }
        public string Status { get; }

        public AlgoStatusEventArgs(ComputeDevice dev, Algorithm algo, string status)
        {
            Device = dev;
            Algorithm = algo;
            Status = status;
        }
    }
}
