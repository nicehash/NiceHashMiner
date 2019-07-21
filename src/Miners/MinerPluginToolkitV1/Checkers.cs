using NHM.Common.Device;
using System;
using System.Collections.Generic;

namespace MinerPluginToolkitV1
{
    public static class Checkers
    {
        /// <summary>
        /// Get whether AMD device is GCN 4th gen or higher (400/500/Vega)
        /// </summary>
        public static bool IsGcn4(AMDDevice dev)
        {
            if (dev.Name.Contains("Vega")  || dev.InfSection.ToLower().Contains("r7500") || dev.InfSection.ToLower().Contains("vega"))
                return true;
            if (dev.InfSection.ToLower().Contains("polaris"))
                return true;

            return false;
        }

        /// <summary>
        /// Get whether AMD device is GCN 2th gen or higher (300/400/500/Vega)
        /// </summary>
        public static bool IsGcn2(AMDDevice dev)
        {
            if (dev.Name.Contains("Vega") || dev.InfSection.ToLower().Contains("r7500") || dev.InfSection.ToLower().Contains("vega"))
                return true;
            if (!dev.InfSection.ToLower().Contains("pitcairn ") && !dev.InfSection.ToLower().Contains("tahiti") && !dev.InfSection.ToLower().Contains("oland") && !dev.InfSection.ToLower().Contains("cape verde"))
                return true;

            return false;
        }

        // https://docs.nvidia.com/cuda/cuda-toolkit-release-notes/index.html
        public enum CudaVersion
        {
            // >= 347.62
            CUDA_7_0_28,

            // >= 353.66
            CUDA_7_5_16,

            // >= 369.30
            CUDA_8_0_44,

            // >= 376.51
            CUDA_8_0_61,

            // >= 385.54
            CUDA_9_0_76,

            // >= 391.29
            CUDA_9_1_85,

            // >= 397.44
            CUDA_9_2_88,

            // >= 398.26
            CUDA_9_2_148,

            // >= 411.31
            CUDA_10_0_130,

            // >= 418.96
            CUDA_10_1_105,
        }

        private static readonly IReadOnlyDictionary<CudaVersion, Version> _cudaVersions = new Dictionary<CudaVersion, Version>
        {
            { CudaVersion.CUDA_7_0_28, new Version(347,62) },
            { CudaVersion.CUDA_7_5_16, new Version(353,66) },
            { CudaVersion.CUDA_8_0_44, new Version(369,30) },
            { CudaVersion.CUDA_8_0_61, new Version(376,51) },
            { CudaVersion.CUDA_9_0_76, new Version(385,54) },
            { CudaVersion.CUDA_9_1_85, new Version(391,29) },
            { CudaVersion.CUDA_9_2_88, new Version(397,44) },
            { CudaVersion.CUDA_9_2_148, new Version(398,26) },
            { CudaVersion.CUDA_10_0_130, new Version(411,31) },
            { CudaVersion.CUDA_10_1_105, new Version(418,96) },
        };

        public static bool IsCudaCompatibleDriver(CudaVersion cudaVersion, Version version)
        {
            if (_cudaVersions.ContainsKey(cudaVersion))
            {
                var compareVersion = _cudaVersions[cudaVersion];
                return version >= compareVersion;
            }

            return false;
        }
    }
}
