using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    /// <summary>
    /// In plugin class we set all settings used by miner (for example pluginUUID, GetSupportedAlgorithms, Internal Settings and more)
    /// Interfaces from MinerPlugin and MinerPluginToolkitV1 provide large set of settings available for use
    /// </summary>
    public class ExamplePlugin: IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeout
    {
        /// <summary>
        /// _pluginUUID is mandatory property of each plugin. It is used to distinguish between them.
        /// </summary>
        private readonly string _pluginUUID;

        // In constructors we set _pluginUUID to generated uuid
        public ExamplePlugin()
        {
            _pluginUUID = "441c74df-d0a6-4c32-a300-32bc0964af70";
        }
        public ExamplePlugin(string pluginUUID = "441c74df-d0a6-4c32-a300-32bc0964af70")
        {
            _pluginUUID = pluginUUID;
        }
        public string PluginUUID => _pluginUUID;

        /// <summary>
        /// With version we set the version of the plugin for further updating capabilities.
        /// </summary>
        public Version Version => new Version(1, 0);

        /// <summary>
        /// This is the name of the plugin
        /// </summary>
        public string Name => "Example";

        /// <summary>
        /// Developer sets his email/name/nickname as Author so it can be used for contact information in case of any issues.
        /// </summary>
        public string Author => "developer@email.com";

        /// <summary>
        /// Creates a new miner instance with required settings
        /// </summary>
        public IMiner CreateMiner()
        {
            return new ExampleMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
            };
        }

        /// <summary>
        /// GetSupportedAlgorithms returns algorithms supported by Miner.
        /// You can also apply hardware filters here.
        /// In this example we will set filter for AMD gpu generation, NVIDIA driver version and NVIDIA CUDA SM version.
        /// </summary>
        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // In this block we take all devices of AMDDevice type and of generation greather than 4th
            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();
            foreach (var gpu in amdGpus)
            {
                var algorithms = GetAMDSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            // This block ensures us that we don't have too old drivers installed - otherwise it won't return any algorithms for nvidia
            // also we filter devices by CUDA SM5.0+
            var minDrivers = new Version(384, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers)
            {
                var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();
                foreach (var gpu in cudaGpus)
                {
                    var algos = GetCUDASupportedAlgorithms(gpu).ToList();
                    if (algos.Count > 0) supported.Add(gpu, algos);
                }
            }
            return supported;
        }

        /// <summary>
        /// Returns list of AMD supported algorithms by our miner. At the end they are filtered by gpu ram.
        /// </summary>
        IReadOnlyList<Algorithm> GetAMDSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle) {Enabled = false },  // this algorithm is disabled by default
            };
            //we filter out algorithms that would require more ram than gpu has
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }
        /// <summary>
        /// Returns list of CUDA supported algorithms by our miner. At the end they are filtered by gpu ram.
        /// </summary>
        IReadOnlyList<Algorithm> GetCUDASupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle) {Enabled = false }, // disabled by default
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        /// <summary>
        /// CanGroup checks if miner can run multiple devices with same algorithm in one miner instance
        /// </summary>
        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        #region Internal Settings
        /// <summary>
        /// InitInternals reads/writes internal settings to files 
        /// </summary>
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;

            var fileMinerReservedPorts = InternalConfigs.InitMinerReservedPorts(pluginRoot, _minerReservedApiPorts);
            if (fileMinerReservedPorts != null) _minerReservedApiPorts = fileMinerReservedPorts;
        }

        /// <summary>
        /// _minerOptionsPackage holds general and temperature options of type <see cref="MinerOption"/> used as extra launch parameters
        /// </summary>
        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// personalization string for equihash algorithm (for example: 'BgoldPoW', 'BitcoinZ', 'Safecoin')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "example_pers",
                    LongName = "--pers",
                },
                /// <summary>
                /// option to control GPU intensity (--intensity, 1-100)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "example_intensity",
                    LongName = "--intensity",
                    // assume it is like the others
                    DefaultValue = "-1",
                    Delimiter = " "
                }
            },
            TemperatureOptions = new List<MinerOption>{
                /// <summary>
                /// space-separated list of temperature limits, upon reaching the limit, the GPU stops mining until it cools down, can be empty (for example: '85 80 75')
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "example_templimit",
                    ShortName = "-t",
                    LongName = "--templimit",
                    DefaultValue = "90",
                    Delimiter = " "
                }
            }
        };

        /// <summary>
        /// _minerSystemEnvironmentVariables holds system environment variables that get set before launching miner
        /// </summary>
        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {
            DefaultSystemEnvironmentVariables = new Dictionary<string, string>
            {
                { "XMRSTAK_NOWAIT", "1" },
                // https://github.com/fireice-uk/xmr-stak/blob/master/doc/tuning.md#increase-memory-pool
                // for AMD backend
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
                {"GPU_FORCE_64BIT_PTR", "1"}
            }
        };

        /// <summary>
        /// _minerReservedApiPorts holds list of reserved ports for specific algorithm.
        /// </summary>
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts {
            AlgorithmReservedPorts = new Dictionary<string, List<int>>
            {
                { "CuckooCycle", new List<int>{4004, 4005 } },
                { "GrinCuckaroo29", new List<int>{ 4001, 4002} }
            }
        };
        #endregion InternalSettings

        /// <summary>
        /// CheckBinaryPackageMissingFiles checks if all files required by miner exist.
        /// Files of type .exe and .dll are listed here.
        /// </summary>
        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "miner.exe", "libcurl.dll" });
        }

        /// <summary>
        /// ShouldReBenchmarkAlgorithmOnDevice is used to perform re-benchmark after update of miner to get new accurate results.
        /// This can be caused by performance improvements, bug fixes, etc.
        /// </summary>
        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            // We compare versions to see if the re-benchmark is needed
            var benchmarkedVersionIsSame = Version.Major == benchmarkedPluginVersion.Major && Version.Minor == benchmarkedPluginVersion.Minor;
            var benchmarkedVersionIsOlder = Version.Major >= benchmarkedPluginVersion.Major && Version.Minor > benchmarkedPluginVersion.Minor;
            if (benchmarkedVersionIsSame || !benchmarkedVersionIsOlder) return false;
            if (ids.Count() == 0) return false;

            // here we define which algorithms should be re-benchmarked. We could also filter by device type.
            var singleAlgorithm = ids[0];
            if (singleAlgorithm == AlgorithmType.Beam) return true;
            if (singleAlgorithm == AlgorithmType.GrinCuckaroo29) return true;

            return false;
        }

        /// <summary>
        /// Used for API watchdog to define max timeout for timedOutGroups that define which miners should be restarted
        /// </summary>
        public TimeSpan GetApiMaxTimeout()
        {
            return new TimeSpan(0, 2, 0);
        }
    }
}
