using NHM.Common.Device;
using NHM.Common.Enums;
using System.Collections.Generic;
using System.Linq;

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
        private static bool HasExtensionSupport(CpuExtensionType type, CpuID cpuID) =>
            type switch
            {
                CpuExtensionType.AVX2_AES => cpuID.SupportsAVX2 && cpuID.SupportsAES_SSE42,
                CpuExtensionType.AVX2 => cpuID.SupportsAVX2,
                CpuExtensionType.AVX_AES => cpuID.SupportsAVX && cpuID.SupportsAES_SSE42,
                CpuExtensionType.AVX => cpuID.SupportsAVX,
                CpuExtensionType.AES => cpuID.SupportsAES_SSE42,
                CpuExtensionType.SSE2 => cpuID.SupportsSSE2,
                _ => false,
            };

        public static List<CpuExtensionType> SupportedExtensions(CpuID cpuID)
        {
            return _detectOrder
                .Where(ext => HasExtensionSupport(ext, cpuID))
                .ToList();
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
