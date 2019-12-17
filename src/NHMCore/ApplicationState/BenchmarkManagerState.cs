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

        private void SetStatus(AlgorithmContainer algorithmContainer)
        {
            lock (_lock)
            {
                var key = GetDeviceAlgorithmVersionKey(algorithmContainer);
                _algorithmsBenchmarksStates[key] = _benchmarkStatuses.Contains(algorithmContainer.Status);
                _benchmarksPending = _algorithmsBenchmarksStates.Values.Where(benchStatus => benchStatus).Count();
                OnPropertyChanged(nameof(BenchmarksPending));
                OnPropertyChanged(nameof(HasBenchmarkWork));
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

        private BenchmarkPerformanceType _selectedBenchmarkType = BenchmarkPerformanceType.Standard;
        public BenchmarkPerformanceType SelectedBenchmarkType
        {
            get
            {
                lock (_lock)
                {
                    return _selectedBenchmarkType;
                }
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
                lock (_lock)
                {
                    return _benchmarksPending;
                }
            }
        }

        public bool HasBenchmarkWork
        {
            get
            {
                lock (_lock)
                {
                    return _benchmarksPending > 0;
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
                _benchmarksPending = _algorithmsBenchmarksStates.Values.Where(benchStatus => benchStatus).Count();
                OnPropertyChanged(nameof(BenchmarksPending));
                OnPropertyChanged(nameof(HasBenchmarkWork));
            }
        }

    }
}
