﻿using Microsoft.Win32;
using NHM.Common;
using NHM.Common.Enums;
using System;
using System.Reflection.Metadata.Ecma335;

namespace NHM.CommonWin32
{
    public static class NHMRegistry
    {
        private static string NHM_SUBKEY => @"SOFTWARE\" + APP_GUID.GUID;
        private static string QM_SUBKEY => @"SOFTWARE\NiceHash QuickMiner";
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

        public static int GetValueForKeyName(string keyName)
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
                Logger.Error("NHMRegistry", $"GetSubKey {keyName} {e}");
                return -1;
            }
        }

        public static void SetValueForKeyName(string keyName, int value)
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
            EnsureNHMSubKey();
            const string machineGuidFallbackKeyName = "MachineGuidNhmGen";
            try
            {
                using var rkFallback = Registry.CurrentUser.OpenSubKey(NHM_SUBKEY, true);
                var fallbackUUIDValue = rkFallback?.GetValue(machineGuidFallbackKeyName, null);
                if (fallbackUUIDValue is string regUUID) return regUUID;
                var genUUID = Guid.NewGuid().ToString();
                rkFallback?.SetValue(machineGuidFallbackKeyName, genUUID);
                return genUUID;
            }
            catch (Exception e)
            {
                //if registry fails do fallback to files
                Logger.Error("NHMRegistry", $"Fallback SetValue: {e.Message}");
                return "";
            }
        }

        public static string Get_QM_MiningaddressFromRegistry()
        {
            try
            {
                using var parent = Registry.LocalMachine.OpenSubKey(@"SOFTWARE", false);
                using var rkey = parent?.OpenSubKey("NiceHash QuickMiner", false);
                object obtc = rkey?.GetValue("MiningAddress");
                return obtc as string;
            }
            catch (Exception ex)
            {
                Logger.Warn("NHMRegistry", $"Exception Get_QM_MiningaddressFromRegistry {ex.Message}");
            }
            return null;
        }

        public static void Set_QM_MiningaddressFromRegistry(string btc)
        {
            try
            {
                if (BuildOptions.BUILD_TAG != BuildTag.PRODUCTION) return;
                var key = Registry.LocalMachine.OpenSubKey(QM_SUBKEY, true);
                if (key == null)
                {
                    Registry.LocalMachine.CreateSubKey(QM_SUBKEY);
                    key = Registry.LocalMachine.OpenSubKey(QM_SUBKEY, true);
                }
                key.SetValue("MiningAddress", btc);
                key.Close();
            }
            catch (Exception ex)
            {
                Logger.Warn("NHMRegistry", $"Exception Set_QM_MiningaddressFromRegistry {ex.Message}");
            }
        }
    }
}
