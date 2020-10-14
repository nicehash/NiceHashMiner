using NHM.Common;
using NHM.Common.Enums;
using NHMCore.ApplicationState;
using NHMCore.Nhmws;
using System.Drawing;

namespace NHMCore.Configs
{
    public class GUISettings : NotifyChangedBase
    {
        public static GUISettings Instance { get; } = new GUISettings();
        private GUISettings() { }

        public Size MainFormSize { get; set; } = new Size { Width = 800, Height = 600 };

        private bool _minimizeToTray = false;
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                _minimizeToTray = value;
                OnPropertyChanged(nameof(MinimizeToTray));
            }
        }

        private bool _showPowerColumns = false;
        public bool ShowPowerColumns
        {
            get => _showPowerColumns;
            set
            {
                _showPowerColumns = value;
                OnPropertyChanged(nameof(ShowPowerColumns));
            }
        }

        private bool _showDiagColumns = true;
        public bool ShowDiagColumns
        {
            get => _showDiagColumns;
            set
            {
                _showDiagColumns = value;
                OnPropertyChanged(nameof(ShowDiagColumns));
            }
        }

        private bool _guiWindowsAlwaysOnTop = false;
        public bool GUIWindowsAlwaysOnTop
        {
            get => _guiWindowsAlwaysOnTop;
            set
            {
                _guiWindowsAlwaysOnTop = value;
                OnPropertyChanged(nameof(GUIWindowsAlwaysOnTop));
            }
        }

        public string DisplayCurrency
        {
            get => BalanceAndExchangeRates.Instance.SelectedFiatCurrency;
            set
            {
                BalanceAndExchangeRates.Instance.SelectedFiatCurrency = value;
                OnPropertyChanged(nameof(DisplayCurrency));
            }
        }

        public TimeUnitType TimeUnit
        {
            get => TimeFactor.UnitType;
            set
            {
                TimeFactor.UnitType = value;
                OnPropertyChanged(nameof(TimeUnit));
            }
        }

        private bool _autoScaleBTCValues = true;
        public bool AutoScaleBTCValues
        {
            get => _autoScaleBTCValues;
            set
            {
                _autoScaleBTCValues = value;
                OnPropertyChanged(nameof(AutoScaleBTCValues));
            }
        }

        //public string Language
        //{
        //    get => TranslationsSettings.Instance.Language;
        //    set
        //    {
        //        TranslationsSettings.Instance.Language = value;
        //        OnPropertyChanged(nameof(Language));
        //    }
        //}

        private string _displayTheme = "Light";
        public string DisplayTheme
        {
            get => _displayTheme;
            set
            {
                _displayTheme = value;
                OnPropertyChanged(nameof(DisplayTheme));
            }
        }

        private bool _displayPureProfit = false;
        public bool DisplayPureProfit
        {
            get => _displayPureProfit;
            set
            {
                _displayPureProfit = value;
                OnPropertyChanged(nameof(DisplayPureProfit));
            }
        }
    }
}
