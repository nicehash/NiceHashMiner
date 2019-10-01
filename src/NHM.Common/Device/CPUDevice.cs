using NHM.Common.Enums;
using System.Collections.Generic;

namespace NHM.Common.Device
{
    public class CPUDevice : BaseDevice
    {
        public CPUDevice(BaseDevice bd, int cpuCount, int threadsPerCPU, bool supportsHyperThreading, List<ulong> affinityMasks, List<CpuExtensionType> extensions) : base(bd)
        {
            PhysicalProcessorCount = cpuCount;
            ThreadsPerCPU = threadsPerCPU;
            SupportsHyperThreading = supportsHyperThreading;
            AffinityMasks = affinityMasks;
            SupportedCpuExtensions = extensions;
        }

        public IReadOnlyList<CpuExtensionType> SupportedCpuExtensions { get; }

        public int PhysicalProcessorCount { get; }
        public int ThreadsPerCPU { get; }
        public bool SupportsHyperThreading { get; }
        public List<ulong> AffinityMasks { get; protected set; } // TODO check if this makes any sense
    }
}
