using NHM.Common.Enums;
using NHMCore.Interfaces;
using NHMCore.Mining;
using NHMCore.Mining.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHMCore.Benchmarking
{
    public static class BenchmarkManager
    {
        private static int _benchmarkCurrentIndex;

        private static bool _hasFailedAlgorithms;

        private static int _benchAlgoCount;
        private static readonly Dictionary<string, BenchmarkSettingsStatus> _benchDevAlgoStatus;

        private static readonly Dictionary<ComputeDevice, AlgorithmContainer> _statusCheckAlgos;

        private static IBenchmarkForm _benchForm;

        private static readonly List<Tuple<ComputeDevice, Queue<AlgorithmContainer>>> _benchDevAlgoQueue;

        public static event EventHandler<StepUpEventArgs> OnStepUp;
        public static event EventHandler<AlgoStatusEventArgs> OnAlgoStatusUpdate;
        public static event EventHandler<BenchEndEventArgs> OnBenchmarkEnd;
        public static event EventHandler<bool> InBenchmarkChanged;

        public static AlgorithmBenchmarkSettingsType Selection { get; set; }

        public static IReadOnlyList<Tuple<ComputeDevice, Queue<AlgorithmContainer>>> BenchDevAlgoQueue
        {
            get
            {
                lock (_benchDevAlgoQueue)
                {
                    return _benchDevAlgoQueue;
                }
            }
        }

        private static bool _startMiningOnFinish;

        public static bool StartMiningOnFinish
        {
            get
            {
                // _benchForm for WinForms, field for WPF
                return _benchForm?.StartMiningOnFinish ?? _startMiningOnFinish;
            }
            set => _startMiningOnFinish = value;
        }

        public static bool IsBenchmarking => BenchmarkingComputeDeviceHandler.IsBenchmarking;

        // TODO what the hell is with the setter?
        public static bool InBenchmark
        {
            get => IsBenchmarking;
            private set
            {
                InBenchmarkChanged?.Invoke(null, value);
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
            Selection = AlgorithmBenchmarkSettingsType.SelectedUnbenchmarkedAlgorithms;
            _benchDevAlgoStatus = new Dictionary<string, BenchmarkSettingsStatus>();
            _benchDevAlgoQueue = new List<Tuple<ComputeDevice, Queue<AlgorithmContainer>>>();
            _statusCheckAlgos = new Dictionary<ComputeDevice, AlgorithmContainer>();
        }

        public static bool DisableLastBenchmarkingFailed { get; set; } = false;


#region Public get helpers

        public static IEnumerable<Tuple<ComputeDevice, AlgorithmContainer>> GetStatusCheckAlgos()
        {
            if (!InBenchmark) yield break;

            lock (_statusCheckAlgos)
            {
                foreach (var kvp in _statusCheckAlgos)
                {
                    yield return new Tuple<ComputeDevice, AlgorithmContainer>(kvp.Key, kvp.Value);
                }
            }
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
                foreach (var cDev in AvailableDevices.Devices)
                {
                    var algorithmQueue = new Queue<AlgorithmContainer>();
                    foreach (var algo in cDev.AlgorithmSettings)
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
                            new Tuple<ComputeDevice, Queue<AlgorithmContainer>>(cDev, algorithmQueue)
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

        private static bool ShouldBenchmark(AlgorithmContainer algorithm)
        {
            var isBenchmarked = !algorithm.BenchmarkNeeded;
            switch (Selection)
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

#endregion

#region Start/Stop methods

        public static void Start(BenchmarkPerformanceType perfType, IBenchmarkForm form)
        {
            _benchForm = form;
            Start(perfType);
        }

        public static void Start(BenchmarkPerformanceType perfType)
        {
            _hasFailedAlgorithms = false;
            lock (_statusCheckAlgos)
            {
                _statusCheckAlgos.Clear();

                foreach (var pair in BenchDevAlgoQueue)
                {
                    BenchmarkingComputeDeviceHandler.BenchmarkDeviceAlgorithms(pair.Item1, pair.Item2, perfType);
                }
                InBenchmark = true;
            }
        }

        private static bool ShouldBenchmark(AlgorithmContainer algo, BenchmarkOption benchmarkOption)
        {
            switch (benchmarkOption)
            {
                case BenchmarkOption.ZeroOnly:
                    return algo.BenchmarkNeeded;
                case BenchmarkOption.ReBecnhOnly:
                    return algo.IsReBenchmark;
                case BenchmarkOption.ZeroOrReBenchOnly:
                    return algo.BenchmarkNeeded || algo.IsReBenchmark;
            }
            return true;
        }

        // from deleted NHM.Extensions
        private static Queue<T> ToQueue<T>(this IEnumerable<T> source)
        {
            var queue = new Queue<T>();
            foreach (var el in source)
            {
                queue.Enqueue(el);
            }
            return queue;
        }

        // network benchmark starts benchmarking on a device
        // assume device is enabled and it exists
        public static void StartBenchmarForDevice(ComputeDevice device, BenchmarkStartSettings benchmarkStartSettings)
        {
            var startMiningAfterBenchmark = benchmarkStartSettings.StartMiningAfterBenchmark;
            var perfType = benchmarkStartSettings.BenchmarkPerformanceType;
            var benchmarkOption = benchmarkStartSettings.BenchmarkOption;
            var unbenchmarkedAlgorithms = device.AlgorithmSettings.Where(algo => algo.Enabled && ShouldBenchmark(algo, benchmarkOption)).ToQueue();
            BenchmarkingComputeDeviceHandler.BenchmarkDeviceAlgorithms(device, unbenchmarkedAlgorithms, perfType, startMiningAfterBenchmark);
            InBenchmark = true;
        }

        public static void StopBenchmarForDevice(ComputeDevice device)
        {
            BenchmarkingComputeDeviceHandler.StopBenchmarkingDevice(device);
        }

        public static void StopBenchmarForDevices(IEnumerable<ComputeDevice> devices)
        {
            foreach (var device in devices)
            {
                BenchmarkingComputeDeviceHandler.StopBenchmarkingDevice(device);
            }
        }

        public static void Stop()
        {
            InBenchmark = false;
            BenchmarkingComputeDeviceHandler.StopBenchmarkingAllDevices();
        }

        private static void End()
        {
            InBenchmark = false;
            EthlargementIntegratedPlugin.Instance.Stop();
            OnBenchmarkEnd?.Invoke(null, new BenchEndEventArgs(_hasFailedAlgorithms, StartMiningOnFinish));
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

        public static void AddToStatusCheck(ComputeDevice device, AlgorithmContainer algorithm)
        {
            lock (_statusCheckAlgos)
            {
                _statusCheckAlgos[device] = algorithm;
            }
        }

        public static void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos, bool startMiningAfterBenchmark = false)
        {
            _hasFailedAlgorithms = failedAlgos || _hasFailedAlgorithms;
            
            if (!IsBenchmarking) End(); // TODO this line here can show a popup window

            if (startMiningAfterBenchmark)
            {
                ApplicationStateManager.StartDevice(device, true);
            }
        }

        public static void SetCurrentStatus(ComputeDevice dev, AlgorithmContainer algo, string status)
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
        public AlgorithmContainer Algorithm { get; }
        public string Status { get; }

        public AlgoStatusEventArgs(ComputeDevice dev, AlgorithmContainer algo, string status)
        {
            Device = dev;
            Algorithm = algo;
            Status = status;
        }
    }

    public class BenchEndEventArgs : EventArgs
    {
        public bool DidAlgosFail { get; }
        public bool StartMining { get; }

        public BenchEndEventArgs(bool algosFailed, bool startMining)
        {
            DidAlgosFail = algosFailed;
            StartMining = startMining;
        }
    }
}
