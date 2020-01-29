using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
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
            // https://bitcointalk.org/index.php?topic=5089248.0 | https://github.com/nanopool/nanominer/releases
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.7.3",
                ExePath = new List<string> { "nanominer-windows-1.7.3", "nanominer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nanopool/nanominer/releases/download/v1.7.3/nanominer-windows-1.7.3.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Nanominer is a versatile tool for mining cryptocurrencies which are based on Ethash, Ubqhash, Cuckaroo29, CryptoNight (v6, v7, v8, R, ReverseWaltz) and RandomHash (PascalCoin) algorithms.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "a841b4b0-ae17-11e9-8e4e-bb1e2c6e76b4";

        public override Version Version => new Version(5, 2);

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
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= new Version(411, 31);
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
            try
            {
                var algo = ids.FirstOrDefault();
                if (benchmarkedPluginVersion.Major < 3 && algo == AlgorithmType.GrinCuckarood29) return true;
                if (benchmarkedPluginVersion.Major == 3 && benchmarkedPluginVersion.Minor < 1 && algo == AlgorithmType.GrinCuckarood29) return true;
            }
            catch (Exception e)
            {
                Logger.Error("NanoMinerPlugin.ShouldReBenchmarkAlgorithmOnDevice", e.Message);
            }
            return false;
        }
    }
}
