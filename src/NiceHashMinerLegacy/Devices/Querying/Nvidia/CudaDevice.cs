using System;

namespace NiceHashMiner.Devices
{
    [Serializable]
    public class CudaDevice
    {
        public uint DeviceID;
        public int pciBusID;
        public int VendorID;
        public string VendorName;
        public string DeviceName;
        public int HasMonitorConnected;
        public int SM_major;
        public int SM_minor;
        public string UUID;
        public ulong DeviceGlobalMemory;
        public uint pciDeviceId; //!< The combined 16-bit device id and 16-bit vendor id
        public uint pciSubSystemId; //!< The 32-bit Sub System Device ID
        public int SMX;

        // more accuare description
        public string GetName()
        {
            if (VendorName == "UNKNOWN")
            {
                VendorName = string.Format(Translations.Tr("V_ID_{0}"), VendorID);
            }
            return $"{VendorName} {DeviceName}";
        }
    }
}
