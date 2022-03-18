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

namespace LolMiner
{
    public partial class LolMinerPlugin : PluginBase, IDevicesCrossReference//, IDriverIsMinimumRecommended, IDriverIsMinimumRequired
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
                BinVersion = "1.42",
                ExePath = new List<string> { "1.42", "lolMiner.exe" },
                Urls = new List<string>
                {
                    "https://github.com/Lolliedieb/lolMiner-releases/releases/download/1.42/lolMiner_v1.42_Win64.zip" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Miner for AMD and Nvidia gpus.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(16, 6);

        public override string Name => "lolMiner";

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => "eb75e920-94eb-11ea-a64d-17be303ea466";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var minDrivers = new Version(470, 5);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;

            if (!isDriverSupported)
            {
                Logger.Error("LolMinerPlugin", $"GetSupportedAlgorithms installed NVIDIA driver is not supported. minimum {minDrivers}, installed {CUDADevice.INSTALLED_NVIDIA_DRIVERS}");
            }

            var gpus = devices
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported))
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID)
                .Cast<BaseDevice>();

            var pcieId = 0;
            foreach (var gpu in gpus)
            {
                // map supported NVIDIA devices so indexes match
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                // add AMD and Nvidia
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
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
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 5;
            return isSupported && isDriverSupported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new LolMiner(PluginUUID, _mappedDeviceIds);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;

            var (minerBinPath, minerCwdPath) = GetBinAndCwdPaths();
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--list-devices --nocolor=on");
            var ts = DateTime.UtcNow.Ticks;
            var dumpFile = $"d{ts}.txt";
            try
            {
                File.WriteAllText(Path.Combine(minerCwdPath, dumpFile), output);
            }
            catch (Exception e)
            {
                Logger.Error("LolMinerPlugin", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
            }
            var mappedDevs = DevicesListParser.ParseLolMinerOutput(output, devices);

            foreach (var (uuid, minerGpuId) in mappedDevs)
            {
                Logger.Info("LolMinerPlugin", $"DevicesCrossReference '{uuid}' => {minerGpuId}");
                _mappedDeviceIds[uuid] = minerGpuId;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "lolMiner.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (ids.Count() != 0)
            {
                if (ids.FirstOrDefault() == AlgorithmType.DaggerHashimoto && benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 5 && device.Name.ToLower().Contains("r9 390")) return true;
                // LHR re-benchmark
                if (device.DeviceType == DeviceType.NVIDIA && ids.FirstOrDefault() == AlgorithmType.DaggerHashimoto && benchmarkedPluginVersion < Version) return true;
            }
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
