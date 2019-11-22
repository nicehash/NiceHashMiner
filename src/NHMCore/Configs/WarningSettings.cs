using NHM.Common;

namespace NHMCore.Configs
{
    public class WarningSettings : NotifyChangedBase
    {
        public static WarningSettings Instance { get; } = new WarningSettings();
        private WarningSettings() { }

        private bool _showDriverVersionWarning = true;
        public bool ShowDriverVersionWarning
        {
            get => _showDriverVersionWarning;
            set
            {
                _showDriverVersionWarning = value;
                OnPropertyChanged(nameof(ShowDriverVersionWarning));
            }
        }

        private bool _disableWindowsErrorReporting = true;
        public bool DisableWindowsErrorReporting
        {
            get => _disableWindowsErrorReporting;
            set
            {
                _disableWindowsErrorReporting = value;
                OnPropertyChanged(nameof(DisableWindowsErrorReporting));
            }
        }

        private bool _showInternetConnectionWarning = true;
        public bool ShowInternetConnectionWarning
        {
            get => _showInternetConnectionWarning;
            set
            {
                _showInternetConnectionWarning = value;
                OnPropertyChanged(nameof(ShowInternetConnectionWarning));
            }
        }
    }
}
