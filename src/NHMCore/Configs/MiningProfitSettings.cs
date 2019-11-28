using NHM.Common;

namespace NHMCore.Configs
{
    public class MiningProfitSettings : NotifyChangedBase
    {
        public static MiningProfitSettings Instance { get; } = new MiningProfitSettings();

        private MiningProfitSettings()
        { }

        public bool IsMinimumProfitProfitEnabled => !MineRegardlessOfProfit;

        private double _minimumProfit = 0;
        public double MinimumProfit
        {
            get => _minimumProfit;
            set
            {
                _minimumProfit = value;
                OnPropertyChanged(nameof(MinimumProfit));
            }
        }

        private bool _mineRegardlessOfProfit = true;
        public bool MineRegardlessOfProfit
        {
            get => _mineRegardlessOfProfit;
            set
            {
                _mineRegardlessOfProfit = value;
                OnPropertyChanged(nameof(MineRegardlessOfProfit));
            }
        }
    }
}
