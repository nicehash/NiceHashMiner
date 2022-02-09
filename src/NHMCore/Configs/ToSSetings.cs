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

        private static string NHM_SUBKEY => @"SOFTWARE\" + APP_GUID.GUID;
        private static bool EnsureNHMSubKeyCalled = false;
        private static void EnsureNHMSubKey()
        {
            if (EnsureNHMSubKeyCalled) return;
            EnsureNHMSubKeyCalled = true;
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, false))
                {
                    if (key == null) Registry.CurrentUser.CreateSubKey(NHM_SUBKEY);
                }
            }
            catch (Exception e)
            {
                Logger.Error("TOSSETTINGS", $"EnsureNHMSubKey {e}");
            }        
        }

        private static int GetSubKey(string subKey)
        {
            EnsureNHMSubKey();
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, false))
                {
                    var value = key?.GetValue(subKey) as string;
                    if (Int32.TryParse(value, out int TOSver)) return TOSver;
                    Logger.Warn("TOSSETTINGS", $"{subKey} was not read, defaulting to -1.");
                    return -1;
                }
            }
            catch (Exception e)
            {
                Logger.Error("TOSSETTINGS", $"GetSubKey {e}");
                return -1;
            }
        }

        private static void SetSubKey(string subKey, int value)
        {
            EnsureNHMSubKey();
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, true))
                {
                    key.SetValue(subKey, value.ToString());
                }
            }
            catch (Exception e)
            {
                Logger.Error("TOSSETTINGS", $"SetSubKey {subKey} {e}");
            }
        }

        public int AgreedWithTOS
        {
            get { return GetSubKey(nameof(AgreedWithTOS)); }
            set
            {
                SetSubKey(nameof(AgreedWithTOS), value);
                OnPropertyChanged(nameof(AgreedWithTOS));
            }
        }

        public int Use3rdPartyMinersTOS
        {
            get { return GetSubKey(nameof(Use3rdPartyMinersTOS)); }
            set
            {
                SetSubKey(nameof(Use3rdPartyMinersTOS), value);
                OnPropertyChanged(nameof(Use3rdPartyMinersTOS));
            }
        }
    }
}
