using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ethminer
{
#warning "MARK abstract. This is due to API stats blocking so we don't want this instantiated"
    public abstract partial class EthminerPlugin : PluginBase
    {
        // mandatory constructor
        public EthminerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://github.com/ethereum-mining/ethminer/releases
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v0.18.0",
                ExePath = new List<string> { "bin", "ethminer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/ethereum-mining/ethminer/releases/download/v0.18.0/ethminer-0.18.0-cuda10.0-windows-amd64.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Ethminer is an Ethash GPU mining worker",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        //public override string PluginUUID => "TODO";

        public override Version Version => new Version(5, 0);
        public override string Name => "Ethminer";

        public override string Author => "info@nicehash.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>();

            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var reqCudaVer = Checkers.CudaVersion.CUDA_10_0_130;
            var isDriverSupported = Checkers.IsCudaCompatibleDriver(reqCudaVer, CUDADevice.INSTALLED_NVIDIA_DRIVERS);
            var supportedGpus = gpus.Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported));

            foreach (var gpu in supportedGpus)
            {
                var algorithms = GetSupportedAlgorithmsForDevice(gpu as BaseDevice);
                if (algorithms.Count > 0) supported.Add(gpu as BaseDevice, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(IGpuDevice dev)
        {
            var isSupported = dev is AMDDevice;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(IGpuDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 3;
            return isSupported && isDriverSupported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new Ethminer(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "ethminer.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }
    }
}
