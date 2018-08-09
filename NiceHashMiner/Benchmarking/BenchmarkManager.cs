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

        public static AlgorithmBenchmarkSettingsType AlgorithmOption =
            AlgorithmBenchmarkSettingsType.SelectedUnbenchmarkedAlgorithms;

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

        public static bool HasWork => BenchDevAlgoStatus.Values.Any(s => s == BenchmarkSettingsStatus.TODO);

        static BenchmarkManager()
        {
            BenchDevAlgoStatus = new Dictionary<string, BenchmarkSettingsStatus>();
            BenchDevAlgoQueue = new List<Tuple<ComputeDevice, Queue<Algorithm>>>();
            StatusCheckAlgos = new Dictionary<ComputeDevice, Algorithm>();
            RunningBenchmarkThreads = new List<BenchmarkHandler>();
        }

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
                    status = algorithmQueue.Count == 0 ? BenchmarkSettingsStatus.NONE : BenchmarkSettingsStatus.TODO;
                    BenchDevAlgoQueue.Add(
                        new Tuple<ComputeDevice, Queue<Algorithm>>(cDev, algorithmQueue)
                    );
                }
                else
                {
                    status = algorithmQueue.Count == 0
                        ? BenchmarkSettingsStatus.DISABLED_NONE
                        : BenchmarkSettingsStatus.DISABLED_TODO;
                }

                BenchDevAlgoStatus[cDev.Uuid] = status;
            }

            _benchmarkCurrentIndex = 0;

            return _benchAlgoCount;
        }

        public static IEnumerable<Tuple<ComputeDevice, Algorithm>> GetStatusCheckAlgos()
        {
            if (!InBenchmark) yield break;

            foreach (var kvp in StatusCheckAlgos)
            {
                yield return new Tuple<ComputeDevice, Algorithm>(kvp.Key, kvp.Value);
            }
        }

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

        public static void End()
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

        private static bool ShouldBenchmark(Algorithm algorithm)
        {
            var isBenchmarked = !algorithm.BenchmarkNeeded;
            switch (AlgorithmOption)
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

        public static bool IsDevBenchmarked(string uuid)
        {
            if (BenchDevAlgoStatus == null) return true;
            var status = BenchDevAlgoStatus[uuid];
            return status == BenchmarkSettingsStatus.TODO || status == BenchmarkSettingsStatus.DISABLED_TODO;
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
