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

namespace NanoMiner
{
    public partial class NanoMinerPlugin : PluginBase, IDevicesCrossReference
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
                BinVersion = "v3.3.4",
                ExePath = new List<string> { "nanominer-windows-3.3.4-cuda11", "nanominer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nanopool/nanominer/releases/download/3.3.4/nanominer-windows-3.3.4-cuda11.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Nanominer is a versatile tool for mining cryptocurrencies which are based on Ethash, Ubqhash, Cuckoo Cycle (Сortex coin),RandomX (Monero), KawPow (Ravencoin) and RandomHash (PascalCoin) algorithms.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "f25fee20-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 0);

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

            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= new Version(455, 23);
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
    }
}
