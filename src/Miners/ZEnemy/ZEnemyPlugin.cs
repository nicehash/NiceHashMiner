using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZEnemy
{
    public class ZEnemyPlugin : PluginBase
    {
        public ZEnemyPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            // https://bitcointalk.org/index.php?topic=3378390.0
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "2-2-cuda10.1", // fix version if wrong
                ExePath = new List<string> { "z-enemy.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/1.9.1.12b/z-enemy-2.2-cuda10.1.zip",
                    "https://mega.nz/#!EPJHRY4D!WyCjfcOZnDof8FuclXoEYB1BXsjX8DbvTWrCNbpzECM" // original source
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Zealot/Enemy (z-enemy) NVIDIA GPU miner.",
                SupportedDevicesAlgorithms = new Dictionary<DeviceType, List<AlgorithmType>>
                {
                    { DeviceType.NVIDIA, new List<AlgorithmType>{ AlgorithmType.X16R, AlgorithmType.X16Rv2 } }
                }
            };
        }

        public override Version Version => new Version(3, 0);

        public override string Name => "ZEnemy";

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => "5532d300-7238-11e9-b20c-f9f12eb6d835";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 6).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var minDrivers = new Version(411, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.X16R),
                new Algorithm(PluginUUID, AlgorithmType.X16Rv2)
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
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
                if (benchmarkedPluginVersion.Major == 2 && benchmarkedPluginVersion.Minor < 2)
                {
                    // v2.1 https://bitcointalk.org/index.php?topic=3378390.0
                    if (ids.FirstOrDefault() == AlgorithmType.X16R) return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
