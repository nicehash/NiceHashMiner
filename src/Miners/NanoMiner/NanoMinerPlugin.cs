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
    public partial class NanoMinerPlugin : PluginBase, IDevicesCrossReference, IDriverIsMinimumRecommended, IDriverIsMinimumRequired
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
                BinVersion = "v3.6.3",
                ExePath = new List<string> { "nanominer-windows-3.6.3-cuda11", "nanominer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nanopool/nanominer/releases/download/v3.6.3/nanominer-windows-3.6.3-cuda11.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Nanominer is a versatile tool for mining cryptocurrencies which are based on Ethash, Ubqhash, Cuckoo Cycle (Сortex coin),RandomX (Monero), KawPow (Ravencoin) and RandomHash (PascalCoin) algorithms.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

#if LHR_BUILD_ON
        public override string PluginUUID => "NanoMiner_LHR";

        public override string Name => "NanoMiner_LHR";
#else
        public override string PluginUUID => "f25fee20-94eb-11ea-a64d-17be303ea466";

        public override string Name => "NanoMiner";
#endif
        public override Version Version => new Version(17, 0);

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
            foreach (var gpu in gpus) _mappedIDs[gpu.UUID] = ++pcieId;
            var minDrivers = new Version(455, 23);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= new Version(456, 38);
            if (!isDriverSupported)
            {
                Logger.Error("NanoMinerPlugin", $"GetSupportedAlgorithms installed NVIDIA driver is not supported. minimum {minDrivers}, installed {CUDADevice.INSTALLED_NVIDIA_DRIVERS}");
            }

            var supported = gpus
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported))
                .Cast<BaseDevice>()
                .Select(gpu => (gpu, algorithms: GetSupportedAlgorithmsForDevice(gpu)))
                .Where(p => p.algorithms.Any())
                .ToDictionary(p => p.gpu, p => p.algorithms);
            return supported;
        }

        private static bool IsSupportedAMDDevice(IGpuDevice dev) => dev is AMDDevice;

        private static bool IsSupportedNVIDIADevice(IGpuDevice dev, bool isDriverSupported) => isDriverSupported && dev is CUDADevice;

        protected override MinerBase CreateMinerBase() => new NanoMiner(PluginUUID, _mappedIDs);

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedIDs.Count == 0) return;
            var minerBinPath = GetBinAndCwdPaths().binPath;

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
            var pluginRootBinsPath = GetBinAndCwdPaths().cwdPath;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "service.dll", "nanominer.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }

        public (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRecommended(BaseDevice device)
        {
            return DriverVersionChecker.CompareAMDDriverVersions(device, new Version(21, 5, 2));
        }


        public (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRequired(BaseDevice device)
        {
            return DriverVersionChecker.CompareCUDADriverVersions(device, CUDADevice.INSTALLED_NVIDIA_DRIVERS, new Version(411, 31));
        }
    }
}
