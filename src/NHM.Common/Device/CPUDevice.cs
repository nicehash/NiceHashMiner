using NHM.Common.Enums;
using System;
using System.Collections.Generic;

namespace NHM.Common.Device
{
    public record CpuID
    {
        public bool AMD { get; set; }
        public bool Intel { get; set; }
        public bool IsZen { get; set; }
        public int L3KB_size { get; set; }
        public string Name { get; set; }
        public int PhysicalProcessorCount { get; set; }
        public bool SupportsAES_SSE42 { get; set; }
        public bool SupportsAVX { get; set; }
        public bool SupportsAVX2 { get; set; }
        public bool SupportsSSE2 { get; set; }
        public string Vendor { get; set; }
    }

    public class CPUDevice : BaseDevice, IEquatable<CPUDevice>
    {
        public static string RawDetectionOutput = string.Empty;
        public CPUDevice(BaseDevice bd, int cpuCount, int threadsPerCPU, bool supportsHyperThreading, List<ulong> affinityMasks, List<CpuExtensionType> extensions, CpuID cpuID) : base(bd)
        {
            PhysicalProcessorCount = cpuCount;
            ThreadsPerCPU = threadsPerCPU;
            SupportsHyperThreading = supportsHyperThreading;
            AffinityMasks = affinityMasks;
            SupportedCpuExtensions = extensions;
            CpuID = cpuID;
        }

        public IReadOnlyList<CpuExtensionType> SupportedCpuExtensions { get; }

        public int PhysicalProcessorCount { get; }
        public int ThreadsPerCPU { get; }
        public bool SupportsHyperThreading { get; }
        public List<ulong> AffinityMasks { get; protected set; } // TODO check if this makes any sense

        public CpuID CpuID { get; protected set; }

        public bool Equals(CPUDevice other)
        {
            if (!base.Equals(other)) return false;
            if(SupportedCpuExtensions != null && other.SupportedCpuExtensions != null)
            {
                if (SupportedCpuExtensions.Count != other.SupportedCpuExtensions.Count) return false;
                for (int i = 0; i < SupportedCpuExtensions.Count; i++)
                {
                    if (SupportedCpuExtensions[i] != other.SupportedCpuExtensions[i]) return false;
                }
            }
            if (SupportedCpuExtensions == null && other.SupportedCpuExtensions != null ||
                SupportedCpuExtensions != null && other.SupportedCpuExtensions == null) return false;
            if (PhysicalProcessorCount != other.PhysicalProcessorCount) return false;
            if (ThreadsPerCPU != other.ThreadsPerCPU) return false;
            if (SupportsHyperThreading != other.SupportsHyperThreading) return false;
            if(AffinityMasks != null && other.AffinityMasks != null)
            {
                if (AffinityMasks.Count != other.AffinityMasks.Count) return false;
                for (int i = 0; i < AffinityMasks.Count; i++)
                {
                    if (AffinityMasks[i] != other.AffinityMasks[i]) return false;
                }
            }
            if (AffinityMasks == null && other.AffinityMasks != null ||
                AffinityMasks != null && other.AffinityMasks == null) return false;
            if (CpuID != other.CpuID) return false;
            return true;
        }
    }
}
