using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1
{
    public static class Checkers
    {
        /// <summary>
        /// Get whether AMD device is GCN 4th gen or higher (400/500/Vega)
        /// </summary>
        public static bool IsGcn4(AMDDevice dev)
        {
            var oldCodeNames = new List<string> {
                /*Gen1*/ "oland", "cape verde", "pitcairn", "tahiti",
                /*Gen2*/"bonaire", "hawaii", "temash", "kabini", "liverpool", "durango", "kaveri", "godavari", "mullins", "beema", "carrizo-L",
                /*Gen3*/"tonga", "fiji", "carrizo", "bristol ridge", "stoney ridge"
            };

            if (oldCodeNames.Contains(dev.InfSection.ToLower()))
                return false;

            return true;
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

            // >= 451.82
            CUDA_11_0_3_Update1,
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
            { CudaVersion.CUDA_11_0_3_Update1, new Version(451,82) },
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

        private static int[] _supportedMajorVersions = new int[] { 16 };
        public static IEnumerable<int> SupportedMajorVersions => _supportedMajorVersions;
        public static bool IsMajorVersionSupported(int major)
        {
            return _supportedMajorVersions.Contains(major);
        }

        public static readonly List<string> ObsoleteMinerPlugins = new List<string> {
            "2257f160-7236-11e9-b20c-f9f12eb6d835",
            "70984aa0-7236-11e9-b20c-f9f12eb6d835",
            "92fceb00-7236-11e9-b20c-f9f12eb6d835",
            "d9c2e620-7236-11e9-b20c-f9f12eb6d835",
            "efd40691-618c-491a-b328-e7e020bda7a3",
            "1b7019d0-7237-11e9-b20c-f9f12eb6d835",
            "435f0820-7237-11e9-b20c-f9f12eb6d835",
            "59bba2c0-b1ef-11e9-8e4e-bb1e2c6e76b4",
            "a841b4b0-ae17-11e9-8e4e-bb1e2c6e76b4",
            "6c07f7a0-7237-11e9-b20c-f9f12eb6d835",
            "f5d4a470-e360-11e9-a914-497feefbdfc8",
            "85f507c0-b2ba-11e9-8e4e-bb1e2c6e76b4",
            "abc3e2a0-7237-11e9-b20c-f9f12eb6d835",
            "d47d9b00-7237-11e9-b20c-f9f12eb6d835",
            "f1945a30-7237-11e9-b20c-f9f12eb6d835",
            "2edd8080-9cb6-11e9-a6b8-09e27549d5bb",
            "1046ea50-c261-11e9-8e4e-bb1e2c6e76b4",
            "3d4e56b0-7238-11e9-b20c-f9f12eb6d835",
            "4aec5ec0-10f8-11ea-bad3-8dea21141bbb",
            "5532d300-7238-11e9-b20c-f9f12eb6d835",
            "CCMinerMTP",
            "CCMinerTpruvot",
            "ClaymoreDual",
            "GMiner",
            "NBMiner",
            "Phoenix",
            "SGminerAvemore",
            "SGminerGM",
            "TeamRedMiner",
            "TRex",
            "XmrStak",
            "VC_REDIST_x64_2015",
        };
    }
}
