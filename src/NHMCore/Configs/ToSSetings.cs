using Microsoft.Win32;
using NHM.Common;
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
            get {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, false))
                {
                    if (key != null && key.GetValue("AgreedWithTOS") is string TOSVersion && 
                        Int32.TryParse(TOSVersion, out int TOSver))
                    {
                        return TOSver;
                    }
                    Logger.Warn("TOSSETTINGS", "AgreedWithTOS was not read, defaulting to -1.");
                    return -1;
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, true))
                {
                    key.SetValue("AgreedWithTOS", value.ToString());
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
                    if (key != null && key.GetValue("Use3rdPartyMinersTOS") is string TOS3rdPartyVersion && 
                        Int32.TryParse(TOS3rdPartyVersion, out int TOSver))
                    {
                        return TOSver;
                    }
                    Logger.Warn("TOSSETTINGS", "Use3rdPartyMinersTOS was not read, defaulting to -1.");
                    return -1;
                }
            }
            set
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, true))
                {
                    key.SetValue("Use3rdPartyMinersTOS", value.ToString());
                }
                OnPropertyChanged(nameof(Use3rdPartyMinersTOS));
            }
        }

    }
}
