using NiceHashMiner.Miners.IntegratedPlugins;
using NiceHashMiner.Utils;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Configs
{
    public class ThirdPartyMinerSettings : INotifyPropertyChanged
    {
        public static ThirdPartyMinerSettings Instance { get; } = new ThirdPartyMinerSettings();

        private ThirdPartyMinerSettings()
        {
            _prop = new NotifyPropertyChangedHelper<Use3rdPartyMiners>(NotifyPropertyChanged);
            _boolProps = new NotifyPropertyChangedHelper<bool>(NotifyPropertyChanged);
            Use3rdPartyMiners = Use3rdPartyMiners.NOT_SET;

        }
        private readonly NotifyPropertyChangedHelper<Use3rdPartyMiners> _prop;
        private readonly NotifyPropertyChangedHelper<bool> _boolProps;

        public Use3rdPartyMiners Use3rdPartyMiners
        {
            get => _prop.Get(nameof(Use3rdPartyMiners));
            set
            {
                _prop.Set(nameof(Use3rdPartyMiners), value);
                Enabled3rdPartyMiners = value == Use3rdPartyMiners.YES;
            }
        }

        public bool Enabled3rdPartyMiners
        {
            get => _boolProps.Get(nameof(Enabled3rdPartyMiners));
            set
            {
                _boolProps.Set(nameof(Enabled3rdPartyMiners), value);
                CanUseEthlargement = value;
                if (Use3rdPartyMiners.NOT_SET == Use3rdPartyMiners) return;
                var enumVal = value ? Use3rdPartyMiners.YES : Use3rdPartyMiners.NO;
                _prop.Set(nameof(Use3rdPartyMiners), enumVal);
            }
        }

        // this is 3rd party miner so we are putting this here
        public bool UseEthlargement
        {
            get => _boolProps.Get(nameof(UseEthlargement));
            set
            {
                var setValue = Enabled3rdPartyMiners && value;
                _boolProps.Set(nameof(UseEthlargement), setValue);
                EthlargementIntegratedPlugin.Instance.ServiceEnabled = CanUseEthlargement && setValue;
            }
        }

        public bool CanUseEthlargement
        {
            get => _boolProps.Get(nameof(CanUseEthlargement));
            set
            {
                var setValue = Helpers.IsElevated && value;
                _boolProps.Set(nameof(CanUseEthlargement), setValue);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
