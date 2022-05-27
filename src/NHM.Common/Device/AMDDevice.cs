using System;

namespace NHM.Common.Device
{
    public class AMDDevice : BaseDevice, IGpuDevice
    {
        public Version DEVICE_AMD_DRIVER { get; set; } = new Version(0, 0);
        public string RawDriverVersion { get; set; }
        public int ADLReturnCode { get; set; }
        public int ADLFunctionCall { get; set; }

        public int OpenCLPlatformID { get; init; }

        public int PCIeBusID { get; init; }
        public ulong GpuRam { get; init; }

        public string Codename { get; init; }
        public string InfSection { get; init; }

        // AMD always true
        public bool IsOpenCLBackendEnabled => true;
    }
}
