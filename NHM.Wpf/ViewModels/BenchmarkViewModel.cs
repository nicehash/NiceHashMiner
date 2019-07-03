using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NHM.Wpf.Annotations;

namespace NHM.Wpf.ViewModels
{
    public class BenchmarkViewModel : BaseVM
    {
        public class FakeDevice : INotifyPropertyChanged
        {
            private bool _enabled;
            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                    OnPropertyChanged();
                }
            }

            public string Name { get; }

            public IReadOnlyList<FakeAlgo> Algos { get; }

            public FakeDevice(string name, IReadOnlyList<FakeAlgo> algos)
            {
                Name = name;
                Algos = algos;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class FakeAlgo
        {
            private static readonly Random R = new Random();

            public string Name { get; }
            public bool Enabled { get; set; }
            public double Hashrate { get; set; } = R.NextDouble();
            public double SecondaryHashrate { get; set; } = R.NextDouble();
            public double Paying { get; set; } = R.NextDouble();
            public double Profit { get; set; } = R.NextDouble();

            public FakeAlgo(string name)
            {
                Name = name;
            }
        }

        public ObservableCollection<FakeDevice> Devices { get; }
        public ObservableCollection<FakeAlgo> SelectedAlgos { get; }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;

                SelectedDev = value >= 0 ? Devices[value] : null;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True iff a device is selected and enabled
        /// </summary>
        public bool AlgosEnabled => SelectedDev?.Enabled ?? false;

        private FakeDevice _selectedDev;
        public FakeDevice SelectedDev
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

                if (_selectedDev == null) return;

                // Add new handler
                _selectedDev.PropertyChanged += SelectedDevOnPropertyChanged;

                foreach (var algo in _selectedDev.Algos)
                {
                    SelectedAlgos.Add(algo);
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(AlgosEnabled));
            }
        }

        private void SelectedDevOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Forward device enabled change notification to AlgosEnabled
            if (e.PropertyName == nameof(_selectedDev.Enabled))
                OnPropertyChanged(nameof(AlgosEnabled));
        }

        public BenchmarkViewModel()
        {
            Devices = new ObservableCollection<FakeDevice>();
            SelectedAlgos = new ObservableCollection<FakeAlgo>();
            RefreshData();
        }

        public void RefreshData()
        {
            Devices.Add(new FakeDevice("CPU", new List<FakeAlgo>
            {
                new FakeAlgo("CPU algo 1"),
                new FakeAlgo("Cpu algo 2")
            }));
            Devices.Add(new FakeDevice("GPU", new List<FakeAlgo>
            {
                new FakeAlgo("GPU algo 1"),
                new FakeAlgo("GPu algo 2"),
                new FakeAlgo("gpu algo 3")
            }));
        }
    }
}
