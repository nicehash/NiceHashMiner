using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Phoenix
{
    public partial class PhoenixPlugin : PluginBase, IDevicesCrossReference
    {
        public PhoenixPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://bitcointalk.org/index.php?topic=2647654.0
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "5.1c",
                ExePath = new List<string> { "PhoenixMiner_5.1c_Windows", "PhoenixMiner.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/3.0.0.2/PhoenixMiner_5.1c_Windows.zip",
                    "https://mega.nz/folder/2VskDJrI#lsQsz1CdDe8x5cH3L8QaBw/file/7MsCwayD" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Phoenix Miner is fast Ethash miner that supports both AMD and Nvidia cards(including in mixed mining rigs).",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "fa369d10-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(14, 0);
        public override string Name => "Phoenix";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // map ids by bus ids
            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            int indexAMD = -1;
            foreach (var gpu in gpus.Where(gpu => gpu is AMDDevice))
            {
                _mappedIDs[gpu.UUID] = ++indexAMD;
            }

            int indexNVIDIA = -1;
            foreach (var gpu in gpus.Where(gpu => gpu is CUDADevice))
            {
                _mappedIDs[gpu.UUID] = ++indexNVIDIA;
            }

            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
#warning TEMP disable NVIDIA driver check
            //var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= new Version(377, 0);
            var isDriverSupported = true;
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
            var isSupported = dev is AMDDevice gpu;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(IGpuDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 3;
            return isSupported && isDriverSupported;
        }

        public override bool CanGroup(MiningPair a, MiningPair b)
        {
            var isSameDeviceType = a.Device.DeviceType == b.Device.DeviceType;
            if (!isSameDeviceType) return false;
            return base.CanGroup(a, b);
        }

        protected override MinerBase CreateMinerBase()
        {
            return new Phoenix(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedIDs.Count == 0) return;
            
            var containsAMD = devices.Any(dev => dev.DeviceType == DeviceType.AMD);
            var containsNVIDIA = devices.Any(dev => dev.DeviceType == DeviceType.NVIDIA);

            var minerBinPath = GetBinAndCwdPaths().Item1;

            if (containsAMD)
            {
                await MapDeviceCrossRefference(devices, minerBinPath, "-list -amd -gbase 0");
            }
            if (containsNVIDIA)
            {
                await MapDeviceCrossRefference(devices, minerBinPath, "-list -nvidia -gbase 0");
            }

        }

        private async Task MapDeviceCrossRefference(IEnumerable<BaseDevice> devices, string minerBinPath, string parameters)
        {
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, parameters);
            var mappedDevs = DevicesListParser.ParsePhoenixOutput(output, devices);

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "PhoenixMiner.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                var reBench = benchmarkedPluginVersion.Major == 2 && benchmarkedPluginVersion.Minor < 2;
                return reBench;
            }
            catch (Exception e)
            {
                Logger.Error("PhoenixPlugin", $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
