using NHM.Common;
using NHM.CommonWin32;
using System;

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
        public int AgreedWithTOS
        {
            get { return NHMRegistry.GetSubKeyName(nameof(AgreedWithTOS)); }
            set
            {
                NHMRegistry.SetSubKeyName(nameof(AgreedWithTOS), value);
                OnPropertyChanged(nameof(AgreedWithTOS));
            }
        }

        public int Use3rdPartyMinersTOS
        {
            get { return NHMRegistry.GetSubKeyName(nameof(Use3rdPartyMinersTOS)); }
            set
            {
                NHMRegistry.SetSubKeyName(nameof(Use3rdPartyMinersTOS), value);
                OnPropertyChanged(nameof(Use3rdPartyMinersTOS));
            }
        }
    }
}
