using NHM.Common;
using NHM.Common.Enums;
using NHMCore;
using NHMCore.ApplicationState;
using System.ComponentModel;

namespace NHM.Wpf.ViewModels
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

        public string BenchmarksPendingStr => Translations.Tr("Pending Benchmarks {0}", BenchmarkManagerState.Instance.BenchmarksPending);

        public bool HasBenchmarkWork => BenchmarkManagerState.Instance.HasBenchmarkWork;
        public bool CanStartBenchmaring => BenchmarkManagerState.Instance.HasBenchmarkWork;

        public BenchmarkViewModel()
        {
            BenchmarkManagerState.Instance.PropertyChanged += Instance_PropertyChanged;
            OnPropertyChanged(nameof(HasBenchmarkWork));
            OnPropertyChanged(nameof(BenchmarksPending));
            OnPropertyChanged(nameof(BenchmarksPendingStr));
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BenchmarkManagerState.HasBenchmarkWork))
            {
                OnPropertyChanged(nameof(HasBenchmarkWork));
            }
            if (e.PropertyName == nameof(BenchmarkManagerState.BenchmarksPending))
            {
                OnPropertyChanged(nameof(BenchmarksPending));
                OnPropertyChanged(nameof(BenchmarksPendingStr));
            }
            if (e.PropertyName == nameof(BenchmarkManagerState.SelectedBenchmarkType))
            {
                OnPropertyChanged(nameof(SelectedBenchmarkType));
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
