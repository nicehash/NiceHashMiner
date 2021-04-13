using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTMiner
{
    public partial class TTMinerPlugin : PluginBase, IDevicesCrossReference
    {
        public TTMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://bitcointalk.org/index.php?topic=5025783.0 
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "4.0.3",
                ExePath = new List<string> { "TT-Miner.exe" },
                Urls = new List<string>
                {
                    "https://tradeproject.de/download/Miner/TT-Miner-4.0.3.zip" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "TT-Miner is mining software for NVIDIA devices.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "074d4a80-94ec-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 0);
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
                var algos = GetSupportedAlgorithmsForDevice(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> {
                "nvml.dll",
                "nvrtc64_100_0.dll",
                "nvrtc64_101_0.dll",
                "nvrtc64_102_0.dll",
                "nvrtc64_92.dll",
                "nvrtc-builtins64_100.dll",
                "nvrtc-builtins64_101.dll",
                "nvrtc-builtins64_102.dll",
                "nvrtc-builtins64_92.dll",
                "TT-Miner.exe",
                "TT-SubSystem.dll",
                "Algos/AlgoEagleSong.dll",
                "Algos/AlgoEagleSong-C100.dll",
                "Algos/AlgoEagleSong-C101.dll",
                "Algos/AlgoEagleSong-C92.dll",
                "Algos/AlgoEthash.dll",
                "Algos/AlgoEthash-C100.dll",
                "Algos/AlgoEthash-C101.dll",
                "Algos/AlgoEthash-C92.dll",
                "Algos/AlgoLyra2Rev3.dll",
                "Algos/AlgoLyra2Rev3-C100.dll",
                "Algos/AlgoLyra2Rev3-C101.dll",
                "Algos/AlgoLyra2Rev3-C92.dll",
                "Algos/AlgoMTP.dll",
                "Algos/AlgoMTP-C100.dll",
                "Algos/AlgoMTP-C101.dll",
                "Algos/AlgoMTP-C92.dll",
                "Algos/AlgoProgPoW.dll",
                "Algos/AlgoProgPoW-C100.dll",
                "Algos/AlgoProgPoW-C101.dll",
                "Algos/AlgoProgPoW-C92.dll"
            });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (ids.Count() != 0)
            {
                if (ids.Contains(AlgorithmType.KAWPOW) && benchmarkedPluginVersion.Major == 10 && benchmarkedPluginVersion.Minor < 1) return true;
            }
            return false;
        }
    }
}
