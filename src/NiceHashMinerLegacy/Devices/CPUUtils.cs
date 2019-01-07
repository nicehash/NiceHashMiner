using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Devices
{
    public static class CpuUtils
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

        ///// <summary>
        ///// Returns most performant CPU extension based on settings.
        ///// Returns automatic if NO extension is avaliable
        ///// </summary>
        ///// <returns></returns>
        //public static CPUExtensionType GetMostOptimized() {
        //    if (ConfigManager.GeneralConfig.ForceCPUExtension == CPUExtensionType.Automatic) {
        //        for (int i = 0; i < _detectOrder.Length; ++i) {
        //            if (HasExtensionSupport(_detectOrder[i])) {
        //                return _detectOrder[i];
        //            }
        //        }
        //    } else if (HasExtensionSupport(ConfigManager.GeneralConfig.ForceCPUExtension)) {
        //        return ConfigManager.GeneralConfig.ForceCPUExtension;
        //    }
        //    return CPUExtensionType.Automatic;
        //}

        /// <summary>
        /// Checks if CPU mining is capable, CPU must have AES support
        /// </summary>
        /// <returns></returns>
        public static bool IsCpuMiningCapable()
        {
            return HasExtensionSupport(CpuExtensionType.AES);
        }
    }
}
