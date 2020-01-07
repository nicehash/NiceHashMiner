using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SgminerAvemore
{
    public partial class SgminerAvemorePlugin : PluginBase
    {
        public SgminerAvemorePlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://github.com/brian112358/avermore-miner/releases current v1.4.1
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.4.1",
                ExePath = new List<string> { "avermore-windows", "sgminer.exe" }, 
                Urls = new List<string>
                {
                    "https://github.com/brian112358/avermore-miner/releases/download/v1.4.1/avermore-v1.4.1-windows.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "This is a multi-threaded multi-pool GPU miner.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "bc95fd70-e361-11e9-a914-497feefbdfc8";

        public override Version Version => new Version(5, 0);
        public override string Name => "SGminerAvemore";

        public override string Author => "info@nicehash.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var amdGpus = devices
                .Where(dev => dev is AMDDevice)
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
            return new SgminerAvemore(PluginUUID);
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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "libcurl.dll", "libeay32.dll", "libidn-11.dll", "librtmp.dll",
                "libssh2.dll", "pdcurses.dll", "pthreadGC2.dll", "ssleay32.dll", "zlib1.dll", "sgminer.exe"
            });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }
    }
}
