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
        private BenchmarkPerformanceType _selectedBenchmarkType = BenchmarkPerformanceType.Standard;
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

        //private void UpdateBenchPending()
        //{
        //    BenchesCompleted = 0;
        //    //BenchesPending = BenchmarkManager.CalcBenchDevAlgoQueue();
        //}

        //private void BenchmarkManagerOnInBenchmarkChanged(object sender, bool e)
        //{
        //    OnPropertyChanged(nameof(InBenchmark));
        //    OnPropertyChanged(nameof(NotInBenchmark));
        //    OnPropertyChanged(nameof(SideBarEnabled));
        //    OnPropertyChanged(nameof(StartStopButtonLabel));
        //    if (!e) BenchesCompleted = 0;
        //}

        public void StartBenchmark()
        {
            //if (Devices?.All(d => !d.Enabled) ?? true)
            //{
            //    MessageBox.Show(Translations.Tr("No device has been selected there is nothing to benchmark"),
            //        Translations.Tr("No device selected"),
            //        MessageBoxButton.OK);
            //    return;
            //}

            //if (!BenchmarkManager.HasWork)
            //{
            //    MessageBox.Show(Translations.Tr("Current benchmark settings are already executed. There is nothing to do."),
            //        Translations.Tr("Nothing to benchmark"),
            //        MessageBoxButton.OK);
            //    return;
            //}

            //Set pending status
            //foreach (var devAlgoTuple in BenchmarkManager.BenchDevAlgoQueue)
            //{
            //    foreach (var algo in devAlgoTuple.Item2) algo.SetBenchmarkPending();
            //}

            ApplicationStateManager.StartBenchmark();
        }

        public async void StopBenchmark()
        {
            await ApplicationStateManager.StopBenchmark();
        }
    }
}
