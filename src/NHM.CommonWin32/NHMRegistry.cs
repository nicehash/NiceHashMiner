using Microsoft.Win32;
using NHM.Common;
using System;

namespace NHM.CommonWin32
{
    public static class NHMRegistry
    {
        private static string NHM_SUBKEY => @"SOFTWARE\" + APP_GUID.GUID;
        private static bool EnsureNHMSubKeyCalled = false;
        private static void EnsureNHMSubKey()
        {
            if (EnsureNHMSubKeyCalled) return;
            EnsureNHMSubKeyCalled = true;
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, false);
                if (key == null) Registry.CurrentUser.CreateSubKey(NHM_SUBKEY);
            }
            catch (Exception e)
            {
                Logger.Error("NHMRegistry", $"EnsureNHMSubKey {e}");
            }
        }

        public static int GetSubKey(string subKey)
        {
            EnsureNHMSubKey();
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, false);
                var value = key?.GetValue(subKey) as string;
                if (Int32.TryParse(value, out int TOSver)) return TOSver;
                Logger.Warn("NHMRegistry", $"{subKey} was not read, defaulting to -1.");
                return -1;
            }
            catch (Exception e)
            {
                Logger.Error("NHMRegistry", $"GetSubKey {e}");
                return -1;
            }
        }

        public static RegistryKey GetSubKey(string subKey, bool writable)
        {
            using var key = Registry.CurrentUser.OpenSubKey(subKey, writable);
            return key;
        }

        public static void SetSubKey(string subKey, int value)
        {
            EnsureNHMSubKey();
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, true);
                key.SetValue(subKey, value.ToString());
            }
            catch (Exception e)
            {
                Logger.Error("NHMRegistry", $"SetSubKey {subKey} {e}");
            }
        }

    }
}
