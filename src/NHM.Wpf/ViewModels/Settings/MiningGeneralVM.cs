using NHM.Common.Enums;
using NHMCore.Configs.Data;
using System.Collections.Generic;

namespace NHM.Wpf.ViewModels.Settings
{
    public class MiningGeneralVM : SettingsBaseVM
    {
        public IEnumerable<IdleCheckType> IdleTypes { get; } = GetEnumValues<IdleCheckType>();

        public MiningGeneralVM(GeneralConfig confObj)
            : base(confObj, "Mining")
        { }
    }
}
