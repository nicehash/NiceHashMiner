using System;

namespace NHM.Common.Device
{
    public class AMDDevice : BaseDevice, IGpuDevice, IEquatable<AMDDevice>
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
        public bool Equals(AMDDevice other)
        {
            if (!base.Equals(other)) return false;
            if (DEVICE_AMD_DRIVER != other.DEVICE_AMD_DRIVER) return false;
            if (RawDriverVersion != other.RawDriverVersion) return false;
            if (ADLReturnCode != other.ADLReturnCode) return false;
            if (ADLFunctionCall != other.ADLFunctionCall) return false;
            if (OpenCLPlatformID != other.OpenCLPlatformID) return false;
            if (PCIeBusID != other.PCIeBusID) return false;
            if (GpuRam != other.GpuRam) return false;
            if (Codename != other.Codename) return false;
            if (InfSection != other.InfSection) return false;
            if (RawDeviceData != other.RawDeviceData) return false;
            return true;
        }
    }
}
