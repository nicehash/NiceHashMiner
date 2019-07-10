using System;
using System.Collections.Generic;
using System.Text;

namespace NHM.Common.Device
{
    public interface IGpuDevice
    {
        string UUID { get; }
        int PCIeBusID { get; }
        ulong GpuRam { get; }
    }
}
