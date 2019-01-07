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

        public static bool InBenchmark
        {
            get => _inBenchmark;
            private set
            {
                _inBenchmark = value;
                NiceHashStats.StateChanged();
            }
        }

        public static BenchmarkSelection Selection { private get; set; } =
            BenchmarkSelection.SelectedUnbenchmarkedAlgorithms;

        private static int _benchmarkCurrentIndex;

        private static bool _hasFailedAlgorithms;

        private static int _benchAlgoCount;
        private static readonly Dictionary<string, BenchmarkSettingsStatus> BenchDevAlgoStatus;
        public static readonly List<Tuple<ComputeDevice, Queue<Algorithm>>> BenchDevAlgoQueue;

        private static readonly Dictionary<ComputeDevice, Algorithm> StatusCheckAlgos;

        private static readonly List<BenchmarkHandler> RunningBenchmarkThreads;

        private static IBenchmarkForm _benchForm;

        public static event EventHandler<StepUpEventArgs> OnStepUp;
        public static event EventHandler<AlgoStatusEventArgs> OnAlgoStatusUpdate;

        public static bool HasWork => BenchDevAlgoStatus.Values.Any(s => s == BenchmarkSettingsStatus.Todo);

        static BenchmarkManager()
        {
            BenchDevAlgoStatus = new Dictionary<string, BenchmarkSettingsStatus>();
            BenchDevAlgoQueue = new List<Tuple<ComputeDevice, Queue<Algorithm>>>();
            StatusCheckAlgos = new Dictionary<ComputeDevice, Algorithm>();
            RunningBenchmarkThreads = new List<BenchmarkHandler>();
        }

        #region Public get helpers

        public static IEnumerable<Tuple<ComputeDevice, Algorithm>> GetStatusCheckAlgos()
        {
            if (!InBenchmark) yield break;

            foreach (var kvp in StatusCheckAlgos)
            {
                yield return new Tuple<ComputeDevice, Algorithm>(kvp.Key, kvp.Value);
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
            BenchDevAlgoStatus.Clear();
            BenchDevAlgoQueue.Clear();
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
                    status = algorithmQueue.Count == 0 ? BenchmarkSettingsStatus.None : BenchmarkSettingsStatus.Todo;
                    BenchDevAlgoQueue.Add(
                        new Tuple<ComputeDevice, Queue<Algorithm>>(cDev, algorithmQueue)
                    );
                }
                else
                {
                    status = algorithmQueue.Count == 0
                        ? BenchmarkSettingsStatus.DisabledNone
                        : BenchmarkSettingsStatus.DisabledTodo;
                }

                BenchDevAlgoStatus[cDev.Uuid] = status;
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
            StatusCheckAlgos.Clear();
            lock (RunningBenchmarkThreads)
            {
                RunningBenchmarkThreads.Clear();

                foreach (var pair in BenchDevAlgoQueue)
                {
                    var handler = new BenchmarkHandler(pair.Item1, pair.Item2, perfType);
                    RunningBenchmarkThreads.Add(handler);
                }
                // Don't start until list is populated
                foreach (var thread in RunningBenchmarkThreads)
                {
                    thread.Start();
                }
            }

            InBenchmark = true;


        }

        public static void Stop()
        {
            InBenchmark = false;

            lock (RunningBenchmarkThreads)
            {
                foreach (var handler in RunningBenchmarkThreads) handler.InvokeQuit();
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
            foreach (var a in q)
                a.Enabled = false;
        }

        #endregion

        #region In-bench status updates

        public static bool IsDevBenchmarked(string uuid)
        {
            if (BenchDevAlgoStatus == null || !BenchDevAlgoStatus.TryGetValue(uuid, out var status)) return true;
            return status == BenchmarkSettingsStatus.Todo || status == BenchmarkSettingsStatus.DisabledTodo;
        }

        public static void AddToStatusCheck(ComputeDevice device, Algorithm algorithm)
        {
            StatusCheckAlgos[device] = algorithm;
        }

        public static void RemoveFromStatusCheck(ComputeDevice device)
        {
            StatusCheckAlgos.Remove(device);
        }

        public static void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos)
        {
            _hasFailedAlgorithms = failedAlgos || _hasFailedAlgorithms;
            lock (RunningBenchmarkThreads)
            {
                RunningBenchmarkThreads.RemoveAll(x => x.Device == device);

                if (RunningBenchmarkThreads.Count <= 0)
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
