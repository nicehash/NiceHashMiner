using NHM.Common;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;

namespace NHM.Wpf.ViewModels
{
    public class DashboardViewModel : NotifyChangedBase
    {
        public bool IsBitcoinAddressValid { get; private set; }
        public string StatusText
        {
            get
            {
                var ret = "";
                // Start/Stop
                ret += !_isRunning ? "Start " : "";
                var statuses = new string[] { null, null };
                if (_isMining)
                {
                    statuses[0] = "Mining";
                }
                if (_isBenchmarking)
                {
                    statuses[1] = "Benchmarking";
                }
                ret += string.Join("/", statuses.Where(s => s != null));
                if (_isMining || _isBenchmarking)
                {
                    ret += $" {new string('.', _dots)}";
                }
                return ret;
            }
        }

        public Visibility CompleteBTCVisibility { get; private set; } = Visibility.Visible;


        private readonly Timer _updateTimer;

        private bool _isRunning = false;
        private bool _isMining = false;
        private bool _isBenchmarking = false;
        private int _dots = 0;
        private static object _lock = new object();

        public DashboardViewModel()
        {
            CredentialsSettings.Instance.PropertyChanged += Instance_PropertyChanged;
            MiningState.Instance.PropertyChanged += MiningStateInstance_PropertyChanged;

            MiningStateInstance_PropertyChanged(this, null);
            CompleteBTCVisibility = CredentialsSettings.Instance.IsBitcoinAddressValid ? Visibility.Collapsed : Visibility.Visible;
            OnPropertyChanged(nameof(CompleteBTCVisibility));


            _updateTimer = new Timer(1000);
            _updateTimer.Elapsed += (s,e) =>
            {
                lock (_lock)
                {
                    if (_isMining || _isBenchmarking)
                    {
                        _dots = (_dots + 1) % 4;
                        OnPropertyChanged(nameof(StatusText));
                    }
                    else if (_dots != 0)
                    {
                        _dots = 0;
                        OnPropertyChanged(nameof(StatusText));
                    }
                }
            };
            _updateTimer.Start();

            //if (MiningState.Instance.IsNotBenchmarkingOrMining)
            //{
            //    StatusText = "Stop Mining";
            //}
            //else
            //{

            //}
            //if (CredentialsSettings.Instance.IsBitcoinAddressValid && ) 
        }

        private void MiningStateInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lock (_lock)
            {
                _isRunning = MiningState.Instance.AnyDeviceRunning;
                _isMining = MiningState.Instance.MiningDeviceStateCount > 0;
                _isBenchmarking = MiningState.Instance.BenchmarkingDeviceStateCount > 0;
            }

            OnPropertyChanged(nameof(StatusText));
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (e.PropertyName == nameof(CredentialsSettings.IsBitcoinAddressValid))
            {
                lock (_lock)
                {
                    CompleteBTCVisibility = CredentialsSettings.Instance.IsBitcoinAddressValid ? Visibility.Collapsed : Visibility.Visible;
                }
                OnPropertyChanged(nameof(CompleteBTCVisibility));
            }
        }
    }
}
