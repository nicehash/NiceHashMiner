using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTMiner
{
    public class TTMinerPlugin : PluginBase, IDevicesCrossReference
    {
        public TTMinerPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            // https://bitcointalk.org/index.php?topic=5025783.0 current 3.0.10 // TODO update
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "3.0.10",
                ExePath = new List<string> { "TT-Miner.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/1.9.1.12b/TT-Miner-3.0.10.zip",
                    "https://tradeproject.de/download/Miner/TT-Miner-3.0.10.zip" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "TT-Miner is mining software for NVIDIA devices.",
                SupportedDevicesAlgorithms = PluginSupportedAlgorithms.SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "f1945a30-7237-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(3, 2);
        public override string Name => "TTMiner";
        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        protected override MinerBase CreateMinerBase()
        {
            return new TTMiner(PluginUUID, _mappedDeviceIds);
        }

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
                _mappedDeviceIds[gpu.UUID] = gpu.ID; //lazy init -> TT-Miner sorts devices as we do
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = PluginSupportedAlgorithms.GetSupportedAlgorithmsNVIDIA(PluginUUID);
            if (PluginSupportedAlgorithms.UnsafeLimits(PluginUUID)) return algorithms;
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            var minerBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "-list");
            var mappedDevs = DevicesListParser.ParseTTMinerOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedDeviceIds[uuid] = indexID;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nvml.dll", "nvrtc64_92.dll", "nvrtc64_100_0.dll", "nvrtc64_101_0.dll",
                "nvrtc-builtins64_92.dll", "nvrtc-builtins64_100.dll", "nvrtc-builtins64_101.dll", "TT-SubSystem.dll", "TT-Miner.exe", @"Algos\AlgoEthash.dll", @"Algos\AlgoEthash-C92.dll",
                @"Algos\AlgoEthash-C100.dll",  @"Algos\AlgoLyra2Rev3-C100.dll", @"Algos\AlgoLyra2Rev3-C92.dll", @"Algos\AlgoLyra2Rev3.dll", @"Algos\AlgoMTP-C100.dll",
                @"Algos\AlgoMTP-C92.dll", @"Algos\AlgoMTP.dll", @"Algos\AlgoMyrGr-C100.dll", @"Algos\AlgoMyrGr-C92.dll", @"Algos\AlgoMyrGr.dll", @"Algos\AlgoProgPoW092-C100.dll",
                @"Algos\AlgoProgPoW092-C92.dll", @"Algos\AlgoProgPoW092.dll", @"Algos\AlgoProgPoW.dll",  @"Algos\AlgoProgPoW-C100.dll", @"Algos\AlgoProgPoW-C92.dll",
                @"Algos\AlgoProgPoWZ-C100.dll", @"Algos\AlgoProgPoWZ-C92.dll", @"Algos\AlgoProgPoWZ.dll", @"Algos\AlgoTethashV1-C100.dll", @"Algos\AlgoTethashV1-C92.dll", @"Algos\AlgoTethashV1.dll",
                @"Algos\AlgoUbqhash-C100.dll", @"Algos\AlgoUbqhash-C92.dll", @"Algos\AlgoUbqhash.dll"
            });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (ids.Count() == 0) return false;
            if (benchmarkedPluginVersion.Major == 2 && benchmarkedPluginVersion.Minor < 3) return true; //improvement on all supported algorithms
            return false;
        }
    }
}
