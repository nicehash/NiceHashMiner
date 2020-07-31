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

            var infoToHash = GetInfoToHash(GetMachineGuidOrFallback());
            if (showInfoToHash)
            {
                Console.WriteLine("NHM/[{cpuSerial}]-[{macUUID}]-[{guid}]-[{extraRigSeed}]");
                Console.WriteLine(infoToHash);
            }
            Logger.Info("NHM.UUID", $"infoToHash='{infoToHash}'");
            var hexUuid = GetHexUUID(infoToHash);
            _deviceB64UUID = $"{0}-{GetB64UUID(hexUuid)}";
            return _deviceB64UUID;
        }

        private static string GetInfoToHash(string guid)
        {
            var cpuSerial = GetCpuID();
            var macUUID = WindowsMacUtils.GetMAC_UUID();
            var extraRigSeed = GetExtraRigSeed();
            var infoToHash = $"NHM/[{cpuSerial}]-[{macUUID}]-[{guid}]-[{extraRigSeed}]";
            return infoToHash;
        }
        public static string GetMachineGuidOrFallback()
        {
            const string hklm = "HKEY_LOCAL_MACHINE";
            const string keyPath = hklm + @"\SOFTWARE\Microsoft\Cryptography";
            const string value = "MachineGuid";

            // main deterministic
            try
            {
                var readValue = Registry.GetValue(keyPath, value, new object());
                return (string)readValue;
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"GetMachineGuid: {e.Message}");
            }
            // fallback deterministic
            try
            {
                //const string userKeyPath = @"Software\" + "";
                const string valueFallback = "MachineGuidNhmGen";
                using (var rkFallback = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\" + APP_GUID.GUID, true))
                {
                    var fallbackUUIDValue = rkFallback.GetValue(valueFallback, null);
                    string genUUID = "";
                    if (fallbackUUIDValue == null)
                    {
                        genUUID = System.Guid.NewGuid().ToString();
                        try
                        {
                            rkFallback?.SetValue(valueFallback, genUUID);
                        }
                        catch (Exception e)
                        {
                            //if registry fails do fallback to files
                            Logger.Error("NHM.UUID", $"Fallback SetValue: {e.Message}");
                            return GetUUIDFromFile(genUUID);
                        }
                    }
                    else if (fallbackUUIDValue is string regUUID)
                    {
                        genUUID = regUUID;
                    }
                    return genUUID;
                }
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"Fallback: {e.Message}");
            }

            // fallback
            Logger.Warn("NHM.UUID", $"GetMachineGuid FALLBACK");
            return GetUUIDFromFile(System.Guid.NewGuid().ToString());
        }

        private static string GetUUIDFromFile(string generatedUUID)
        {
            var guidFallbackPath = Paths.RootPath("guidFallback.txt");
            if (File.Exists(guidFallbackPath))
            {
                var fileReadUUID = File.ReadAllText(guidFallbackPath);
                if (System.Guid.TryParse(fileReadUUID, out var fileUUID))
                {
                    return fileUUID.ToString();
                }
            }

            File.WriteAllText(guidFallbackPath, generatedUUID);

            //log fallback to logs
            var logGuidFallbackPath = Paths.RootPath(Path.Combine("logs", "guidFallback.txt"));
            File.AppendAllText(logGuidFallbackPath, GetInfoToHash(generatedUUID));
            return generatedUUID;
        }

        public static string GetCpuID()
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
            catch { }
            return serial;
        }

        private static string GetExtraRigSeed()
        {
            string path = Paths.RootPath("extra_rig_seed.txt");
            if (File.Exists(path))
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (Exception)
                {
                }
            }
            return "";
        }
    }
}
