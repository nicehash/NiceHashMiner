using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NiceHashMinerLegacy.Common;
using System.IO;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1;

namespace GMinerPlugin
{
    public class GMinerPlugin : IMinerPlugin, IInitInternals, IDevicesCrossReference, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeout
    {
        public GMinerPlugin()
        {
            _pluginUUID = "1b7019d0-7237-11e9-b20c-f9f12eb6d835";
        }
        public GMinerPlugin(string pluginUUID = "1b7019d0-7237-11e9-b20c-f9f12eb6d835")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 7);

        public string Name => "GMinerCuda9.0+";

        public string Author => "stanko@nicehash.com";

        protected readonly Dictionary<string, int> _mappedCudaIds = new Dictionary<string, int>();

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new GMiner(PluginUUID, _mappedCudaIds)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
            };
        }


        // Supported algoritms:
        //   - Cuckaroo29/Cuckatoo31 (Grin)
        //   - Cuckoo29 (Aeternity)
        //   - Equihash 96,5 (MinexCoin)
        //   - Equihash 144,5 (Bitcoin Gold, BitcoinZ, SnowGem, SafeCoin, Litecoin Z) // ZHash
        //   - Equihash 150,5 (BEAM)
        //   - Equihash 192,7 (Zero, Genesis)
        //   - Equihash 210,9 (Aion)

        // Requirements:
        //   - CUDA compute compability 5.0+ #1
        //   - Cuckaroo29 ~ 5.6GB VRAM
        //   - Cuckatoo31 ~ 7.4GB VRAM
        //   - Cuckoo29 ~ 5.6GB VRAM
        //   - Equihash 96,5 ~0.75GB VRAM
        //   - Equihash 144,5 ~1.75GB VRAM
        //   - Equihash 150,5 ~2.9GB VRAM
        //   - Equihash 192,7 ~2.75GB VRAM
        //   - Equihash 210,9 ~1GB VRAM
        //   - CUDA 9.0+ 

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();
            foreach (var gpu in amdGpus)
            {
                var algorithms = GetAMDSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            //CUDA 9.0+: minimum drivers 384.xx
            var minDrivers = new Version(384, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            // we filter CUDA SM5.0+ and order them by PCIe IDs
            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Select(dev => (CUDADevice)dev)
                .OrderBy(dev => dev.PCIeBusID);
            var pcieId = 0; // GMiner takes CUDA devices by 
            foreach (var gpu in cudaGpus)
            {
                // naive method
                _mappedCudaIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetCUDASupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetCUDASupportedAlgorithms(CUDADevice gpu) {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.ZHash),
                new Algorithm(PluginUUID, AlgorithmType.Beam),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        IReadOnlyList<Algorithm> GetAMDSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.Beam),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        #region Internal Settings
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
                    ID = "gminer_pers",
                    LongName = "--pers",
                },
                /// <summary>
                /// enable/disable power efficiency calculator. Power efficiency calculator display of energy efficiency statistics of GPU in S/w, higher CPU load. Default value is '1' ('0' - off or '1' - on)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_pec",
                    LongName = "--pec",
                    DefaultValue = "1"
                },
                /// <summary>
                /// pass cost of electricity in USD per kWh, miner will report $ spent to mining
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "gminer_electricity",
                    LongName = "--electricity_cost"
                },
                /// <summary>
                /// option to control GPU intensity (--intensity, 1-100)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "gminer_intensity",
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
                    ID = "gminer_templimit",
                    ShortName = "-t",
                    LongName = "--templimit",
                    DefaultValue = "90",
                    Delimiter = " "
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };
        #endregion Internal Settings

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedCudaIds.Count == 0) return;
            // TODO will block
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--list_devices");
            var mappedDevs = DevicesListParser.ParseGMinerOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedCudaIds[uuid] = indexID;
            }
        }

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "miner.exe" });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            /*
            var benchmarkedVersionIsSame = Version.Major == benchmarkedPluginVersion.Major && Version.Minor == benchmarkedPluginVersion.Minor;
            var benchmarkedVersionIsOlder = Version.Major >= benchmarkedPluginVersion.Major && Version.Minor > benchmarkedPluginVersion.Minor;
            if (benchmarkedVersionIsSame || !benchmarkedVersionIsOlder) return false;
            if (ids.Count() == 0) return false;
            */

            return false;
        }

        public TimeSpan GetApiMaxTimeout()
        {
            return new TimeSpan(0, 2, 0);
        }
    }
}
