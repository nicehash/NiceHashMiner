using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private static bool HasExtensionSupport(CpuExtensionType type)
        {
            switch (type)
            {
                case CpuExtensionType.AVX2_AES: return (CpuID.SupportsAVX2() == 1) && (CpuID.SupportsAES() == 1);
                case CpuExtensionType.AVX2: return CpuID.SupportsAVX2() == 1;
                case CpuExtensionType.AVX_AES: return (CpuID.SupportsAVX() == 1) && (CpuID.SupportsAES() == 1);
                case CpuExtensionType.AVX: return CpuID.SupportsAVX() == 1;
                case CpuExtensionType.AES: return CpuID.SupportsAES() == 1;
                case CpuExtensionType.SSE2: return CpuID.SupportsSSE2() == 1;
                default: // CPUExtensionType.Automatic
                    break;
            }
            return false;
        }

        /// <summary>
        /// Checks if CPU mining is capable, CPU must have AES support
        /// </summary>
        /// <returns></returns>
        public static bool IsCpuMiningCapable()
        {
            return HasExtensionSupport(CpuExtensionType.AES);
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
