using System.ComponentModel;
using NHM.Common.Enums;
using NHMCore.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class AdvancedMiningVM : SettingsBaseVM
    {
        public bool EthlargementAvailable => IsElevated && Config.Use3rdPartyMiners == Use3rdPartyMiners.YES;

        public AdvancedMiningVM(GeneralConfig confObj)
            : base(confObj, "Mining")
        { }

        protected override void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.Config_PropertyChanged(sender, e);

            if (e.PropertyName == nameof(Config.Use3rdPartyMiners))
                OnPropertyChanged(nameof(EthlargementAvailable));
        }
    }
}
