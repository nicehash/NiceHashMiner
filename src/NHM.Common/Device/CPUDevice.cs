using NHM.Common.Enums;
using System.Collections.Generic;

namespace NHM.Common.Device
{
    public class CpuID
    {
        public bool AMD { get; init; }
        public bool Intel { get; init; }
        public bool IsZen { get; init; }
        public int L3KB_size { get; init; }
        public string Name { get; init; }
        public int PhysicalProcessorCount { get; init; }
        public bool SupportsAES_SSE42 { get; init; }
        public bool SupportsAVX { get; init; }
        public bool SupportsAVX2 { get; init; }
        public bool SupportsSSE2 { get; init; }
        public string Vendor { get; init; }
    }

    public class CPUDevice : BaseDevice
    {
        public IReadOnlyList<CpuExtensionType> SupportedCpuExtensions { get; init; }

        public int PhysicalProcessorCount { get; init; }
        public int ThreadsPerCPU { get; init; }
        public bool SupportsHyperThreading { get; init; }
        public List<ulong> AffinityMasks { get; init; } // TODO check if this makes any sense

        public CpuID CpuID { get; init; }
    }
}
