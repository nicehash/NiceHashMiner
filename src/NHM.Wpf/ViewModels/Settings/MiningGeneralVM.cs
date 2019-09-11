using NHM.Common.Enums;
using NHMCore.Configs.Data;
using System;
using System.Collections.Generic;

namespace NHM.Wpf.ViewModels.Settings
{
    public class MiningGeneralVM : SettingsBaseVM
    {
        public IEnumerable<IdleCheckType> IdleTypes { get; } = GetEnumValues<IdleCheckType>();

        public bool Enable3rdPartyCheck
        {
            get => Config.Use3rdPartyMiners == Use3rdPartyMiners.YES;
            set
            {
                Config.Use3rdPartyMiners = value ? Use3rdPartyMiners.YES : Use3rdPartyMiners.NO;
            }
        }

        public MiningGeneralVM(GeneralConfig confObj)
            : base(confObj, "Mining")
        { }
    }
}
