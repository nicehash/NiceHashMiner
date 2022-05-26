using System;

namespace NHM.Common.Device
{
    public class CUDADevice : BaseDevice, IGpuDevice, IEquatable<CUDADevice>
    {
        public static string RawDetectionOutput = string.Empty;
        public CUDADevice(BaseDevice bd, int iPCIeBusID, ulong gpuRam, int sM_major, int sM_minor, bool isLhr) : base(bd)
        {
            PCIeBusID = iPCIeBusID;
            GpuRam = gpuRam;
            SM_major = sM_major;
            SM_minor = sM_minor;
            IsLHR = isLhr;
        }

        // TODO does it make sense to set here the actual installed NVIDIA drivers??
        public static Version INSTALLED_NVIDIA_DRIVERS = new Version(0, 0); // or use just null

        public int PCIeBusID { get; }
        public ulong GpuRam { get; }

        // we assume disabled and we check it after OpenCL detection.
        public bool IsOpenCLBackendEnabled { get; private set; } = false;

        // these should be more than enough for filtering 
        public int SM_major { get; }
        public int SM_minor { get; }
        public bool IsLHR { get; }
        public string RawDeviceData { get; set; }

        public void SetIsOpenCLBackendEnabled(bool enabled)
        {
            IsOpenCLBackendEnabled = enabled;
        }

        public bool Equals(CUDADevice other)
        {
            if (!base.Equals(other)) return false;
            if (PCIeBusID != other.PCIeBusID) return false;
            if (GpuRam != other.GpuRam) return false;
            if (IsOpenCLBackendEnabled != other.IsOpenCLBackendEnabled) return false;
            if (SM_major != other.SM_major) return false;
            if (SM_minor != other.SM_minor) return false;
            if (IsLHR != other.IsLHR) return false;
            if (RawDeviceData != other.RawDeviceData) return false;
            return true;
        }
    }
}
