using NHM.Common;

namespace NHMCore.Configs
{
    public class ToSSetings : NotifyChangedBase
    {
        public static ToSSetings Instance { get; } = new ToSSetings();

        private ToSSetings() { }

        private string _hwid = "";
        public string Hwid
        {
            get => _hwid;
            set
            {
                _hwid = value;
                OnPropertyChanged(nameof(Hwid));
            }
        }

        private int _agreedWithTOS = 0;
        public int AgreedWithTOS
        {
            get => _agreedWithTOS;
            set
            {
                _agreedWithTOS = value;
                OnPropertyChanged(nameof(AgreedWithTOS));
            }
        }

        private int _use3rdPartyMinersTOS = 0;
        public int Use3rdPartyMinersTOS
        {
            get => _use3rdPartyMinersTOS;
            set
            {
                _use3rdPartyMinersTOS = value;
                OnPropertyChanged(nameof(Use3rdPartyMinersTOS));
            }
        }

    }
}
