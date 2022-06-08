using System;

namespace NHM.Common.Device
{
    public class AMDDevice : BaseDevice, IGpuDevice
    {
        public static string RawDetectionOutput = string.Empty;
        public AMDDevice(BaseDevice bd, int iPCIeBusID, ulong gpuRam, string codename, string infSection, int openCLPlatformID = -1) : base(bd)
        {
            PCIeBusID = iPCIeBusID;
            GpuRam = gpuRam;
            Codename = codename;
            InfSection = infSection;
            OpenCLPlatformID = openCLPlatformID;
        }
        public Version DEVICE_AMD_DRIVER = new Version(0, 0);
        public string RawDriverVersion { get; set; }
        public int ADLReturnCode { get; set; }
        public int ADLFunctionCall { get; set; }
        public int OpenCLPlatformID { get; }

        public int PCIeBusID { get; }
        public ulong GpuRam { get; }

        public string Codename { get; }
        public string InfSection { get; }
        public string RawDeviceData { get; set; }

        // AMD always true
        public bool IsOpenCLBackendEnabled => true;
    }
}
