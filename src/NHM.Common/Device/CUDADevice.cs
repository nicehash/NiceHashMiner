using System;

namespace NHM.Common.Device
{
    public class CUDADevice : BaseDevice, IGpuDevice
    {
        public CUDADevice(BaseDevice bd, int iPCIeBusID, ulong gpuRam, int sM_major, int sM_minor) : base(bd)
        {
            PCIeBusID = iPCIeBusID;
            GpuRam = gpuRam;
            SM_major = sM_major;
            SM_minor = sM_minor;
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

        public void SetIsOpenCLBackendEnabled(bool enabled)
        {
            IsOpenCLBackendEnabled = enabled;
        }
    }
}
