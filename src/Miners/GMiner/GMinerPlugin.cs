using NHM.Common;
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

namespace GMinerPlugin
{
    public partial class GMinerPlugin : PluginBase, IDevicesCrossReference
    {
        public GMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://bitcointalk.org/index.php?topic=5034735.0 | https://github.com/develsoftware/GMinerRelease/releases
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "2.54",
                ExePath = new List<string> { "miner.exe" },
                Urls = new List<string>
                {
                    "https://github.com/develsoftware/GMinerRelease/releases/download/2.54/gminer_2_54_windows64.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "GMiner - High-performance miner for NVIDIA and AMD GPUs.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "e7a58030-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 1);

        public override string Name => "GMinerCuda9.0+";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        protected override MinerBase CreateMinerBase()
        {
            return new GMiner(PluginUUID, _mappedDeviceIds);
        }


        // Supported algoritms:
        //   - Cuckaroo29/Cuckatoo31 (Grin)
        //   - Cuckoo29 (Aeternity)
        //   - Equihash 96,5 (MinexCoin)
        //   - Equihash 144,5 (Bitcoin Gold, BitcoinZ, SnowGem, SafeCoin, Litecoin Z) // ZHash
        //   - Equihash 150,5 (BEAM)
        //   - Equihash 192,7 (Zero, Genesis)
        //   - Equihash 210,9 (Aion)

        // Requirements:
        //   - CUDA compute compability 5.0+ #1
        //   - Cuckaroo29 ~ 5.6GB VRAM
        //   - Cuckatoo31 ~ 7.4GB VRAM
        //   - Cuckoo29 ~ 5.6GB VRAM
        //   - Equihash 96,5 ~0.75GB VRAM
        //   - Equihash 144,5 ~1.75GB VRAM
        //   - Equihash 150,5 ~2.9GB VRAM
        //   - Equihash 192,7 ~2.75GB VRAM
        //   - Equihash 210,9 ~1GB VRAM
        //   - CUDA 9.0+ 

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var gpus = devices
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev))
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            var pcieId = 0; // GMiner sortes devices by PCIe
            foreach (var gpu in gpus)
            {
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                if (gpu is AMDDevice amd)
                {
                    var algorithms = GetSupportedAlgorithmsForDevice(amd);
                    if (algorithms.Count > 0) supported.Add(amd, algorithms);
                }
                if (gpu is CUDADevice cuda)
                {
                    var algorithms = GetSupportedAlgorithmsForDevice(cuda);
                    if (algorithms.Count > 0) supported.Add(cuda, algorithms);
                }
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice gpu && Checkers.IsGcn4(gpu);
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(BaseDevice dev)
        {
            //CUDA 9.0+: minimum drivers 384.xx
            var minDrivers = new Version(384, 0);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 5;
            return isSupported && isDriverSupported;
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            var minerBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--list_devices");
            var mappedDevs = DevicesListParser.ParseGMinerOutput(output, devices.ToList());

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "miner.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() != 0)
                {
                    if (benchmarkedPluginVersion.Major < 16 && ids.First() == AlgorithmType.KAWPOW) return true;
                    if (benchmarkedPluginVersion.Major < 16 && ids.First() == AlgorithmType.DaggerHashimoto) return true;
                    if (benchmarkedPluginVersion.Major < 16 && ids.First() == AlgorithmType.GrinCuckatoo32) return true;
                    if (benchmarkedPluginVersion.Major < 16 && ids.First() == AlgorithmType.BeamV3) return true;
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
