using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMinerLegacy.Common.Device
{
    public interface IGpuDevice
    {
        int PCIeBusID { get; }
        ulong GpuRam { get; }
    }
}
