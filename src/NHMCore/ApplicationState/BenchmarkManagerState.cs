using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Mining;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NHMCore.ApplicationState
{
    public class BenchmarkManagerState : NotifyChangedBase
    {
        public static BenchmarkManagerState Instance { get; } = new BenchmarkManagerState();
        private BenchmarkManagerState() { }

        private static readonly object _lock = new object();
        // we will have keys and true/false for benchmarks pending. 
        private static readonly Dictionary<string, bool> _algorithmsBenchmarksStates = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> _algorithmsCanStartStates = new Dictionary<string, bool>();

        private static string GetDeviceAlgorithmVersionKey(AlgorithmContainer algorithmContainer)
        {
            var deviceUUID = algorithmContainer.ComputeDevice.Uuid;
            var algoStrId = algorithmContainer.AlgorithmStringID;
            var version = $"{algorithmContainer.PluginVersion.Major}.{algorithmContainer.PluginVersion.Minor}";
            return $"{deviceUUID}-{algoStrId}-{version}";
        }

        private static AlgorithmStatus[] _benchmarkStatuses = new AlgorithmStatus[4] {
            AlgorithmStatus.NoBenchmark,
            AlgorithmStatus.ReBenchmark,
            AlgorithmStatus.BenchmarkPending,
            AlgorithmStatus.Benchmarking
        };

        private static AlgorithmStatus[] _startStatuses = new AlgorithmStatus[6] {
            AlgorithmStatus.NoBenchmark,
            AlgorithmStatus.Benchmarked,
            AlgorithmStatus.ReBenchmark,
            AlgorithmStatus.BenchmarkPending,
            AlgorithmStatus.Benchmarking,
            AlgorithmStatus.Mining,
        };

        private void SetStatus(AlgorithmContainer algorithmContainer)
        {
            lock (_lock)
            {
                var key = GetDeviceAlgorithmVersionKey(algorithmContainer);
                _algorithmsBenchmarksStates[key] = _benchmarkStatuses.Contains(algorithmContainer.Status) && algorithmContainer.ComputeDevice.Enabled;
                _algorithmsCanStartStates[key] = _startStatuses.Contains(algorithmContainer.Status) && algorithmContainer.ComputeDevice.Enabled;
                _benchmarksPending = _algorithmsBenchmarksStates.Values.Where(benchStatus => benchStatus).Count();
                _canStartCount = _algorithmsCanStartStates.Values.Where(canStartStatus => canStartStatus).Count();
                OnPropertyChanged(nameof(BenchmarksPending));
                OnPropertyChanged(nameof(HasBenchmarkWork));
                OnPropertyChanged(nameof(CanStart));
                var anyToBench = _algorithmsBenchmarksStates.Where(benchStatus => benchStatus.Key.Contains(algorithmContainer.ComputeDevice.Uuid)).Where(pair => pair.Value).Count();
                _deviceCanStartBenchmarkingStates[algorithmContainer.ComputeDevice.Uuid] = algorithmContainer.ComputeDevice.State == DeviceState.Stopped && anyToBench > 0;
                OnPropertyChanged(nameof(CanStartBenchmarking));
            }
        }

        public void ComputeDeviceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var computeDevice = sender as ComputeDevice;
            if (computeDevice == null) return;
            if (e.PropertyName == nameof(ComputeDevice.Enabled))
            {
                foreach (var algorithmContainer in computeDevice.AlgorithmSettings) SetStatus(algorithmContainer);
            }
            if (e.PropertyName == nameof(ComputeDevice.State))
            {
                lock (_lock)
                {
                    var anyToBench = _algorithmsBenchmarksStates.Where(benchStatus => benchStatus.Key.Contains(computeDevice.Uuid)).Where(pair => pair.Value).Count();
                    _deviceCanStartBenchmarkingStates[computeDevice.Uuid] = computeDevice.State == DeviceState.Stopped && anyToBench > 0;
                    OnPropertyChanged(nameof(CanStartBenchmarking));
                }
            }
        }

        private void AlgorithmContainerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var algorithmContainer = sender as AlgorithmContainer;
            if (algorithmContainer == null) return;
            if (e.PropertyName == nameof(AlgorithmContainer.Status))
            {
                SetStatus(algorithmContainer);
            }
        }

        private int _benchmarksPending = 0;
        private int _canStartCount = 0;
        private Dictionary<string, bool> _deviceCanStartBenchmarkingStates = new Dictionary<string, bool>();

        private BenchmarkPerformanceType _selectedBenchmarkType = BenchmarkPerformanceType.Standard;
        public BenchmarkPerformanceType SelectedBenchmarkType
        {
            get
            {
                lock (_lock) return _selectedBenchmarkType;
            }
            set
            {
                lock (_lock)
                {
                    _selectedBenchmarkType = value;
                    OnPropertyChanged(nameof(SelectedBenchmarkType));
                }
            }
        }

        public int BenchmarksPending
        {
            get
            {
                lock (_lock) return _benchmarksPending;
            }
        }

        public bool HasBenchmarkWork
        {
            get
            {
                lock (_lock) return _benchmarksPending > 0;
            }
        }

        public bool CanStart
        {
            get
            {
                lock (_lock) return _canStartCount > 0;
            }
        }

        public bool CanStartBenchmarking
        {
            get
            {
                lock (_lock)
                {
                    return _deviceCanStartBenchmarkingStates.Any(canStart => canStart.Value);
                }
            }
        }

        internal void AddAlgorithmContainer(AlgorithmContainer algorithmContainer)
        {
            lock (_lock)
            {
                algorithmContainer.PropertyChanged += AlgorithmContainerOnPropertyChanged;
                SetStatus(algorithmContainer);
            }
        }

        internal void RemoveAlgorithmContainer(AlgorithmContainer algorithmContainer)
        {
            lock (_lock)
            {
                algorithmContainer.PropertyChanged -= AlgorithmContainerOnPropertyChanged;
                var key = GetDeviceAlgorithmVersionKey(algorithmContainer);
                _algorithmsBenchmarksStates.Remove(key);
                _algorithmsCanStartStates.Remove(key);
                _benchmarksPending = _algorithmsBenchmarksStates.Values.Where(benchStatus => benchStatus).Count();
                _canStartCount = _algorithmsCanStartStates.Values.Where(canStartStatus => canStartStatus).Count();
                OnPropertyChanged(nameof(BenchmarksPending));
                OnPropertyChanged(nameof(HasBenchmarkWork));
                OnPropertyChanged(nameof(CanStart));
                var anyToBench = _algorithmsBenchmarksStates.Where(benchStatus => benchStatus.Key.Contains(algorithmContainer.ComputeDevice.Uuid)).Where(pair => pair.Value).Count();
                _deviceCanStartBenchmarkingStates[algorithmContainer.ComputeDevice.Uuid] = algorithmContainer.ComputeDevice.State == DeviceState.Stopped && anyToBench > 0;
                OnPropertyChanged(nameof(CanStartBenchmarking));
            }
        }

    }
}
