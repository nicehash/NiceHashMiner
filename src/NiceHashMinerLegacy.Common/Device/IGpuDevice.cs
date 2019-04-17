using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMinerLegacy.Common.Device
{
    public interface IGpuDevice
    {
        string UUID { get; }
        int PCIeBusID { get; }
        ulong GpuRam { get; }
    }
}
