using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Common.Enums;
using NiceHashMiner.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class ProfitabilitySettingsVM : SettingsBaseVM
    {
        public IEnumerable<TimeUnitType> TimeUnits { get; }

        public ProfitabilitySettingsVM(GeneralConfig confObj)
            : base(confObj, "Profitability")
        {
            TimeUnits = Enum.GetValues(typeof(TimeUnitType)).Cast<TimeUnitType>().ToList();
        }
    }
}
