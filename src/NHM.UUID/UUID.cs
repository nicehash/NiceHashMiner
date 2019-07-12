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

        public static string GetDeviceB64UUID(bool showInfoToHash = false)
        {
            var cpuSerial = GetCpuID();
            var macUUID = WindowsMacUtils.GetMAC_UUID();
            var guid = GetMachineGuidOrFallback();
            var extraRigSeed = GetExtraRigSeed();
            var infoToHash = $"NHM/[{cpuSerial}]-[{macUUID}]-[{guid}]-[{extraRigSeed}]";
            if (showInfoToHash)
            {
                Console.WriteLine("NHM/[{cpuSerial}]-[{macUUID}]-[{guid}]-[{extraRigSeed}]");
                Console.WriteLine(infoToHash);
            }
            Logger.Info("NHM.UUID", $"infoToHash='{infoToHash}'");
            var hexUuid = GetHexUUID(infoToHash);
            return $"{0}-{GetB64UUID(hexUuid)}";
        }

        public static string GetMachineGuidOrFallback()
        {
            const string hklm = "HKEY_LOCAL_MACHINE";
            const string keyPath = hklm + @"\SOFTWARE\Microsoft\Cryptography";
            const string value = "MachineGuid";

            try
            {
                var readValue = Registry.GetValue(keyPath, value, new object());
                return (string)readValue;
            }
            catch (Exception e)
            {
                Logger.Error("NHM.UUID", $"GetMachineGuid: {e.Message}");
            }
            // fallback
            Logger.Warn("NHM.UUID", $"GetMachineGuid FALLBACK");
            return System.Guid.NewGuid().ToString();
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
