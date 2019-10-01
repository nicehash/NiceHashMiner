using System;

namespace NHM.DeviceDetection.NVIDIA.Models
{
    [Serializable]
    internal class CudaDevice
    {
        public uint DeviceID { get; set; } // CUDA Index
        public int pciBusID { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public string DeviceName { get; set; }
        public int HasMonitorConnected { get; set; }
        public int SM_major { get; set; }
        public int SM_minor { get; set; }
        public string UUID { get; set; }
        public ulong DeviceGlobalMemory { get; set; }
        public uint pciDeviceId { get; set; } //!< The combined 16-bit device id and 16-bit vendor id
        public uint pciSubSystemId { get; set; } //!< The 32-bit Sub System Device ID
        public int SMX { get; set; }
    }
}
