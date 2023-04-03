using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.IntelGPU
{
    [Serializable]
    internal class IntelGpuDeviceDetectionResult
    {
        internal record Device
        {
            public int VendorId { get; set; }
            public int pciSubSystemId { get; set; }
            public int pciDeviceId { get; set; }
            public int pciBusID { get; set; }
            public string DeviceName { get; set; }
            public string DriverVersion { get; set; }
            public ulong DeviceMemory { get; set; }
        }

        public List<Device> IgclDevices { get; set; }
        public string ErrorString { get; set; }
        public int IgclLoaded { get; set; }
        public int IgclInitialized { get; set; }
    }
}
