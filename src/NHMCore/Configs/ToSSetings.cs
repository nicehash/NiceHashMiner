using Microsoft.Win32;
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

        public int AgreedWithTOS
        {
            get {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, false))
                {
                    if (key != null && key.GetValue("AgreedWithTOS") is int TOSVersion)
                    {
                        return TOSVersion;
                    }
                    return -1;
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, true))
                {
                    key.SetValue("AgreedWithTOS", value);
                }
                OnPropertyChanged(nameof(AgreedWithTOS));
            }
        }

        public int Use3rdPartyMinersTOS
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, false))
                {
                    if (key != null && key.GetValue("Use3rdPartyMinersTOS") is int TOS3rdPartyVersion)
                    {
                        return TOS3rdPartyVersion;
                    }
                    return -1;
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, true))
                {
                    key.SetValue("Use3rdPartyMinersTOS", value);
                }
                OnPropertyChanged(nameof(Use3rdPartyMinersTOS));
            }
        }

    }
}
