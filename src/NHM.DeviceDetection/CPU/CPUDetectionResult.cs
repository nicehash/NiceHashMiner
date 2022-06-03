using System;
using System.Collections.Generic;

namespace NHM.DeviceDetection.CPU
{
    internal class CPUDetectionResult : IEquatable<CPUDetectionResult>
    {
        public int NumberOfCPUCores { get; internal set; }
        public int VirtualCoresCount { get; internal set; }
        public bool IsHyperThreadingEnabled => VirtualCoresCount > NumberOfCPUCores;
        public List<CpuInfo> CpuInfos { get; internal set; } = new List<CpuInfo>();
        public bool Equals(CPUDetectionResult other)
        {
            if(NumberOfCPUCores != other.NumberOfCPUCores) return false;
            if(VirtualCoresCount != other.VirtualCoresCount) return false;
            if(IsHyperThreadingEnabled != other.IsHyperThreadingEnabled) return false;
            if (CpuInfos.Count != other.CpuInfos.Count) return false;
            for(int i = 0; i < CpuInfos.Count; i++)
            {
                if(CpuInfos[i] != other.CpuInfos[i]) return false;
            }
            return true;
        }
    }
}
