using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NanoMiner
{
    public class NanoMinerPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IDevicesCrossReference, IGetApiMaxTimeoutV2
    {
        public NanoMinerPlugin()
        {
            _pluginUUID = "fa2f3530-67ff-11e9-b04e-b5d540d02534";
        }
        public NanoMinerPlugin(string pluginUUID = "fa2f3530-67ff-11e9-b04e-b5d540d02534")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 4);

        public string Name => "NanoMiner";

        public string Author => "domen.kirnkrefl@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
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
                if (gpu is AMDDevice amd)
                {
                    var algorithms = GetAMDSupportedAlgorithms(amd).ToList();
                    if (algorithms.Count > 0) supported.Add(amd, algorithms);
                }
                if (gpu is CUDADevice cuda)
                {
                    var algorithms = GetNvidiaSupportedAlgorithms(cuda).ToList();
                    if (algorithms.Count > 0) supported.Add(cuda, algorithms);
                }
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

        IReadOnlyList<Algorithm> GetAMDSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightR),
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        private IEnumerable<Algorithm> GetNvidiaSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightR),
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        public IMiner CreateMiner()
        {
            return new NanoMiner(PluginUUID, _mappedIDs)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts,
                MinerBenchmarkTimeSettings = _minerBenchmarkTimeSettings
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

            var fileMinerReservedPorts = InternalConfigs.InitMinerReservedPorts(pluginRoot, _minerReservedApiPorts);
            if (fileMinerReservedPorts != null) _minerReservedApiPorts = fileMinerReservedPorts;

            var fileMinerApiMaxTimeoutSetting = InternalConfigs.InitMinerApiMaxTimeoutSetting(pluginRoot, _getApiMaxTimeoutConfig);
            if (fileMinerApiMaxTimeoutSetting != null) _getApiMaxTimeoutConfig = fileMinerApiMaxTimeoutSetting;

            var fileMinerBenchmarkTimeSetting = InternalConfigs.InitMinerBenchmarkTimeSettings(pluginRoot, _minerBenchmarkTimeSettings);
            if (fileMinerBenchmarkTimeSetting != null) _minerBenchmarkTimeSettings = fileMinerBenchmarkTimeSetting;
        }

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// This is the minimum acceptable hashrate. This function keeps track of the rig’s total hashrate and compares it with this parameter.
                /// If five minutes after the miner is launched the set minimum is not reached, nanominer will automatically restart.
                /// Likewise, the miner will restart if for any reason the average hashrate over a ten-minute period falls below the set value.
                /// This value can be set with an optional modifier letter that represents a thousand for kilohash or a million for megahash per second.
                /// For example, setting the value to 100 megahashes per second can be written as 100M, 100.0M, 100m, 100000k, 100000K or 100000000.
                /// If this parameter is not defined, the miner will not restart.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nanominer_minHash",
                    ShortName = "minHashrate="
                },
                /// <summary>
                /// This parameter accepts the values true or false (the default is false). If this parameter is set to true then no log files will be recorded onto the hard drive.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nanominer_noLog",
                    ShortName = "noLog=",
                    DefaultValue = "false"
                },
                /// <summary>
                /// This parameter can either be used to set the name of the folder in which log files will be created (e.g. logPath=logfolder/),
                /// or to specify a path to single file, which will be used for all logs (e.g. logPath=logs/log.txt, logPath=/var/log/nanominer/log.txt, logPath=C:\logs\log.txt).
                /// Both relative and absolute paths work. Default value for this parameter is logs/.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "nanominer_logPath",
                    ShortName = "logPath=",
                    DefaultValue = "logs/"
                },
                /// <summary>
                /// Can be set to modify AMD GPU timings on the fly for Ethash algorithm.
                /// The following AMD ASICs are currently supported: gfx900, gfx901, gfx906, gfx907, Baffin, Ellesmere, gfx804, Hawaii, Tahiti, Pitcairn, Tonga.
                /// Default memory tweak value is 1 which means slightly improving memory timings. Zero value means timings are left as is without modifications.
                /// Parameter values must be separated by a comma or space (first value is for GPU0, second is for GPU1, and so on).
                /// Supported memory tweak value range is from 0 to 10 (0 means disabling timings modification, 1 is the least intense, 10 is the most intense).
                /// You can also apply same settings for each GPU by defining only one memory tweak value.
                /// Miner must be launched using admin/root privileges in order to change timings.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nanominer_memTweak",
                    ShortName = "memTweak=",
                    DefaultValue = "1",
                    Delimiter = ","
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// Can be used to overclock/underclock NVIDIA GPU’s. Absolute (e.g. 4200) as well as relative (e.g. +200, -150) values in megabytes are accepted.
                /// The values must be separated by a comma or space (first value is for GPU0, second is for GPU1, and so on). For example, if it is set as
                /// coreClocks=+200,-150
                /// memClocks = +300,3900
                /// then GPU0 will be overclocked by 200 MHz of core and 300 MHz of memory, whereas GPU1 core clock will be underclocked by 150 MHz, and its memory clock set to 3900 MHz.
                /// You can also apply same settings for each GPU by defining only one of the core and memory clock values, for example:
                /// coreClocks=+200
                /// memClocks = +300
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nanominer_coreClocks",
                    ShortName = "coreClocks=",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "nanominer_memClocks",
                    ShortName = "memClocks=",
                    Delimiter = ","
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };
        protected static MinerApiMaxTimeoutSetting _getApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout = _defaultTimeout,
        };
        protected static MinerBenchmarkTimeSettings _minerBenchmarkTimeSettings = new MinerBenchmarkTimeSettings { };
        #endregion Internal Settings

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedIDs.Count == 0) return;
            // TODO will break
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;

            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "-d");
            var mappedDevs = DevicesListParser.ParseNanoMinerOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedIDs[uuid] = indexID;
            }
        }

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nvrtc64_100_0.dll", "nvrtc-builtins64_100.dll", "service.dll", "nanominer.exe" });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }

        #region IGetApiMaxTimeoutV2
        public bool IsGetApiMaxTimeoutEnabled => MinerApiMaxTimeoutSetting.ParseIsEnabled(true, _getApiMaxTimeoutConfig);

        protected static TimeSpan _defaultTimeout = new TimeSpan(0, 5, 0);
        public TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            return MinerApiMaxTimeoutSetting.ParseMaxTimeout(_defaultTimeout, _getApiMaxTimeoutConfig, miningPairs);
        }
        #endregion IGetApiMaxTimeoutV2
    }
}
