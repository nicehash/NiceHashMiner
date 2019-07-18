using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CCMinerTpruvot
{
    public abstract class CCMinerTpruvotPlugin : PluginBase
    {
        public CCMinerTpruvotPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://github.com/tpruvot/ccminer/releases current 2.3.1
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                Urls = new List<string>
                {
                    "https://github.com/tpruvot/ccminer/releases/download/2.3.1-tpruvot/ccminer-2.3.1-cuda10.7z", // original
                }
            };
        }

        //public override string PluginUUID => "MISSING";

        public override Version Version => new Version(2, 0);
        public override string Name => "CCMinerTpruvot";

        public override string Author => "stanko@nicehash.com";

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
                var algorithms = new List<Algorithm> {
                    new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
                    new Algorithm(PluginUUID, AlgorithmType.X16R), // TODO check performance
                };
                var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
                if (filteredAlgorithms.Count > 0) supported.Add(gpu, filteredAlgorithms);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new CCMinerTpruvot(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
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
