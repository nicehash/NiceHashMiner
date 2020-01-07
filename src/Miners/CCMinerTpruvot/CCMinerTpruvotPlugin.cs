using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CCMinerTpruvot
{
    public partial class CCMinerTpruvotPlugin : PluginBase
    {
        public CCMinerTpruvotPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://github.com/tpruvot/ccminer/releases current 2.3.1
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "2.3.1",
                ExePath = new List<string> { "ccminer-x64.exe" },
                Urls = new List<string>
                {
                    "https://github.com/tpruvot/ccminer/releases/download/2.3.1-tpruvot/ccminer-2.3.1-cuda10.7z", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "NVIDIA miner for Lyra2REv3 and X16R.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "2257f160-7236-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(5, 0);
        public override string Name => "CCMinerTpruvot";

        public override string Author => "info@nicehash.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var reqCudaVer = Checkers.CudaVersion.CUDA_10_0_130;
            var isCompatible = Checkers.IsCudaCompatibleDriver(reqCudaVer, CUDADevice.INSTALLED_NVIDIA_DRIVERS);
            if (!isCompatible) return supported; // return emtpy

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 3)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new CCMinerTpruvot(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "ccminer-x64.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                // X16 R is overestimated in version v1.0
                var isX16R = ids.Contains(AlgorithmType.X16R);
                var isOverestimatedVersion = benchmarkedPluginVersion.Major == 1 && benchmarkedPluginVersion.Minor == 0;
                return isX16R && isOverestimatedVersion;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
