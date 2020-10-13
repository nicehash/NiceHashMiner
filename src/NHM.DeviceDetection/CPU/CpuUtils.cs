using NHM.Common.Device;
using NHM.Common.Enums;
using System.Collections.Generic;

namespace NHM.DeviceDetection.CPU
{
    internal static class CpuUtils
    {
        // this is the order we check and initialize if automatic
        private static CpuExtensionType[] _detectOrder =
        {
            CpuExtensionType.AVX2_AES,
            CpuExtensionType.AVX2,
            CpuExtensionType.AVX_AES,
            CpuExtensionType.AVX,
            CpuExtensionType.AES,
            CpuExtensionType.SSE2, // disabled
        };

        /// <summary>
        /// HasExtensionSupport checks CPU extensions support, if type automatic just return false.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>False if type Automatic otherwise True if supported</returns>
        private static bool HasExtensionSupport(CpuExtensionType type, CpuID cpuID)
        {
            switch (type)
            {
                case CpuExtensionType.AVX2_AES: return cpuID.SupportsAVX2 && cpuID.SupportsAES_SSE42;
                case CpuExtensionType.AVX2: return cpuID.SupportsAVX2;
                case CpuExtensionType.AVX_AES: return cpuID.SupportsAVX && cpuID.SupportsAES_SSE42;
                case CpuExtensionType.AVX: return cpuID.SupportsAVX;
                case CpuExtensionType.AES: return cpuID.SupportsAES_SSE42;
                case CpuExtensionType.SSE2: return cpuID.SupportsSSE2;
                default: // CPUExtensionType.Automatic
                    break;
            }
            return false;
        }

        public static List<CpuExtensionType> SupportedExtensions(CpuID cpuID)
        {
            var ret = new List<CpuExtensionType>();
            foreach (var ext in _detectOrder)
            {
                if (HasExtensionSupport(ext, cpuID))
                {
                    ret.Add(ext);
                }
            }
            return ret;
        }

        /// <summary>
        /// Checks if CPU mining is capable, CPU must have AES support
        /// </summary>
        /// <returns></returns>
        public static bool IsCpuMiningCapable(CpuID cpuID)
        {
            return HasExtensionSupport(CpuExtensionType.AES, cpuID);
        }

        public static ulong CreateAffinityMask(int index, int percpu)
        {
            ulong mask = 0;
            const ulong one = 0x0000000000000001;
            for (var i = index * percpu; i < (index + 1) * percpu; i++)
                mask = mask | (one << i);
            return mask;
        }
    }
}
