using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
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
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://github.com/NebuTech/NBMiner/releases/ 
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v39.4",
                ExePath = new List<string> { "NBMiner_Win", "nbminer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/NebuTech/NBMiner/releases/download/v39.4/NBMiner_39.4_Win.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "GPU Miner for GRIN, AE and ETH mining.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "f683f550-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 3);
        public override string Name => "NBMiner";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        private static bool isSupportedVersion(int major, int minor)
        {
            var nbMinerSMSupportedVersions = new List<Version>
            {
                new Version(6,0),
                new Version(6,1),
                new Version(7,0),
                new Version(7,5),
                new Version(8,0),
                new Version(8,6),
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

            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNvidiaDevice(dev))
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID)
                .Cast<BaseDevice>()
                .Select((gpu, minerDeviceId) => (gpu, minerDeviceId))
                .ToArray();

            // NBMiner sortes devices by PCIe and indexes are 0 based
            foreach (var (gpu, minerDeviceId) in gpus)
            {
                _mappedIDs[gpu.UUID] = minerDeviceId;
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (gpu is CUDADevice cuda && cuda.SM_major >= 8) algorithms = algorithms.Where(a => a.FirstAlgorithmType != AlgorithmType.GrinCuckatoo32).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedNvidiaDevice(BaseDevice dev)
        {
            var minDrivers = new Version(377, 0);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;
            if (isDriverSupported && dev is CUDADevice cudaDev) return isSupportedVersion(cudaDev.SM_major, cudaDev.SM_minor);
            return false;
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice gpu && Checkers.IsGcn4(gpu);
            return isSupported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new NBMiner(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            try
            {
                if (_mappedIDs.Count == 0) return;
                var (minerBinPath, minerCwdPath) = GetBinAndCwdPaths();
                var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--device-info-json --no-watchdog"); // AMD + NVIDIA
                var dumpFile = $"d{DateTime.UtcNow.Ticks}.txt";
                try
                {
                    File.WriteAllText(Path.Combine(minerCwdPath, dumpFile), output);
                }
                catch (Exception e)
                {
                    Logger.Error("NBMiner", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
                }
                var mappedDevs = DevicesListParser.ParseNBMinerOutput(output, devices);

                foreach (var (uuid, indexID) in mappedDevs.Select(kvp => (kvp.Key, kvp.Value)))
                {
                    _mappedIDs[uuid] = indexID;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NBMiner", $"Error during DevicesCrossReference: {ex.Message}");
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nbminer.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() == 0) return false;
                if (benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 3 && device.DeviceType == DeviceType.NVIDIA && ids.Contains(AlgorithmType.DaggerHashimoto)) return true;
                if ((benchmarkedPluginVersion.Major < 16 || (benchmarkedPluginVersion.Major == 16 && benchmarkedPluginVersion.Minor < 2)) && device.DeviceType == DeviceType.AMD && ids.Contains(AlgorithmType.DaggerHashimoto)) return true;
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
