using System;

namespace NHM.Common.Device
{
    public class CUDADevice : BaseDevice, IGpuDevice
    {
        // TODO does it make sense to set here the actual installed NVIDIA drivers??
        public static Version INSTALLED_NVIDIA_DRIVERS = new Version(0, 0); // or use just null

        public int PCIeBusID { get; init; }
        public ulong GpuRam { get; init; }

        // we assume disabled and we check it after OpenCL detection.
        public bool IsOpenCLBackendEnabled { get; private set; } = false;

        // these should be more than enough for filtering 
        public int SM_major { get; init; }
        public int SM_minor { get; init; }
        public bool IsLHR { get; init; }

        public void SetIsOpenCLBackendEnabled(bool enabled)
        {
            IsOpenCLBackendEnabled = enabled;
        }
    }
}
