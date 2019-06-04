using Microsoft.Win32;
using System;

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

        public static string GetDeviceB64UUID()
        {
            var guid = GetMachineGuid();
            if (guid == null)
            {
                // fallback
                return $"{0}-{System.Guid.NewGuid()}";
            }

            var hexUuid = GetHexUUID($"NHM/{guid}");
            return $"{0}-{GetB64UUID(hexUuid)}";
        }

        public static string GetMachineGuid()
        {
            const string hklm = "HKEY_LOCAL_MACHINE";
            const string keyPath = hklm + @"\SOFTWARE\Microsoft\Cryptography";
            const string value = "MachineGuid";

            try
            {
                return (string)Registry.GetValue(keyPath, value, new object());
            }
            catch (Exception e)
            {
                //Logger.Error("REGISTRY", e.Message);
            }

            return null;
        }
    }
}
