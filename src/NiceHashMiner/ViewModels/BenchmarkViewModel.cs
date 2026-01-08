using NHM.Common;
using NHM.Common.Enums;
using NHMCore;
using NHMCore.ApplicationState;
using System.ComponentModel;

namespace NiceHashMiner.ViewModels
{
    public class BenchmarkViewModel : NotifyChangedBase
    {

        #region BenchmarkSettings
        public BenchmarkPerformanceType SelectedBenchmarkType
        {
            get => BenchmarkManagerState.Instance.SelectedBenchmarkType;
            set => BenchmarkManagerState.Instance.SelectedBenchmarkType = value;
        }

        public BenchmarkPerformanceType Quick => BenchmarkPerformanceType.Quick;
        public BenchmarkPerformanceType Standard => BenchmarkPerformanceType.Standard;
        public BenchmarkPerformanceType Precise => BenchmarkPerformanceType.Precise;
        #endregion BenchmarkSettings

        public int BenchmarksPending => BenchmarkManagerState.Instance.BenchmarksPending;

        public string BenchmarksPendingStr => Translations.Tr("Pending Benchmarks: {0}", BenchmarkManagerState.Instance.BenchmarksPending);

        public bool HasBenchmarkWork => BenchmarkManagerState.Instance.HasBenchmarkWork;
        public bool CanStartBenchmaring => BenchmarkManagerState.Instance.HasBenchmarkWork && BenchmarkManagerState.Instance.CanStartBenchmarking;

        public bool CanStart => BenchmarkManagerState.Instance.CanStart;
        
        public bool StartEnabled => BenchmarkManagerState.Instance.StartEnabled;
        public bool CanStartAndButtonEnabled => CanStart && StartEnabled;
        public BenchmarkViewModel()
        {
            BenchmarkManagerState.Instance.PropertyChanged += Instance_PropertyChanged;
            OnPropertyChanged(nameof(HasBenchmarkWork));
            OnPropertyChanged(nameof(BenchmarksPending));
            OnPropertyChanged(nameof(BenchmarksPendingStr));
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStartBenchmaring));
            OnPropertyChanged(nameof(SelectedBenchmarkType));
            OnPropertyChanged(nameof(StartEnabled));
            OnPropertyChanged(nameof(CanStartAndButtonEnabled));
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BenchmarkManagerState.CanStart))
            {
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStartAndButtonEnabled));
            }
            if (e.PropertyName == nameof(BenchmarkManagerState.HasBenchmarkWork) || e.PropertyName == nameof(BenchmarkManagerState.CanStartBenchmarking))
            {
                OnPropertyChanged(nameof(HasBenchmarkWork));
                OnPropertyChanged(nameof(CanStartBenchmaring));
                OnPropertyChanged(nameof(CanStartAndButtonEnabled));
            }
            if (e.PropertyName == nameof(BenchmarkManagerState.BenchmarksPending))
            {
                OnPropertyChanged(nameof(BenchmarksPending));
                OnPropertyChanged(nameof(BenchmarksPendingStr));
                OnPropertyChanged(nameof(CanStartAndButtonEnabled));
            }
            if (e.PropertyName == nameof(BenchmarkManagerState.SelectedBenchmarkType))
            {
                OnPropertyChanged(nameof(SelectedBenchmarkType));
            }
            if(e.PropertyName == nameof(BenchmarkManagerState.StartEnabled))
            {
                OnPropertyChanged(nameof(StartEnabled));
                OnPropertyChanged(nameof(CanStartAndButtonEnabled));
            }
        }

        public void StartBenchmark()
        {
            ApplicationStateManager.StartBenchmark();
        }

        public async void StopBenchmark()
        {
            await ApplicationStateManager.StopBenchmark();
        }
    }
}
