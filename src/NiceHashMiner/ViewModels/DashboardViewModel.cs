using NHM.Common;
using NHMCore;
using NHMCore.ApplicationState;
using NHMCore.Configs;
using System.Linq;
using System.Windows;
using System.ComponentModel;

namespace NiceHashMiner.ViewModels
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
                ret += !_isRunning ? "Start Mining" : "";
                var statuses = new string[] { null, null };
                if (_isMining)
                {
                    statuses[0] = "Mining";
                }
                if (_isBenchmarking)
                {
                    statuses[1] = "Benchmarking";
                }
                ret += string.Join(" / ", statuses.Where(s => s != null));
                return Translations.Tr(ret);
            }
        }

        public string StatusToolTip
        {
            get
            {
                var ret = "";
                // Start/Stop
                ret += !_isRunning ? "Start " : "Stop ";
                var statuses = new string[] { null, null };
                if (_isMining)
                {
                    statuses[0] = "Mining";
                }
                if (_isBenchmarking)
                {
                    statuses[1] = "Benchmarking";
                }
                ret += string.Join(" / ", statuses.Where(s => s != null));
                return Translations.Tr(ret);
            }
        }

        public bool ShowProgress
        {
            get
            {
                return _isRunning || _isMining || _isBenchmarking;
            }
        }

        public Visibility CompleteBTCVisibility { get; private set; } = Visibility.Visible;

        private bool _isRunning = false;
        private bool _isMining = false;
        private bool _isBenchmarking = false;
        private static object _lock = new object();

        public DashboardViewModel()
        {
            CredentialsSettings.Instance.PropertyChanged += Instance_PropertyChanged;
            MiningState.Instance.PropertyChanged += MiningStateInstance_PropertyChanged;

            MiningStateInstance_PropertyChanged(this, null);
            CompleteBTCVisibility = CredentialsSettings.Instance.IsBitcoinAddressValid ? Visibility.Collapsed : Visibility.Visible;
            OnPropertyChanged(nameof(CompleteBTCVisibility));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusToolTip));
            OnPropertyChanged(nameof(ShowProgress));

            //if (MiningState.Instance.IsNotBenchmarkingOrMining)
            //{
            //    StatusText = "Stop Mining";
            //}
            //else
            //{

            //}
            //if (CredentialsSettings.Instance.IsBitcoinAddressValid && ) 
        }

        private void MiningStateInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (_lock)
            {
                _isRunning = MiningState.Instance.AnyDeviceRunning;
                _isMining = MiningState.Instance.MiningDeviceStateCount > 0;
                _isBenchmarking = MiningState.Instance.BenchmarkingDeviceStateCount > 0;
            }

            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusToolTip));
            OnPropertyChanged(nameof(ShowProgress));
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
