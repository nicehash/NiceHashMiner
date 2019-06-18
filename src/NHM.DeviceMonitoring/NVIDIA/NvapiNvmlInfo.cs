using ManagedCuda.Nvml;
using NVIDIA.NVAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.NVIDIA
{
    internal class NvapiNvmlInfo
    {
        public string UUID { get; internal set; }
        public int BusID { get; internal set; }
        public NvPhysicalGpuHandle nvHandle { get; internal set; }
        public nvmlDevice nvmlHandle { get; internal set; }
    }
}
