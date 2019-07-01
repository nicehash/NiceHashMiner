using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ClaymoreCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ClaymoreDual
{
    public class ClaymoreDualPlugin : IMinerPlugin, IInitInternals/*, IDevicesCrossReference*/, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeout
    {
        public ClaymoreDualPlugin()
        {
            _pluginUUID = "70984aa0-7236-11e9-b20c-f9f12eb6d835";
        }
        public ClaymoreDualPlugin(string pluginUUID = "70984aa0-7236-11e9-b20c-f9f12eb6d835")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 3);

        public string Name => "ClaymoreDual";

        public string Author => "domen.kirnkrefl@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            int claymoreIndex = -1;
            // AMD
            var amdGpus = devices
                .Where(dev => dev is AMDDevice gpu && !Checkers.IsGcn4(gpu))
                .Cast<AMDDevice>()
                .OrderBy(amd => amd.PCIeBusID);
            foreach (var gpu in amdGpus)
            {
                _mappedIDs[gpu.UUID] = ++claymoreIndex;
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            // CUDA
            var minDrivers = new Version(398, 26);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 3 && gpu.SM_major < 6)
                .Cast<CUDADevice>()
                .OrderBy(gpu => gpu.PCIeBusID); ;

            foreach (var gpu in cudaGpus)
            {
                _mappedIDs[gpu.UUID] = ++claymoreIndex;
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(IGpuDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto),
            // duals disabled by default
#pragma warning disable 0618
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak) {Enabled = false },
#pragma warning restore 0618
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        public IMiner CreateMiner()
        {
            return new ClaymoreDual(PluginUUID, _mappedIDs)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return MinerToolkit.IsSameAlgorithmType(a.Algorithm, b.Algorithm);
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
                /// Ethereum algorithm mode for AMD cards. 0 - optimized for fast cards, 1 - optimized for slow cards, 2 - for gpu-pro Linux drivers. -1 - autodetect (default, automatically selects between 0 and 1). 
	            /// You can also set this option for every card individually, for example "-etha 0,1,0".
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_eth_algorithm_mode_AMD",
                    ShortName = "-etha",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// Ethereum intensity. Default value is 8, you can decrease this value if you don't want Windows to freeze or if you have problems with stability. The most low GPU load is "-ethi 0".
	            ///Also "-ethi" can set intensity for every card individually, for example "-ethi 1,8,6".
                ///You can also specify negative values, for example, "-ethi -8192", it exactly means "global work size" parameter which is used in official miner.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_intensity_primary",
                    ShortName = "-ethi",
                    DefaultValue = "8",
                    Delimiter = ","
                },
                /// <summary>
                /// this setting can improve stability on multi-GPU systems if miner hangs during startup. It serializes GPUs initalization routines. Use "-gser 1" to serailize some of routines and "-gser 2" to serialize all routines. 
	            /// Using values higher than 2 allows you also to set custom delay between DAG generation on GPUs, for example, "-gser 5" means same as "-gser 2" and also adds 3sec delay between DAG generation (can be useful for buggy drivers and/or weak PSU).
                /// Default value is "0" (no serialization, fast initialization).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "claymoreDual_gpu_serialization",
                    ShortName = "-gser",
                    DefaultValue = "0",
                },
                /// <summary>
                /// Decred/Siacoin/Lbry/Pascal intensity, or Ethereum fine-tuning value in ETH-only ASM mode. Default value is 30, you can adjust this value to get the best Decred/Siacoin/Lbry mining speed without reducing Ethereum mining speed. 
	            ///You can also specify values for every card, for example "-dcri 30,100,50".
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_intensity_secondary",
                    ShortName = "-dcri",
                    DefaultValue = "30",
                    Delimiter = ","
                },
                /// <summary>
                /// low intensity mode. Reduces mining intensity, useful if your cards are overheated. Note that mining speed is reduced too. 
	            /// More value means less heat and mining speed, for example, "-li 10" is less heat and mining speed than "-li 1". You can also specify values for every card, for example "-li 3,10,50".
                /// Default value is "0" - no low intensity mode.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_lowIntensity",
                    ShortName = "-li",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// low intensity mode for DAG generation, it can help with OC or weak PSU. Supported values are 0, 1, 2, 3, more value means lower intensity. Example: "-lidag 1".
	            /// You can also specify values for every card, for example "-lidag 1,0,3". Default value is "0" (no low intensity for DAG generation).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_lowIntensity_dag",
                    ShortName = "-lidag",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// set "1" to cancel my developer fee at all. In this mode some optimizations are disabled so mining speed will be slower by about 3%. 
	            /// By enabling this mode, I will lose 100% of my earnings, you will lose only about 2% of your earnings.
                /// So you have a choice: "fastest miner" or "completely free miner but a bit slower".
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "claymoreDual_noFee",
                    ShortName = "-nofee",
                    DefaultValue = "0",
                },
                /// <summary>
                /// enables Compute Mode and disables CrossFire for AMD cards. "-y 1" works as pressing "y" key when miner starts. This option works in Windows only.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "claymoreDual_computeMode",
                    ShortName = "-y",
                    DefaultValue = "1",
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// set target GPU temperature. For example, "-tt 80" means 80C temperature. You can also specify values for every card, for example "-tt 70,80,75".
                /// You can also set static fan speed if you specify negative values, for example "-tt -50" sets 50% fan speed.Specify zero to disable control and hide GPU statistics.
                /// "-tt 1" (default) does not manage fans but shows GPU temperature and fan status every 30 seconds.Specify values 2..5 if it is too often.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_target_temperature",
                    ShortName = "-tt",
                    DefaultValue = "1",
                    Delimiter = ","
                },
                /// <summary>
                /// reduce Decred/Siacoin/Lbry/Pascal intensity automatically if GPU temperature is above specified value. For example, "-ttdcr 80" reduces Decred intensity if GPU temperature is above 80C. 
                /// You can see current Decred intensity coefficients in detailed statistics ("s" key). So if you set "-dcri 50" but Decred/Siacoin intensity coefficient is 20% it means that GPU currently mines Decred/Siacoin at "-dcri 10".
                /// You can also specify values for every card, for example "-ttdcr 80,85,80". You also should specify non-zero value for "-tt" option to enable this option.
                /// It is a good idea to set "-ttdcr" value higher than "-tt" value by 3-5C.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_target_temperature_dcr",
                    ShortName = "-ttdcr",
                    Delimiter = ","
                },
                /// <summary>
                /// reduce entire mining intensity (for all coins) automatically if GPU temperature is above specified value. For example, "-ttli 80" reduces mining intensity if GPU temperature is above 80C.
                /// You can see if intensity was reduced in detailed statistics ("s" key).
                /// You can also specify values for every card, for example "-ttli 80,85,80". You also should specify non-zero value for "-tt" option to enable this option.
                /// It is a good idea to set "-ttli" value higher than "-tt" value by 3-5C.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_target_temperature_lowerIntensity",
                    ShortName = "-ttli",
                    Delimiter = ","
                },
                /// <summary>
                /// set stop GPU temperature, miner will stop mining if GPU reaches specified temperature. For example, "-tstop 95" means 95C temperature. You can also specify values for every card, for example "-tstop 95,85,90".
                /// This feature is disabled by default ("-tstop 0"). You also should specify non-zero value for "-tt" option to enable this option.
                /// If it turned off wrong card, it will close miner in 30 seconds.
                /// You can also specify negative value to close miner immediately instead of stopping GPU, for example, "-tstop -95" will close miner as soon as any GPU reach 95C temperature.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_temperature_stop",
                    ShortName = "-tstop",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// set start temperature for overheated GPU that was previously stopped with "-tstop" option. For example, "-tstop 95 -tstart 50" disables GPU when it reaches 95C and re-enables it when it reaches 50C.
                /// You can also specify values for every card, for example "-tstart 50,55,50". Note that "-tstart" option value must be less than "-tstop" option value.
                /// This feature is disabled by default ("-tstart 0"). You also should specify non-zero value for "-tt" option to enable this option.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_temperature_start",
                    ShortName = "-tstart",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// set stop GPU temperature, miner will stop mining if GPU reaches specified temperature. For example, "-tstop 95" means 95C temperature. You can also specify values for every card, for example "-tstop 95,85,90".
                /// This feature is disabled by default ("-tstop 0"). You also should specify non-zero value for "-tt" option to enable this option.
                /// If it turned off wrong card, it will close miner in 30 seconds.
                /// You can also specify negative value to close miner immediately instead of stopping GPU, for example, "-tstop -95" will close miner as soon as any GPU reach 95C temperature.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_temperature_stop",
                    ShortName = "-tstop",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// set maximal fan speed, in percents, for example, "-fanmax 80" will set maximal fans speed to 80%. You can also specify values for every card, for example "-fanmax 50,60,70".
                /// This option works only if miner manages cooling, i.e. when "-tt" option is used to specify target temperature. Default value is "100".
                /// Note: for NVIDIA cards this option is supported in Windows only.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_fanMax",
                    ShortName = "-fanmax",
                    DefaultValue = "100",
                    Delimiter = ","
                },
                /// <summary>
                /// set minimal fan speed, in percents, for example, "-fanmin 50" will set minimal fans speed to 50%. You can also specify values for every card, for example "-fanmin 50,60,70".
                /// This option works only if miner manages cooling, i.e. when "-tt" option is used to specify target temperature. Default value is "0".
                /// Note: for NVIDIA cards this option is supported in Windows only.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_fanMin",
                    ShortName = "-fanmin",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// set target GPU core clock speed, in MHz. If not specified or zero, miner will not change current clock speed. You can also specify values for every card, for example "-cclock 1000,1050,1100,0".
                /// For NVIDIA you can also specify delta clock by using "+" and "-" prefix, for example, "-cclock +300,-400,+0".
                /// Note: for some drivers versions AMD blocked underclocking for some reason, you can overclock only.
                /// Note: this option changes clocks for all power states, so check voltage for all power states in WattMan or use -cvddc option.  
                /// By default, low power states have low voltage, setting high GPU clock for low power states without increasing voltage can cause driver crash.
                /// Note: for NVIDIA cards this option is supported in Windows only. 
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_core_clock",
                    ShortName = "-cclock",
                    Delimiter = ","
                },
                /// <summary>
                /// set target GPU memory clock speed, in MHz. If not specified or zero, miner will not change current clock speed. You can also specify values for every card, for example "-mclock 1200,1250,1200,0".
                /// For NVIDIA you can also specify delta clock by using "+" and "-" prefix, for example, "-cclock +300,-400,+0".
                /// Note: for some drivers versions AMD blocked underclocking for some reason, you can overclock only.
                /// Note: for NVIDIA cards this option is supported in Windows only.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_memory_clock",
                    ShortName = "-mclock",
                    Delimiter = ","
                },
                /// <summary>
                /// set power limit, usually from -50 to 50. For example, "-powlim -20" means 80% power limit. If not specified, miner will not change power limit. You can also specify values for every card, for example "-powlim 20,-20,0,10".
                /// Note: for NVIDIA cards this option is supported in Windows only.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "claymoreDual_power_limit",
                    ShortName = "-powlim",
                    Delimiter = ","
                }
            }
        };
        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };
        #endregion Internal Settings

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var minerCwd = miner.GetBinAndCwdPaths().Item2;
            // no device list so 'start mining'
            var logFile = "noappend_cross_ref_devs.txt";
            var logFilePath = Path.Combine(minerCwd, logFile);
            var args = $"-mport 0 -benchmark 1 -wd 0 -colors 0 -dbg 1 -logfile {logFile}";
            var output = await MinerPluginToolkitV1.ClaymoreCommon.DevicesCrossReferenceHelpers.ReadLinesUntil(minerBinPath, minerCwd, args, logFilePath, new List<string> { "Total cards", "Stratum - connecting to" });
            var mappedDevs = DevicesListParser.ParseClaymoreDualOutput(output, devices.ToList());

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "EthDcrMiner64.exe", "libcurl.dll", "msvcr110.dll",
                @"cuda10.0\cudart64_100.dll", @"cuda10.0\EthDcrMiner64.exe", @"cuda6.5\cudart64_65.dll", @"cuda6.5\EthDcrMiner64.exe", @"cuda7.5\cudart64_75.dll", @"cuda7.5\EthDcrMiner64.exe",
                @"Remote manager\EthMan.exe", @"Remote manager\libeay32.dll", @"Remote manager\ssleay32.dll"
            });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            // error/bug in v1.0
            // because the previous miner plugin mapped wrong GPU indexes rebench everything
            return false;
        }

        public TimeSpan GetApiMaxTimeout()
        {
            return new TimeSpan(0, 5, 0);
        }
    }
}
