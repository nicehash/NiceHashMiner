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
using System.Text;
using System.Threading.Tasks;

namespace MP.GMiner
{
    public partial class GMinerPlugin : PluginBase, IDevicesCrossReference, IDriverIsMinimumRequired, IDriverIsMinimumRecommended
    {
        public GMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            MinerCommandLineSettings = PluginInternalSettings.MinerCommandLineSettings;
            // set default internal settings
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v3.44",
                ExePath = new List<string> { "", "miner.exe" },
                Urls = new List<string>
                {
                    "https://github.com/develsoftware/GMinerRelease/releases/download/3.44/gminer_3_44_windows64.zip" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "GPU Miner for multiple algorithms.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "d8ddcaf2-95c5-4f9a-b65f-c123a0d4fbc2";

        public override string Name => "GMiner";

        public override Version Version => new Version(24, 0);


        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        private static bool isSupportedVersion(int major, int minor)
        {
            //todo is there even a list for this?
            return true;
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var minDrivers = new Version(377, 0);
            var supportedNVIDIA_Driver = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;
            if (!supportedNVIDIA_Driver)
            {
                Logger.Error("GMinerPlugin", $"IsSupportedNvidiaDevice: installed NVIDIA driver is not supported. minimum {minDrivers}, installed {CUDADevice.INSTALLED_NVIDIA_DRIVERS}");
            }

            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Where(dev => IsSupportedAMDDevice(dev) || (supportedNVIDIA_Driver && IsSupportedNvidiaDevice(dev)))
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
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedNvidiaDevice(BaseDevice dev)
        {
            return dev is CUDADevice cudaDev && isSupportedVersion(cudaDev.SM_major, cudaDev.SM_minor);
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice gpu && Checkers.IsGcn4(gpu);
            return isSupported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new GMiner(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            try
            {
                if (_mappedIDs.Count == 0) return;
                var (minerBinPath, minerCwdPath) = GetBinAndCwdPaths();
                var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--list_devices"); // AMD + NVIDIA
                var dumpFile = $"d{DateTime.UtcNow.Ticks}.txt";
                try
                {
                    File.WriteAllText(Path.Combine(minerCwdPath, dumpFile), output);
                }
                catch (Exception e)
                {
                    Logger.Error("GMiner", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
                }
                var mappedDevs = DevicesListParser.ParseGMinerOutput(output, devices.ToList());

                foreach (var (uuid, indexID) in mappedDevs.Select(kvp => (kvp.Key, kvp.Value)))
                {
                    _mappedIDs[uuid] = indexID;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GMiner", $"Error during DevicesCrossReference: {ex.Message}");
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().cwdPath;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "miner.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() == 0) return false;
                if (device.DeviceType != DeviceType.NVIDIA) return false;
                if (ids.FirstOrDefault() != AlgorithmType.DaggerHashimoto) return false;
                return benchmarkedPluginVersion < Version;
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }

        public (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRequired(BaseDevice device)
        {
            return DriverVersionChecker.CompareCUDADriverVersions(device, CUDADevice.INSTALLED_NVIDIA_DRIVERS, new Version(411, 31));
        }

        public (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRecommended(BaseDevice device)
        {
            return DriverVersionChecker.CompareAMDDriverVersions(device, new Version(21, 5, 2));
        }
    }
}
