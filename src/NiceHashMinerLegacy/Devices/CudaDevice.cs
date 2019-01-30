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
                VendorName = string.Format(International.GetText("ComputeDevice_UNKNOWN_VENDOR_REPLACE"), VendorID);
            }
            return $"{VendorName} {DeviceName}";
        }

        public bool IsEtherumCapable()
        {
            // exception devices
            if (DeviceName.Contains("750") && DeviceName.Contains("Ti"))
            {
                Helpers.ConsolePrint("CudaDevice",
                    "GTX 750Ti found! By default this device will be disabled for ethereum as it is generally too slow to mine on it.");
                return false;
            }
            return DeviceGlobalMemory >= ComputeDevice.Memory3Gb && SM_major >= 3;
        }
    }
}
