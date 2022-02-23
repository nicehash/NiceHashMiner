using Microsoft.Win32;
using NHM.Common;
using System;

namespace NHM.CommonWin32
{
    public static class NHMRegistry
    {
        private static string NHM_SUBKEY => @"SOFTWARE\" + APP_GUID.GUID;
        private const string ValueFallback = "MachineGuidNhmGen";
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

        public static int GetSubKeyName(string keyName)
        {
            EnsureNHMSubKey();
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, false);
                var value = key?.GetValue(keyName) as string;
                if (Int32.TryParse(value, out int TOSver)) return TOSver;
                Logger.Warn("NHMRegistry", $"{keyName} was not read, defaulting to -1.");
                return -1;
            }
            catch (Exception e)
            {
                Logger.Error("NHMRegistry", $"GetSubKey {e}");
                return -1;
            }
        }

        public static RegistryKey GetSubKey(bool writable)
        {
            EnsureNHMSubKey();
            var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, writable);
            return key;
        }

        public static void SetSubKeyName(string keyName, int value)
        {
            EnsureNHMSubKey();
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, true);
                key.SetValue(keyName, value.ToString());
            }
            catch (Exception e)
            {
                Logger.Error("NHMRegistry", $"SetSubKey {keyName} {e}");
            }
        }

        public static string MachineGuidNhmGenGet()
        {
            using var rkFallback = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, true);
            var fallbackUUIDValue = rkFallback?.GetValue(ValueFallback, null);
            if (fallbackUUIDValue == null)
            {
                try
                {
                    var genUUID = MachineGuidNhmGenSet(rkFallback);
                    return genUUID;
                }
                catch (Exception e)
                {
                    //if registry fails do fallback to files
                    Logger.Error("NHMRegistry", $"Fallback SetValue: {e.Message}");
                    return "";
                }
            }
            else if (fallbackUUIDValue is string regUUID)
            {
                return regUUID;
            }
            return "";
        }

        public static string MachineGuidNhmGenSet(RegistryKey rkFallback)
        {
            var genUUID = Guid.NewGuid().ToString();
            rkFallback?.SetValue(ValueFallback, genUUID);

            return genUUID;
        }
    }
}
