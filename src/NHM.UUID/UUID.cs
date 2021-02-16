using Microsoft.Win32;
using NHM.Common;
using System;
using System.IO;
using System.Management;

namespace NHM.UUID
{
    public static class UUID
    {
        private static System.Guid _defaultNamespace = Guid.UUID.Nil().AsGuid();
        public static string GetHexUUID(string infoToHashed)
        {
            var uuidHex = Guid.UUID.V5(_defaultNamespace, infoToHashed).AsGuid().ToString();
            return uuidHex;
        }

        public static string GetB64UUID(string hexUUID)
        {
            var uuid = Guid.UUID.V5(_defaultNamespace, hexUUID);
            var b64 = Convert.ToBase64String(uuid.AsGuid().ToByteArray());
            var b64Web = $"{b64.Trim('=').Replace('/', '-')}";
            return b64Web;
        }

        private static string _deviceB64UUID = null;
        public static string GetDeviceB64UUID(bool showInfoToHash = false)
        {
            if (_deviceB64UUID != null) return _deviceB64UUID;

            var (macHexOK, macHex) = WindowsMacUtils.GetAndCacheMACHex();
            var (systemGuidOK, systemGuid) = GetMachineGuidOrFallback();
            var (gotCachedHexUuid, cachedHexUuid) = GetCachedHexUUID();
            if (macHexOK && systemGuidOK)
            {
                var infoToHash = GetInfoToHash(macHex, systemGuid);
                if (showInfoToHash)
                {
                    Console.WriteLine("NHM/[{cpuSerial}]-[{macHex}]-[{guid}]-[{extraRigSeed}]");
                    Console.WriteLine(infoToHash);
                }
                Logger.Info("NHM.UUID", $"infoToHash='{infoToHash}'");
                var hexUuid = GetHexUUID(infoToHash);
                _deviceB64UUID = $"{0}-{GetB64UUID(hexUuid)}";
                return _deviceB64UUID;
            }
            else if (gotCachedHexUuid)
            {
                Logger.Error("NHM.UUID", $"Unable to read MAC or GUID read macHexOK='{macHexOK}' systemGuidOK='{systemGuidOK}'");
                Logger.Info("NHM.UUID", $"gotCachedHexUuid cachedHexUuid='{cachedHexUuid}'");
                _deviceB64UUID = $"{0}-{GetB64UUID(cachedHexUuid)}";
                return _deviceB64UUID;
            }
            else
            {
                Logger.Error("NHM.UUID", $"Unable to read MAC or GUID read macHexOK='{macHexOK}' systemGuidOK='{systemGuidOK}'");
                var hexUuidGenRandom = GetHexUUID(System.Guid.NewGuid().ToString());
                Logger.Info("NHM.UUID", $"Generating hexUuidGenRandom='{hexUuidGenRandom}' ok='{CacheHexUUIDToFile(hexUuidGenRandom)}'");
                _deviceB64UUID = $"{0}-{GetB64UUID(hexUuidGenRandom)}";
                return _deviceB64UUID;
            }
        }

        private static string GetInfoToHash(string macHex, string guid)
        {
            var cpuSerial = GetCpuID();
            var extraRigSeed = GetExtraRigSeed();
            var infoToHash = $"NHM/[{cpuSerial}]-[{macHex}]-[{guid}]-[{extraRigSeed}]";
            return infoToHash;
        }

        private static (bool ok, string systemGuid) GetMachineGuidOrFallback()
        {
            // main deterministic
            try
            {
                var readValue = Registry.GetValue("HKEY_LOCAL_MACHINE" + @"\SOFTWARE\Microsoft\Cryptography", "MachineGuid", new object());
                return (true, (string)readValue);
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"GetMachineGuid: {e.Message}");
            }

            // fallback deterministic
            try
            {
                const string valueFallback = "MachineGuidNhmGen";
                using (var rkFallback = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, true))
                {
                    var fallbackUUIDValue = rkFallback?.GetValue(valueFallback, null);
                    if (fallbackUUIDValue == null)
                    {
                        try
                        {
                            var genUUID = System.Guid.NewGuid().ToString();
                            rkFallback?.SetValue(valueFallback, genUUID);
                            return (true, genUUID);
                        }
                        catch (Exception e)
                        {
                            //if registry fails do fallback to files
                            Logger.Error("NHM.UUID", $"Fallback SetValue: {e.Message}");
                            return (false, "");
                        }
                    }
                    else if (fallbackUUIDValue is string regUUID)
                    {
                        return (true, regUUID);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"MachineGuidNhmGen: {e.Message}");
            }

            return (false, "");
        }

        private static string CachedRigUUIDPath = Paths.RootPath("rig_hex_uuid.txt");

        private static (bool, string) GetCachedHexUUID()
        {
            try
            {
                var lastSavedGuid = File.ReadAllText(CachedRigUUIDPath);
                if (System.Guid.TryParse(lastSavedGuid, out var _))
                {
                    Logger.Info("NHM.UUID", $"Returning valid GUID from file '{lastSavedGuid}'");
                    return (true, lastSavedGuid);
                }
                else
                {
                    Logger.Warn("NHM.UUID", $"Read invalid GUID from file '{lastSavedGuid}'");
                }
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"Error while reading saved GUID: {e.Message}");
            }
            return (false, "");
        }

        private static bool CacheHexUUIDToFile(string hexUUID)
        {
            bool saved = true;
            try
            {
                File.WriteAllText(CachedRigUUIDPath, hexUUID);
            }
            catch (Exception ex)
            {
                Logger.Error("NHM.UUID", $"Save GUID fallback failed: {ex.Message}");
                saved = false;
            }
            try
            {
                //log fallback to logs
                File.AppendAllText(Paths.RootPath("logs", "guidFallback.txt"), hexUUID);
            }
            catch (Exception ex)
            {
                Logger.Error("NHM.UUID", $"Logging failed: {ex.Message}");
            }
            return saved;
        }

        private static string GetCpuID()
        {
            var serial = "N/A";
            try
            {
                using (var searcher = new ManagementObjectSearcher("Select ProcessorID from Win32_processor"))
                using (var query = searcher.Get())
                {
                    foreach (var item in query)
                    {
                        serial = item.GetPropertyValue("ProcessorID").ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"Error GetCpuID(): {e.Message}");
            }
            return serial;
        }

        private static string GetExtraRigSeed()
        {
            try
            {
                string path = Paths.RootPath("extra_rig_seed.txt");
                if (File.Exists(path)) return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"Error GetExtraRigSeed(): {e.Message}");
            }
            return "";
        }
    }
}
