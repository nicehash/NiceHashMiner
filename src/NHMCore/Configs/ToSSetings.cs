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
            get { return NHMRegistry.GetSubKey(nameof(AgreedWithTOS)); }
            set
            {
                NHMRegistry.SetSubKey(nameof(AgreedWithTOS), value);
                OnPropertyChanged(nameof(AgreedWithTOS));
            }
        }

        public int Use3rdPartyMinersTOS
        {
            get { return NHMRegistry.GetSubKey(nameof(Use3rdPartyMinersTOS)); }
            set
            {
                NHMRegistry.SetSubKey(nameof(Use3rdPartyMinersTOS), value);
                OnPropertyChanged(nameof(Use3rdPartyMinersTOS));
            }
        }
    }
}
