using NHM.Common.Enums;
using NHMCore.Configs.Data;
using System.Collections.Generic;

namespace NHM.Wpf.ViewModels.Settings
{
    public class ProfitabilitySettingsVM : SettingsBaseVM
    {
        public IEnumerable<TimeUnitType> TimeUnits { get; } = GetEnumValues<TimeUnitType>();

        public ProfitabilitySettingsVM(GeneralConfig confObj)
            : base(confObj, "Profitability")
        { }
    }
}
