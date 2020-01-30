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

namespace LolMiner
{
    public partial class LolMinerPlugin : PluginBase, IDevicesCrossReference
    {
        public LolMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://github.com/Lolliedieb/lolMiner-releases/releases | https://bitcointalk.org/index.php?topic=4724735.0 
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "0.9.7",
                ExePath = new List<string> { "0.9.7", "lolMiner.exe" },
                Urls = new List<string>
                {
                    "https://github.com/Lolliedieb/lolMiner-releases/releases/download/0.97/lolMiner_v097_Win64.zip" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Miner for AMD and NVIDIA gpus.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(7, 2);

        public override string Name => "lolMiner";

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => "435f0820-7237-11e9-b20c-f9f12eb6d835";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // NVIDIA backend is NOT CUDA but OpenCL!!!!
            //CUDA 9.0+: minimum drivers 384.xx
            var minDrivers = new Version(384, 0);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;

            var gpus = devices
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported))
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            var pcieId = 0;
            foreach (var gpu in gpus)
            {
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetSupportedAlgorithmsForDevice(gpu as BaseDevice);
                if (algorithms.Count > 0) supported.Add(gpu as BaseDevice, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(BaseDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 2 && gpu.IsOpenCLBackendEnabled;
            return isSupported && isDriverSupported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new LolMiner(PluginUUID, _mappedDeviceIds);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            // will block
            var minerBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--benchmark BEAM --longstats 60 --devices -1", new List<string> { "Start Benchmark..." });
            var mappedDevs = DevicesListParser.ParseLolMinerOutput(output, devices.ToList());

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "lolMiner.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (ids.Count() == 0) return false;
            if (benchmarkedPluginVersion.Major <= 7 && ids.FirstOrDefault() == AlgorithmType.Cuckaroom)
            {
                if (benchmarkedPluginVersion.Major == 7 && benchmarkedPluginVersion.Minor == 1) return false;
                if (device.DeviceType == DeviceType.AMD) return true;
            }
            if(benchmarkedPluginVersion.Major <=7 && ids.FirstOrDefault() == AlgorithmType.GrinCuckatoo32)
            {
                if (benchmarkedPluginVersion.Major == 7 && benchmarkedPluginVersion.Minor == 2) return false;
                if (device.DeviceType == DeviceType.AMD && device.Name.ToLower().Contains("navi")) return true;
            }

            return false;
        }
    }
}
