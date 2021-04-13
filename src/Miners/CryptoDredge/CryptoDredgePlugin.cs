using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoDredge
{
    public partial class CryptoDredgePlugin : PluginBase
    {
        public CryptoDredgePlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://github.com/technobyl/CryptoDredge/releases | https://cryptodredge.org/ | https://bitcointalk.org/index.php?topic=4807821.0
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                // BinVersion github and bitcointalk missmatch
                BinVersion = "0.26.0",
                ExePath = new List<string> { "CryptoDredge.exe" },
                Urls = new List<string>
                {
                    "https://github.com/technobyl/CryptoDredge/releases/download/v0.26.0/CryptoDredge_0.26.0_cuda_11.2_windows.zip", // original source
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Simple in use and highly optimized cryptocurrency mining software with stable power consumption.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(16, 0);
        public override string Name => "CryptoDredge";

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => "e294f620-94eb-11ea-a64d-17be303ea466";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var isDriverCompatible = Checkers.IsCudaCompatibleDriver(Checkers.CudaVersion.CUDA_10_1_105, CUDADevice.INSTALLED_NVIDIA_DRIVERS);
            if (!isDriverCompatible) return supported;

            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithmsForDevice(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new CryptoDredge(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "CryptoDredge.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }

        // Since the API doesn't work for this one set the default value to false
        public override bool IsGetApiMaxTimeoutEnabled => MinerApiMaxTimeoutSetting.ParseIsEnabled(false, GetApiMaxTimeoutConfig);
    }
}
