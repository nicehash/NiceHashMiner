using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceDetection.CPU
{
    internal class CPUDetectionResult
    {
        public int NumberOfCPUCores { get; internal set; }
        public int VirtualCoresCount { get; internal set; }
        public bool IsHyperThreadingEnabled => VirtualCoresCount > NumberOfCPUCores;
        public List<CpuInfo> CpuInfos { get; internal set; }
    }
}
