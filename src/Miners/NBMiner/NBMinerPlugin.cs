using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;

namespace NBMiner
{
    public class NBMinerPlugin : IMinerPlugin, IInitInternals, IDevicesCrossReference, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker
    {
        public NBMinerPlugin()
        {
            _pluginUUID = "6c07f7a0-7237-11e9-b20c-f9f12eb6d835";
        }
        public NBMinerPlugin(string pluginUUID = "6c07f7a0-7237-11e9-b20c-f9f12eb6d835")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 2);
        public string Name => "NBMiner";

        public string Author => "Dillon Newell";
        
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

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
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
                Shared.MappedCudaIds[gpu.UUID] = pcieID;
                ++pcieID;
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle),
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        public IMiner CreateMiner()
        {
            return new NBMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption> { 
                /// <summary>
                /// Set intensity of cuckoo, cuckaroo, cuckatoo, [1, 12]. Smaller value means higher CPU usage to gain more hashrate. Set to 0 means autumatically adapt. Default: 0.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_intensity",
                    LongName = "--cuckoo-intensity",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Set this option to reduce the range of power consumed by rig when minining with algo cuckatoo.
                /// This feature can reduce the chance of power supply shutdown caused by overpowered.
                /// Warning: Setting this option may cause drop on minining performance.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nbminer_powerOptimize",
                    LongName = "--cuckatoo-power-optimize",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Generate log file named `log_<timestamp>.txt`.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_log",
                    LongName = "--log"
                },
                /// <summary>
                /// Use 'yyyy-MM-dd HH:mm:ss,zzz' for log time format.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "nbminer_longTimeFormat",
                    LongName = "--long-format"
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        #endregion Internal Settings

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            // TODO will break
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--device-info-json -RUN");
            var mappedDevs = DevicesListParser.ParseNBMinerOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                Shared.MappedCudaIds[uuid] = indexID;
            }
        }

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nbminer.exe", "OhGodAnETHlargementPill-r2.exe" });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            var benchmarkedVersionIsSame = Version.Major == benchmarkedPluginVersion.Major && Version.Minor == benchmarkedPluginVersion.Minor;
            var benchmarkedVersionIsOlder = Version.Major >= benchmarkedPluginVersion.Major && Version.Minor > benchmarkedPluginVersion.Minor;
            if (benchmarkedVersionIsSame || !benchmarkedVersionIsOlder) return false;
            if (ids.Count() == 0) return false;
            // plugin version 1.2 bundles NBMiner v21.3
            // plugin version 1.3 bundles NBMiner v23.2
            // performance and compatibility optimizations Grin29 and Grin31,
            // Added support for CuckoCycle
            // there were 5 releases in between and many changes
            if (device.DeviceType != DeviceType.NVIDIA) return false;
            var singleAlgorithm = ids[0];
            if (singleAlgorithm == AlgorithmType.GrinCuckaroo29) return true;
            if (singleAlgorithm == AlgorithmType.GrinCuckatoo31) return true;

            return false;
        }
    }
}
