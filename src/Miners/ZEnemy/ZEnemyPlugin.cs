using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZEnemy
{
    class ZEnemyPlugin : PluginBase
    {
        public ZEnemyPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            // https://bitcointalk.org/index.php?topic=3378390.0 current 1-28-cuda10.0 // TODO update
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                Urls = new List<string>
                {
                    "https://github.com/nicehash/NiceHashMinerLegacyTest/releases/download/1.9.1.4/z-enemy.1-28-cuda10.0.zip",
                    //"" // TODO set original source
                }
            };
        }

        public override Version Version => new Version(2, 0);

        public override string Name => "ZEnemy";

        public override string Author => "domen.kirnkrefl@nicehash.com";

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
                new Algorithm(PluginUUID, AlgorithmType.X16R)
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
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "vcruntime140.dll", "z-enemy.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }
    }
}
