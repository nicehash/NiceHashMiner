using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.NVIDIA
{
    [Serializable]
    internal class CudaDeviceDetectionResult
    {
        internal record Device
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

        public List<Device> CudaDevices { get; set; }
        public string DriverVersion { get; set; }
        public string ErrorString { get; set; }
        public int NvmlLoaded { get; set; }
        public int NvmlInitialized { get; set; }
    }
}
