using NHM.Common.Algorithm;
using NHM.Common;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NanoMiner
{
    public partial class NanoMinerPlugin : PluginBase, IDevicesCrossReference//, IDriverIsMinimumRecommended, IDriverIsMinimumRequired
    {
        public NanoMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://github.com/nanopool/nanominer/releases
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v3.5.2",
                ExePath = new List<string> { "nanominer-windows-3.5.2-cuda11", "nanominer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nanopool/nanominer/releases/download/v3.5.2/nanominer-windows-3.5.2-cuda11.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Nanominer is a versatile tool for mining cryptocurrencies which are based on Ethash, Ubqhash, Cuckoo Cycle (Сortex coin),RandomX (Monero), KawPow (Ravencoin) and RandomHash (PascalCoin) algorithms.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "f25fee20-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 4);

        public override string Name => "NanoMiner";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // map ids by bus ids
            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            int pcieId = -1;
            foreach (var gpu in gpus)
            {
                _mappedIDs[gpu.UUID] = ++pcieId;
            }
            var minDrivers = new Version(455, 23);
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= new Version(456, 38);
            if (!isDriverSupported)
            {
                Logger.Error("NanoMinerPlugin", $"GetSupportedAlgorithms installed NVIDIA driver is not supported. minimum {minDrivers}, installed {CUDADevice.INSTALLED_NVIDIA_DRIVERS}");
            }
            var supportedGpus = gpus.Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported));

            foreach (var gpu in supportedGpus)
            {
                var algorithms = GetSupportedAlgorithmsForDevice(gpu as BaseDevice);
                if (algorithms.Count > 0) supported.Add(gpu as BaseDevice, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(IGpuDevice dev)
        {
            var isSupported = dev is AMDDevice;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(IGpuDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice;
            return isSupported && isDriverSupported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new NanoMiner(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedIDs.Count == 0) return;
            var minerBinPath = GetBinAndCwdPaths().Item1;

            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "-d");
            var mappedDevs = DevicesListParser.ParseNanoMinerOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedIDs[uuid] = indexID;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "service.dll", "nanominer.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }

        public (int ret, Version minRequired) IsDriverMinimumRecommended(BaseDevice device)
        {
            var minAMD = new Version(21, 5, 2);
            if (device is AMDDevice amd)
            {
                if (amd.DEVICE_AMD_DRIVER < minAMD) return (-2, minAMD);
                return (0, minAMD);
            }
            return (-1, new Version(0, 0));
        }


        public (int ret, Version minRequired) IsDriverMinimumRequired(BaseDevice device)
        {
            var minNVIDIA = new Version(411, 31);
            if (device is CUDADevice nvidia)
            {
                if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minNVIDIA) return (-2, minNVIDIA);
                return (0, minNVIDIA);
            }
            return (-1, new Version(0, 0));
        }
    }
}
