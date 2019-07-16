using System;
using System.Collections.Generic;
using System.Linq;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1;

namespace TTMiner
{
    public class TTMinerPlugin : PluginBase
    {
        public TTMinerPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
        }

        public override string PluginUUID => "f1945a30-7237-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(2, 0);
        public override string Name => "TTMiner";
        public override string Author => "stanko@nicehash.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 398.26
            var minDrivers = new Version(398, 26);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

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
                new Algorithm(PluginUUID, AlgorithmType.MTP) { Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new TTMiner(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nvml.dll", "nvrtc-builtins64_100.dll", "nvrtc-builtins64_101.dll",
                "nvrtc-builtins64_92.dll", "nvrtc64_100_0.dll", "nvrtc64_101_0.dll", "nvrtc64_92.dll", "SubSystemDll.dll", "TT-Miner.exe", @"Algos\AlgoEthash-C100.dll",
                @"Algos\AlgoEthash-C92.dll", @"Algos\AlgoEthash.dll", @"Algos\AlgoLyra2Rev3-C100.dll", @"Algos\AlgoLyra2Rev3-C92.dll", @"Algos\AlgoLyra2Rev3.dll", @"Algos\AlgoMTP-C100.dll",
                @"Algos\AlgoMTP-C92.dll", @"Algos\AlgoMTP.dll", @"Algos\AlgoMyrGr-C100.dll", @"Algos\AlgoMyrGr-C92.dll", @"Algos\AlgoMyrGr.dll", @"Algos\AlgoProgPoW-C100.dll",
                @"Algos\AlgoProgPoW-C92.dll", @"Algos\AlgoProgPoW.dll", @"Algos\AlgoUbqhash-C100.dll", @"Algos\AlgoUbqhash-C92.dll", @"Algos\AlgoUbqhash.dll"
            });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }
    }
}
