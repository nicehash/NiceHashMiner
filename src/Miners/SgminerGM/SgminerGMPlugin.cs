using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SgminerGM
{
    public partial class SgminerGMPlugin : PluginBase
    {
        public SgminerGMPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://github.com/nicehash/sgminer-gm/releases current v5.5.5-8
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v5.5.5-8",
                ExePath = new List<string> { "sgminer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/sgminer-gm/releases/download/5.5.5-8/sgminer-5.5.5-gm-nicehash-8-windows-amd64.zip",
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "This is a multi-threaded multi-pool GPU miner with ATI GPU monitoring.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "d5f18960-e361-11e9-a914-497feefbdfc8";

        public override Version Version => new Version(5, 0);
        public override string Name => "SGminerGM";

        public override string Author => "info@nicehash.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            const ulong MinDaggerHashimotoMemory = 3UL << 30; // 3GB
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var amdGpus = devices
                .Where(dev => dev is AMDDevice amdGpu && amdGpu.GpuRam > MinDaggerHashimotoMemory)
                .Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new SgminerGM(PluginUUID);
        }

        public override bool CanGroup(MiningPair a, MiningPair b)
        {
            var canGroup = base.CanGroup(a, b);
            if (canGroup && a.Device is AMDDevice aDev && b.Device is AMDDevice bDev && aDev.OpenCLPlatformID != bDev.OpenCLPlatformID)
            {
                // OpenCLPlatorm IDs must match
                return false;
            }
            return canGroup;
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "sgminer.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }
    }
}
