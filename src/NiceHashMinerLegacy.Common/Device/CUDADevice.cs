using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMinerLegacy.Common.Device
{
    public class CUDADevice : BaseDevice
    {
        public CUDADevice(BaseDevice bd, int iPCIeBusID, ulong gpuRam, int sM_major, int sM_minor) : base(bd)
        {
            PCIeBusID = iPCIeBusID;
            GpuRam = gpuRam;
            SM_major = sM_major;
            SM_minor = sM_minor;
        }

        // TODO does it make sense to set here the actual installed NVIDIA drivers??
        public static string INSTALLED_NVIDIA_DRIVERS = "";

        public int PCIeBusID { get; }
        public ulong GpuRam { get; }

        // these should be more than enough for filtering 
        public int SM_major { get; }
        public int SM_minor { get; }
    }
}
