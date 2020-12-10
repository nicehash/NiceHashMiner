using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using NHM.MinerPluginToolkitV1.Interfaces;
using System.Threading.Tasks;

namespace Excavator
{
    public partial class ExcavatorPlugin : PluginBase
    {
        public ExcavatorPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // TODO link
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.6.0a", // fix version if wrong
                ExePath = new List<string> { "excavator.exe" },
                Urls = new List<string>
                {
                    // TODO
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "NiceHash excavator NVIDIA GPU miner.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(15, 0);

        public override string Name => "Excavator";

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => "60ebcd15-e99f-4d6a-858e-78609d5cebf7";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // SM 6.0+
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 6).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var minDrivers = new Version(411, 0); // TODO
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithmsForDevice(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new Excavator(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "excavator.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }
    }
}
