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

namespace TRex
{
    public class TRexPlugin : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2
    {
        public TRexPlugin()
        {
            _pluginUUID = "d47d9b00-7237-11e9-b20c-f9f12eb6d835";
        }
        public TRexPlugin(string pluginUUID = "d47d9b00-7237-11e9-b20c-f9f12eb6d835")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 6);

        public string Name => "TRex";

        public string Author => "domen.kirnkrefl@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 3).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            yield return new Algorithm(PluginUUID, AlgorithmType.Lyra2Z);
            yield return new Algorithm(PluginUUID, AlgorithmType.X16R);
            yield return new Algorithm(PluginUUID, AlgorithmType.MTP) { Enabled = false };
        }

        public IMiner CreateMiner()
        {
            return new TRex(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables,
                MinerReservedApiPorts = _minerReservedApiPorts
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
        }

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage{
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU intensity 8-25 (default: auto).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    DefaultValue = "auto"
                },
                /// <summary>
                /// Set process priority (default: 2) 0 idle, 2 normal to 5 highest.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "2"
                },
                /// <summary>
                /// Forces miner to immediately reconnect to pool on N successively failed shares (default: 10).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_reconectFailed",
                    ShortName = "--reconnect-on-fail-shares",
                    DefaultValue = "10"
                },
                /// <summary>
                /// Sliding window length in seconds used to compute average hashrate (default: 60).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_avgHashrate",
                    ShortName = "--hashrate-avr",
                    DefaultValue = "60"
                },
                /// <summary>
                /// Sliding window length in seconds used to compute sharerate (default: 600).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_avgSharerate",
                    ShortName = "--sharerate-avr",
                    DefaultValue = "600"
                },
                /// <summary>
                /// Set temperature color for GPUs stat. Example: 55,65 - it means that
                /// temperatures above 55 will have yellow color, above 65 - red color. (default: 67,77)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_tempColor",
                    ShortName = "--temperature-color",
                    DefaultValue = "67,77"
                },
                /// <summary>
                /// GPU stats report frequency. (default: 5. every 5th share)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_reportInterval",
                    ShortName = "--gpu-report-interval",
                    DefaultValue = "5"
                },
                /// <summary>
                /// Quiet mode. No GPU stats at all.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_quiet",
                    ShortName = "--quiet"
                },
                /// <summary>
                /// Don't show date in console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_hideDate",
                    ShortName = "--hide-date"
                },
                /// <summary>
                /// Disable color output for console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_noColor",
                    ShortName = "--no-color"
                },
                /// <summary>
                /// Disable NVML GPU stats.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_noNvml",
                    ShortName = "--no-nvml"
                },
                /// <summary>
                /// Full path of the log file.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_logPath",
                    ShortName = "--log-path"
                },
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU shutdown temperature. (default: 0 - disabled)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_tempLimit",
                    LongName = "--temperature-limit",
                    DefaultValue = "0"
                },
                /// <summary>
                /// GPU temperature to enable card after disable. (default: 0 - disabled)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_tempStart",
                    LongName = "--temperature-start",
                    DefaultValue = "0"
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        protected static MinerReservedPorts _minerReservedApiPorts = new MinerReservedPorts { };
        protected static MinerApiMaxTimeoutSetting _getApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout =  _defaultTimeout,
        };
        #endregion Internal Settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "msvcr71.dll", "t-rex.exe" });
        }

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }

        #region IGetApiMaxTimeoutV2
        public bool IsGetApiMaxTimeoutEnabled => MinerApiMaxTimeoutSetting.ParseIsEnabled(true, _getApiMaxTimeoutConfig);

        protected static TimeSpan _defaultTimeout = new TimeSpan(0, 1, 0);
        public TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            return MinerApiMaxTimeoutSetting.ParseMaxTimeout(_defaultTimeout, _getApiMaxTimeoutConfig, miningPairs);
        }
        #endregion IGetApiMaxTimeoutV2
    }
}
