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

        public class FakeAlgo : INotifyPropertyChanged
        {
            private static readonly Random R = new Random();

            public string Name { get; }
            private bool _enabled;

            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                    PropertyChanged1();
                }
            }
            private double _hashrate = R.NextDouble();
            private double _secondaryHashrate = R.NextDouble();

            public double Hashrate
            {
                get => _hashrate;
                set
                {
                    _hashrate = value;
                    PropertyChanged1();
                    PropertyChanged1(nameof(Profit));
                }
            }

            public double SecondaryHashrate
            {
                get => _secondaryHashrate;
                set
                {
                    _secondaryHashrate = value;
                    PropertyChanged1();
                }
            }

            public double Paying { get; set; } = R.NextDouble();
            public double Profit => Paying * Hashrate;

            public double Power { get; set; }
            public bool IsDual { get; set; }

            public FakeAlgo(string name)
            {
                Name = name;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void PropertyChanged1([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<FakeDevice> Devices { get; }
        public ObservableCollection<FakeAlgo> SelectedAlgos { get; }

        /// <summary>
        /// True iff a device is selected and enabled
        /// </summary>
        public bool AlgosEnabled => SelectedDev?.Enabled ?? false;

        public bool SideBarEnabled => SelectedAlgo?.Enabled ?? false;

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

                OnPropertyChanged();
                OnPropertyChanged(nameof(AlgosEnabled));

                if (_selectedDev == null) return;

                // Add new handler
                _selectedDev.PropertyChanged += SelectedDevOnPropertyChanged;

                foreach (var algo in _selectedDev.Algos)
                {
                    SelectedAlgos.Add(algo);
                }
            }
        }

        private FakeAlgo _selectedAlgo;

        public FakeAlgo SelectedAlgo
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
                _selectedDev.PropertyChanged += SelectedAlgoOnPropertyChanged;
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
                new FakeAlgo("gpu algo 3") { IsDual = true }
            }));
        }
    }
}
