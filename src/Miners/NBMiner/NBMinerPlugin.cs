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

namespace NBMiner
{
    public partial class NBMinerPlugin : PluginBase, IDevicesCrossReference
    {
        public NBMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            // https://github.com/NebuTech/NBMiner/releases/ 
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v26.2",
                ExePath = new List<string> { "NBMiner_Win", "nbminer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/NebuTech/NBMiner/releases/download/v26.2/NBMiner_26.2_Win.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "GPU Miner for GRIN and AE mining.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "6c07f7a0-7237-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(5, 0);
        public override string Name => "NBMiner";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        private bool isSupportedVersion(int major, int minor)
        {
            var nbMinerSMSupportedVersions = new List<Version>
            {
                new Version(6,0),
                new Version(6,1),
                new Version(7,0),
                new Version(7,5),
            };
            var cudaDevSMver = new Version(major, minor);
            foreach (var supportedVer in nbMinerSMSupportedVersions)
            {
                if (supportedVer == cudaDevSMver) return true;
            }
            return false;
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 377.xx
            var minDrivers = new Version(377, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && isSupportedVersion(gpu.SM_major, gpu.SM_minor))
                .Cast<CUDADevice>()
                .OrderBy(dev => dev.PCIeBusID)
                .ToList();

            var pcieID = 0;
            foreach (var gpu in cudaGpus)
            {
                _mappedIDs[gpu.UUID] = pcieID;
                ++pcieID;
                var algos = GetSupportedAlgorithmsForDevice(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new NBMiner(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
#warning Blocks exit
            return;
            if (_mappedIDs.Count == 0) return;
            // TODO will break
            var minerBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--device-info-json --no-watchdog --platform 1"); // NVIDIA only
            var mappedDevs = DevicesListParser.ParseNBMinerOutput(output, devices.ToList());

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nbminer.exe", "OhGodAnETHlargementPill-r2.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() == 0) return false;
                if (benchmarkedPluginVersion.Major == 2 && benchmarkedPluginVersion.Minor < 2)
                {
                    // v24.2 https://github.com/NebuTech/NBMiner/releases/tag/v24.2
                    // Slightliy improve RTX2060 Grin29 performance under win10
                    var isRTX2060 = device.Name.Contains("RTX") && device.Name.Contains("2060");
                    var isGrin29 = ids.FirstOrDefault() == AlgorithmType.GrinCuckarood29;
                    return isRTX2060 && isGrin29;
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
