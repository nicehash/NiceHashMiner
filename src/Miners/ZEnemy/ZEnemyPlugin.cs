using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZEnemy
{
    public partial class ZEnemyPlugin : PluginBase
    {
        public ZEnemyPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://bitcointalk.org/index.php?topic=3378390.0
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                // YEY GITHUB TAG! => 'kawpow262'
                BinVersion = "2.6.2-win-cuda10.1", // fix version if wrong
                ExePath = new List<string> { "z-enemy.exe" },
                Urls = new List<string>
                {
                    "https://github.com/zealot-rvn/z-enemy/releases/download/kawpow262/z-enemy-2.6.2-win-cuda10.1.zip",
                    "https://mega.nz/#!UXRBCChJ!v7JqOCuvq4hl1XR76BGiC75Gq97vKSliuH2uKZvU1iQ" // original source
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Zealot/Enemy (z-enemy) NVIDIA GPU miner.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(16, 0);

        public override string Name => "ZEnemy";

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => "1484c660-94ec-11ea-a64d-17be303ea466";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 6).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var minDrivers = new Version(411, 0);
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
            return new ZEnemy(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "vcruntime140.dll", "z-enemy.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() == 0) return false;
                if (ids.Contains(AlgorithmType.KAWPOW) && benchmarkedPluginVersion.Major == 10 && benchmarkedPluginVersion.Minor < 1) return true;
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
