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
    public partial class NBMinerPlugin : PluginBase, IDevicesCrossReference, IDriverIsMinimumRequired, IDriverIsMinimumRecommended
    {
        public NBMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            MinerCommandLineSettings = PluginInternalSettings.MinerCommandLineSettings;
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://github.com/NebuTech/NBMiner/releases/ 
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v42.2",
                ExePath = new List<string> { "NBMiner_Win", "nbminer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/NebuTech/NBMiner/releases/download/v42.2/NBMiner_42.2_Win.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "GPU Miner for GRIN, AE and ETH mining.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

#if LHR_BUILD_ON
        public override string PluginUUID => "NBMiner_LHR";
        public override string Name => "NBMiner_LHR";
#else
        public override string PluginUUID => "f683f550-94eb-11ea-a64d-17be303ea466";
        public override string Name => "NBMiner";
#endif

        public override Version Version => new Version(17, 2);
        

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
            var minDrivers = new Version(377, 0);
            var supportedNVIDIA_Driver = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;
            if (!supportedNVIDIA_Driver)
            {
                Logger.Error("NBMinerPlugin", $"IsSupportedNvidiaDevice: installed NVIDIA driver is not supported. minimum {minDrivers}, installed {CUDADevice.INSTALLED_NVIDIA_DRIVERS}");
            }
#if LHR_BUILD_ON
            var gpus = devices
                .Where(dev => dev is CUDADevice)
                .Cast<CUDADevice>()
                .Where(dev => supportedNVIDIA_Driver && IsSupportedNvidiaDevice(dev))
                .Where(dev => IsLHR(dev.Name))
                .OrderBy(gpu => gpu.PCIeBusID)
                .Cast<BaseDevice>()
                .Select((gpu, minerDeviceId) => (gpu, minerDeviceId))
                .ToArray();
#else
            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Where(dev => IsSupportedAMDDevice(dev) || (supportedNVIDIA_Driver && IsSupportedNvidiaDevice(dev)))
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID)
                .Cast<BaseDevice>()
                .Select((gpu, minerDeviceId) => (gpu, minerDeviceId))
                .ToArray();
#endif


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
            return dev is CUDADevice cudaDev && isSupportedVersion(cudaDev.SM_major, cudaDev.SM_minor);
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
            var pluginRootBinsPath = GetBinAndCwdPaths().cwdPath;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nbminer.exe" });
        }

        private static bool IsLHR(string name)
        {
            var nonLHR_GPUs = new string[] { "GeForce RTX 3050", "GeForce RTX 3060", "GeForce RTX 3060 Ti", "GeForce RTX 3070", "GeForce RTX 3080", "GeForce RTX 3090" };
            return nonLHR_GPUs.Any(name.Contains);
        }

        //private static bool IsLHR_Ignore(CUDADevice dev)
        //{
        //    const ulong maxGPU_VRAM = 11UL << 30; // 11GB
        //    return dev.Name.Contains("GeForce RTX 3080") && dev.GpuRam > maxGPU_VRAM;
        //}

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() == 0) return false;
                if (device.DeviceType != DeviceType.NVIDIA) return false;
                if (ids.FirstOrDefault() != AlgorithmType.DaggerHashimoto) return false;
                if (!IsLHR(device.Name)) return false;
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
#if LHR_BUILD_ON
            return DriverVersionChecker.CompareCUDADriverVersions(device, CUDADevice.INSTALLED_NVIDIA_DRIVERS, new Version(512, 15));
#else
            return DriverVersionChecker.CompareCUDADriverVersions(device, CUDADevice.INSTALLED_NVIDIA_DRIVERS, new Version(411, 31));
#endif
        }

    public (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRecommended(BaseDevice device)
        {
            return DriverVersionChecker.CompareAMDDriverVersions(device, new Version(21, 5, 2));
        }
    }
}
