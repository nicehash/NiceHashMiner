using NHM.Common.Enums;
using NHMCore.Configs.Data;
using System.Collections.Generic;

namespace NHM.Wpf.ViewModels.Settings
{
    public class ProfitabilitySettingsVM : SettingsBaseVM
    {
        public IEnumerable<TimeUnitType> TimeUnits { get; } = GetEnumValues<TimeUnitType>();

        public bool MinProfitEnabled
        {
            get => !Config.MineRegardlessOfProfit;
            set
            {
                Config.MineRegardlessOfProfit = !value;
                OnPropertyChanged();
            }
        }

        public ProfitabilitySettingsVM(GeneralConfig confObj)
            : base(confObj, "Profitability")
        { }
    }
}
