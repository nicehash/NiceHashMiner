using MinerPlugin;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MinerPluginToolkitV1
{
    using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;
    // TODO add documentation
    public abstract class PluginBase : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2, IMinerBinsSource, IBinAndCwdPathsGettter, IGetMinerBinaryVersion, IGetPluginMetaInfo, IPluginSupportedAlgorithmsSettings
    {
        protected abstract MinerBase CreateMinerBase();

        #region IMinerPlugin
        public abstract Version Version { get; }
        public abstract string Name { get; }
        public abstract string Author { get; }
        public abstract string PluginUUID { get; }

        public virtual bool CanGroup(MiningPair a, MiningPair b)
        {
            var checkELPCompatibility = MinerOptionsPackage?.GroupMiningPairsOnlyWithCompatibleOptions ?? false;
            var isSameAlgoType = MinerToolkit.IsSameAlgorithmType(a.Algorithm, b.Algorithm);
            if (isSameAlgoType && checkELPCompatibility)
            {
                var ignoreDefaults = MinerOptionsPackage.IgnoreDefaultValueOptions;
                var areGeneralOptionsCompatible = ExtraLaunchParametersParser.CheckIfCanGroup(a, b, MinerOptionsPackage.GeneralOptions, ignoreDefaults);
                var areTemperatureOptionsCompatible = ExtraLaunchParametersParser.CheckIfCanGroup(a, b, MinerOptionsPackage.TemperatureOptions, ignoreDefaults);
                return areGeneralOptionsCompatible && areTemperatureOptionsCompatible;
            }

            return isSameAlgoType;
        }


        public virtual IMiner CreateMiner()
        {
            var miner = CreateMinerBase();
            miner.BinAndCwdPathsGettter = this; // set the paths interface
            // set internal settings
            if (MinerOptionsPackage != null) miner.MinerOptionsPackage = MinerOptionsPackage;
            if (MinerSystemEnvironmentVariables != null) miner.MinerSystemEnvironmentVariables = MinerSystemEnvironmentVariables;
            if (MinerReservedApiPorts != null) miner.MinerReservedApiPorts = MinerReservedApiPorts;
            if (MinerBenchmarkTimeSettings != null) miner.MinerBenchmarkTimeSettings = MinerBenchmarkTimeSettings;
            return miner;
        }

        #endregion IMinerPlugin

        public abstract Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices);


        protected PluginMetaInfo PluginMetaInfo { get; set; } = null;

        #region IInitInternals
        public virtual void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitInternalSetting(pluginRoot, MinerSystemEnvironmentVariables, "MinerSystemEnvironmentVariables.json");
            if (readFromFileEnvSysVars != null) MinerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalSetting(pluginRoot, MinerOptionsPackage, "MinerOptionsPackage.json");
            if (fileMinerOptionsPackage != null) MinerOptionsPackage = fileMinerOptionsPackage;

            var fileMinerReservedPorts = InternalConfigs.InitInternalSetting(pluginRoot, MinerReservedApiPorts, "MinerReservedPorts.json");
            if (fileMinerReservedPorts != null) MinerReservedApiPorts = fileMinerReservedPorts;

            var fileMinerApiMaxTimeoutSetting = InternalConfigs.InitInternalSetting(pluginRoot, GetApiMaxTimeoutConfig, "MinerApiMaxTimeoutSetting.json");
            if (fileMinerApiMaxTimeoutSetting != null) GetApiMaxTimeoutConfig = fileMinerApiMaxTimeoutSetting;

            var fileMinerBenchmarkTimeSettings = InternalConfigs.InitInternalSetting(pluginRoot, MinerBenchmarkTimeSettings, "MinerBenchmarkTimeSettings.json");
            if (fileMinerBenchmarkTimeSettings != null) MinerBenchmarkTimeSettings = fileMinerBenchmarkTimeSettings;

            var fileMinersBinsUrlsSettings = InternalConfigs.InitInternalSetting(pluginRoot, MinersBinsUrlsSettings, "MinersBinsUrlsSettings.json");
            if (fileMinersBinsUrlsSettings != null) MinersBinsUrlsSettings = fileMinersBinsUrlsSettings;

            var filePluginSupportedAlgorithmsSettings = InternalConfigs.InitInternalSetting(pluginRoot, PluginSupportedAlgorithmsSettings, "PluginSupportedAlgorithmsSettings.json");
            if (filePluginSupportedAlgorithmsSettings != null) PluginSupportedAlgorithmsSettings = filePluginSupportedAlgorithmsSettings;
        }

        // internal settings
        protected MinerOptionsPackage MinerOptionsPackage { get; set; } = new MinerOptionsPackage { };
        protected MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables { get; set; } = new MinerSystemEnvironmentVariables{};
        protected MinerReservedPorts MinerReservedApiPorts { get; set; } = new MinerReservedPorts {};
        protected MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new MinerApiMaxTimeoutSetting { GeneralTimeout = new TimeSpan(0, 5, 0) };
        protected MinerBenchmarkTimeSettings MinerBenchmarkTimeSettings { get; set; } = new MinerBenchmarkTimeSettings { };

        protected MinersBinsUrlsSettings MinersBinsUrlsSettings { get; set; } = new MinersBinsUrlsSettings { };

        public PluginSupportedAlgorithmsSettings PluginSupportedAlgorithmsSettings { get; set; } = new PluginSupportedAlgorithmsSettings();

        #endregion IInitInternals

        #region IReBenchmarkChecker
        public abstract bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids);
        #endregion IReBenchmarkChecker

        #region IGetApiMaxTimeoutV2
        public virtual bool IsGetApiMaxTimeoutEnabled => MinerApiMaxTimeoutSetting.ParseIsEnabled(true, GetApiMaxTimeoutConfig);


        protected TimeSpan DefaultTimeout { get; set; } = new TimeSpan(0, 5, 0);
        public virtual TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            return MinerApiMaxTimeoutSetting.ParseMaxTimeout(DefaultTimeout, GetApiMaxTimeoutConfig, miningPairs);
        }
        #endregion IGetApiMaxTimeoutV2

        #region IBinaryPackageMissingFilesChecker
        public abstract IEnumerable<string> CheckBinaryPackageMissingFiles();
        #endregion IBinaryPackageMissingFilesChecker

        #region IMinerBinsSource
        public virtual IEnumerable<string> GetMinerBinsUrlsForPlugin()
        {
            if (MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.Urls == null) return Enumerable.Empty<string>();
            return MinersBinsUrlsSettings.Urls;
        }
        #endregion IMinerBinsSource

        #region IBinAndCwdPathsGettter
        public virtual Tuple<string, string> GetBinAndCwdPaths()
        {
            if (MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.ExePath == null || MinersBinsUrlsSettings.ExePath.Count == 0)
            {
                throw new Exception("Unable to return cwd and exe paths MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.Path == null || MinersBinsUrlsSettings.Path.Count == 0");
            }
            var paths = new List<string>{ Paths.MinerPluginsPath(), PluginUUID, "bins" };
            paths.AddRange(MinersBinsUrlsSettings.ExePath);
            var binCwd = Path.Combine(paths.GetRange(0, paths.Count - 1).ToArray());
            var binPath = Path.Combine(paths.ToArray());
            return Tuple.Create(binPath, binCwd);
        }
        #endregion IBinAndCwdPathsGettter

        #region IGetMinerBinaryVersion
        public string GetMinerBinaryVersion()
        {
            if (MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.BinVersion == null)
            {
                // return this or throw???
                return "N/A";
            }
            return MinersBinsUrlsSettings.BinVersion;
        }
        #endregion IGetMinerBinaryVersion

        #region IGetPluginMetaInfo
        public PluginMetaInfo GetPluginMetaInfo()
        {
            return PluginMetaInfo;
        }
        #endregion IGetPluginMetaInfo

        #region IPluginSupportedAlgorithmsSettings
        public virtual bool UnsafeLimits()
        {
            return PluginSupportedAlgorithmsSettings.EnableUnsafeRAMLimits;
        }

        public virtual Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithmsDict()
        {
            DeviceType[] deviceTypes = new DeviceType[] { DeviceType.CPU, DeviceType.AMD, DeviceType.NVIDIA };
            var ret = new Dictionary<DeviceType, List<AlgorithmType>> { };
            foreach (var deviceType in deviceTypes)
            {
                var algos = GetSupportedAlgorithmsForDeviceType(deviceType);
                if (algos == null || algos.Count == 0) continue;
                ret[deviceType] = algos.SelectMany(a => a.IDs).ToList();
            }
            return ret;
        }

        public virtual List<Algorithm> GetSupportedAlgorithmsForDeviceType(DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.CPU:
                    return PluginSupportedAlgorithmsSettings.CPU_Algorithms?.Select(sas => sas.ToAlgorithm(PluginUUID)).ToList();
                case DeviceType.AMD:
                    return PluginSupportedAlgorithmsSettings.AMD_Algorithms?.Select(sas => sas.ToAlgorithm(PluginUUID)).ToList();
                case DeviceType.NVIDIA:
                    return PluginSupportedAlgorithmsSettings.NVIDIA_Algorithms?.Select(sas => sas.ToAlgorithm(PluginUUID)).ToList();
            }
            return null;
        }

        public virtual string AlgorithmName(params AlgorithmType[] algorithmTypes)
        {
            if (algorithmTypes.Length == 1)
            {
                var id = algorithmTypes[0];
                if (PluginSupportedAlgorithmsSettings.AlgorithmNames != null && PluginSupportedAlgorithmsSettings.AlgorithmNames.ContainsKey(id))
                {
                    return PluginSupportedAlgorithmsSettings.AlgorithmNames[id];
                }
            }
            // TODO throw 
            return "";
        }

        public virtual double DevFee(params AlgorithmType[] algorithmTypes)
        {
            if (algorithmTypes.Length == 1)
            {
                var id = algorithmTypes[0];
                if (PluginSupportedAlgorithmsSettings.AlgorithmFees != null && PluginSupportedAlgorithmsSettings.AlgorithmFees.ContainsKey(id))
                {
                    return PluginSupportedAlgorithmsSettings.AlgorithmFees[id];
                }
            }
            return PluginSupportedAlgorithmsSettings.DefaultFee;
        }
        #endregion IPluginSupportedAlgorithmsSettings

        protected Dictionary<AlgorithmType, ulong> GetCustomMinimumMemoryPerAlgorithm(DeviceType deviceType)
        {
            List<SAS> sass = null;
            var ret = new Dictionary<AlgorithmType, ulong>();
            switch (deviceType)
            {
                case DeviceType.CPU:
                    sass = PluginSupportedAlgorithmsSettings.CPU_Algorithms;
                    break;
                case DeviceType.AMD:
                    sass = PluginSupportedAlgorithmsSettings.AMD_Algorithms;
                    break;
                case DeviceType.NVIDIA:
                    sass = PluginSupportedAlgorithmsSettings.NVIDIA_Algorithms;
                    break;
            }
            if (sass != null)
            {
                var customRAMLimits = sass.Where(sas => sas.NonDefaultRAMLimit.HasValue);
                foreach (var el in customRAMLimits)
                {
                    ret[el.IDs.First()] = el.NonDefaultRAMLimit.Value;
                }
            }
            return ret;
        }
    }
}
