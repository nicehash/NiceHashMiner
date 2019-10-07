using NHMCore.Configs.Data;
using System.ComponentModel;

namespace NHM.Wpf.ViewModels.Settings
{
    public class AdvancedMiningVM : SettingsBaseVM
    {
        public bool EthlargementAvailable => IsElevated;

        public AdvancedMiningVM(GeneralConfig confObj)
            : base(confObj, "Mining")
        { }

        protected override void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.Config_PropertyChanged(sender, e);

            //if (e.PropertyName == nameof(Config.Use3rdPartyMiners))
            //    OnPropertyChanged(nameof(EthlargementAvailable));
        }
    }
}
