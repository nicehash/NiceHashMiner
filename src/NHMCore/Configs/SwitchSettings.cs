using NHM.Common;
using NHMCore.Switching;

namespace NHMCore.Configs
{
    public class SwitchSettings : NotifyChangedBase
    {
        public static SwitchSettings Instance { get; } = new SwitchSettings();
        private SwitchSettings() { }

        // Switching settings are not in GUI so we don't need notify property change
        public Interval SwitchSmaTicksStable = new Interval(2, 3);
        public Interval SwitchSmaTicksUnstable = new Interval(5, 13);

        private Interval _switchSmaTimeChangeSeconds = new Interval(34, 55);
        public Interval SwitchSmaTimeChangeSeconds
        {
            get => _switchSmaTimeChangeSeconds;
            set
            {
                _switchSmaTimeChangeSeconds = value;
                OnPropertyChanged(nameof(SwitchSmaTimeChangeSeconds));
            }
        }

        // TODO move this to mining profitability settings
        private double _kwhPrice = 0;
        public double KwhPrice
        {
            get => _kwhPrice;
            set
            {
                _kwhPrice = value;
                OnPropertyChanged(nameof(KwhPrice));
            }
        }
        private double _switchProfitabilityThreshold = 0.02; // percent
        public double SwitchProfitabilityThreshold
        {
            get => _switchProfitabilityThreshold;
            set
            {
                _switchProfitabilityThreshold = value;
                OnPropertyChanged(nameof(SwitchProfitabilityThreshold));
            }
        }
    }
}
