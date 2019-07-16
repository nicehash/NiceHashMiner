using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoDredge
{
    public class CryptoDredgePlugin : PluginBase
    {
        public CryptoDredgePlugin()
        {
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
        }

        public override Version Version => new Version(2, 0);
        public override string Name => "CryptoDredge";

        public override string Author => "domen.kirnkrefl@nicehash.com";

        public override string PluginUUID => "d9c2e620-7236-11e9-b20c-f9f12eb6d835";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var minDrivers = new Version(397, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
                new Algorithm(PluginUUID, AlgorithmType.X16R),
                new Algorithm(PluginUUID, AlgorithmType.MTP) { Enabled = false }
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new CryptoDredge(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
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
