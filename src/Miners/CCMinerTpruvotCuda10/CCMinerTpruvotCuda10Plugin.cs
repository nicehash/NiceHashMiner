using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.IO;
using MinerPluginToolkitV1.Configs;
using NiceHashMinerLegacy.Common;
using MinerPluginToolkitV1;

namespace CCMinerTpruvotCuda10
{
    public class CCMinerTpruvotCuda10Plugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker
    {
        public Version Version => new Version(1, 1);

        public string Name => "CCMinerTpruvotCuda10";

        public string Author => "stanko@nicehash.com";

        public string PluginUUID => "2257f160-7236-11e9-b20c-f9f12eb6d835";

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new CCMinerTpruvotCuda10Miner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // 410.48
            var minimumNVIDIADriver = new Version(410, 48);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minimumNVIDIADriver) return supported; // return emtpy
            
            // SM3.+ and CUDA 10 drivers
            var cudaGpus = devices.Where(dev => dev is CUDADevice cudaDev && cudaDev.SM_major > 3).Select(dev => (CUDADevice)dev);

            foreach (var gpu in cudaGpus)
            {
                supported.Add(gpu, GetSupportedAlgorithms());
            }

            cudaGpus = devices.Where(dev => dev is CUDADevice cudaDev && cudaDev.SM_major == 3).Select(dev => (CUDADevice)dev);
            foreach (var gpu in cudaGpus)
            {
                supported.Add(gpu, GetFilteredSupportedAlgorithms());
            }
            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms()
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.NeoScrypt),
                new Algorithm(PluginUUID, AlgorithmType.Keccak),
                new Algorithm(PluginUUID, AlgorithmType.Skunk),
                new Algorithm(PluginUUID, AlgorithmType.X16R),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
            };
        }

        IReadOnlyList<Algorithm> GetFilteredSupportedAlgorithms()
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.Keccak),
                new Algorithm(PluginUUID, AlgorithmType.Skunk),
                new Algorithm(PluginUUID, AlgorithmType.X16R),
            };
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

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU threads per call 8-25 (2^N + F, default: 0=auto). Decimals and multiple values are allowed for fine tuning
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ccminertpruvot_intensity",
                    ShortName = "-i",
                    LongName = "--intensity=",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// number of miner threads (default: number of nVidia GPUs in your system)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_threads",
                    ShortName = "-t",
                    LongName = "--threads=",
                }, 
                /// <summary>
                /// Set device threads scheduling mode (default: auto)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ccminertpruvot_cuda_schedule",
                    ShortName = "--cuda-schedule",
                },
                /// <summary>
                /// set process priority (default: 0 idle, 2 normal to 5 highest)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "0",
                },
                /// <summary>
                /// set process affinity to specific cpu core(s) mask
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_affinity",
                    ShortName = "--cpu-affinity",
                },
                /// <summary>
                /// will force the Geforce 9xx to run in P0 P-State
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_pstate",
                    ShortName = "--pstate=",
                    DefaultValue = "0",
                },
                /// <summary>
                /// set the gpu power limit, allow multiple values for N cards. On windows this parameter use percentages (like OC tools)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_plimit",
                    ShortName = "--plimit=",
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// Only mine if gpu temp is less than specified value
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_max_temp",
                    ShortName = "--max-temp=",
                },
                /// <summary>
                /// Set the gpu thermal limit (windows only)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_tlimit",
                    ShortName = "--tlimit=",
                    DefaultValue = "85"
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        #endregion Internal Settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "ccminer.exe", "msvcr120.dll" });
        }
    }
}
