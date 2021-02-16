using NHM.Common;

namespace NHMCore.Configs
{
    public class IFTTTSettings : NotifyChangedBase
    {
        public static IFTTTSettings Instance { get; } = new IFTTTSettings();

        private IFTTTSettings() { }

        private bool _useIFTTT = false;
        public bool UseIFTTT
        {
            get => _useIFTTT;
            set
            {
                _useIFTTT = value;
                OnPropertyChanged(nameof(UseIFTTT));
            }
        }

        private string _iFTTTKey = "";
        public string IFTTTKey
        {
            get => _iFTTTKey;
            set
            {
                _iFTTTKey = value;
                OnPropertyChanged(nameof(IFTTTKey));
            }
        }
    }
}
