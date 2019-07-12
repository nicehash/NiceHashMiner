using MinerPlugin;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using MinerPluginToolkitV1.Interfaces;
using System.IO;
using NHM.Common;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;

namespace CPUMinerBase
{
    public class CPUMinerPlugin : IMinerPlugin, IInitInternals, IReBenchmarkChecker, IGetApiMaxTimeout
    {
        public string PluginUUID => "92fceb00-7236-11e9-b20c-f9f12eb6d835";

        public Version Version => new Version(1, 2);

        public string Name => "cpuminer-opt";

        public string Author => "stanko@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cpus = devices.Where(dev => dev is CPUDevice).Select(dev => (CPUDevice)dev);
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var cpu in cpus)
            {
                supported.Add(cpu, GetSupportedAlgorithms());
            }

            return supported;
        }

        public IMiner CreateMiner() => new CpuMiner(PluginUUID)
        {
            MinerOptionsPackage = _minerOptionsPackage,
            MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
        };

        
        // TODO check get what kind of benchmark it is, local or network

        // TODO reserved miner API port
        // TODO miner connection type add ssl disable ssl, has SSL, when to use SSL does it make sense to use it? 
        // TODO add is online or offline benchmark
        // TODO DevFee does it have one what kind of a fee is there, does it differ from algo or ssl enabled
        // extra launch parameters thingy should be taken care of per miner

        public bool CanGroup(MiningPair a, MiningPair b) 
        {
            return false;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms()
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.Lyra2Z)
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
                /// number of miner threads (default: number of processors)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "cpuminer_threads",
                    ShortName = "-t",
                    LongName = "--threads=",
                },
                /// <summary>
                /// set process priority (default: 0 idle, 2 normal to 5 highest)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "cpuminer_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "0"
                },
                //TODO WARNING this functionality can overlap with already implemented one!!!
                /// <summary>
                /// set process affinity to cpu core(s), mask 0x3 for cores 0 and 1
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "cpuminer_affinity",
                    ShortName = "--cpu-affinity",
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        #endregion Internal Settings

        public bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }

        public TimeSpan GetApiMaxTimeout()
        {
            return new TimeSpan(1, 5, 0);
        }
    }
}
