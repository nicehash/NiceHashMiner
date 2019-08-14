using NHM.Common.Algorithm;
using NiceHashMiner.Configs;
using NiceHashMiner.Mining;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NHM.Wpf.ViewModels
{
    public class BenchmarkViewModel : BaseVM
    {
        #region ListView selection

        private IEnumerable<ComputeDevice> _devices;

        public IEnumerable<ComputeDevice> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                SelectedDev = null;
                SelectedAlgo = null;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AlgorithmContainer> SelectedAlgos { get; }

        /// <summary>
        /// True iff a device is selected and enabled
        /// </summary>
        public bool AlgosEnabled => SelectedDev?.Enabled ?? false;

        public bool SideBarEnabled => SelectedAlgo?.Enabled ?? false;

        private ComputeDevice _selectedDev;
        public ComputeDevice SelectedDev
        {
            get => _selectedDev;
            set
            {
                if (value == _selectedDev) return;

                // Remove old handler
                if (_selectedDev != null)
                    _selectedDev.PropertyChanged -= SelectedDevOnPropertyChanged;

                _selectedDev = value;
                SelectedAlgos.Clear();

                OnPropertyChanged();
                OnPropertyChanged(nameof(AlgosEnabled));

                if (_selectedDev == null) return;

                // Add new handler
                _selectedDev.PropertyChanged += SelectedDevOnPropertyChanged;

                foreach (var algo in _selectedDev.AlgorithmSettings)
                {
                    SelectedAlgos.Add(algo);
                }
            }
        }

        private AlgorithmContainer _selectedAlgo;

        public AlgorithmContainer SelectedAlgo
        {
            get => _selectedAlgo;
            set
            {
                if (value == _selectedAlgo) return;

                // Remove old handler
                if (_selectedAlgo != null)
                    _selectedAlgo.PropertyChanged -= SelectedAlgoOnPropertyChanged;

                _selectedAlgo = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(SideBarEnabled));

                if (_selectedAlgo == null) return;

                // Add new handler
                _selectedAlgo.PropertyChanged += SelectedAlgoOnPropertyChanged;
            }
        }

        private void SelectedAlgoOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_selectedAlgo.Enabled))
                OnPropertyChanged(nameof(SideBarEnabled));
        }

        private void SelectedDevOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Forward device enabled change notification to AlgosEnabled
            if (e.PropertyName == nameof(_selectedDev.Enabled))
                OnPropertyChanged(nameof(AlgosEnabled));
        }

        #endregion

        public BenchmarkViewModel()
        {
            SelectedAlgos = new ObservableCollection<AlgorithmContainer>();
        }

        public void CommitBenchmarks()
        {
            ConfigManager.CommitBenchmarks();
        }
    }
}
