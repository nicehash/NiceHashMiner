using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Configs;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using InternalConfigsCommon = NHM.Common.Configs.InternalConfigs;

namespace NHM.MinerPluginToolkitV1
{
    // TODO add documentation
    public abstract class PluginBase : IMinerPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2, IMinerBinsSource, IBinAndCwdPathsGettter, IGetMinerBinaryVersion, IGetPluginMetaInfo, IPluginSupportedAlgorithmsSettings, IGetMinerOptionsPackage, IGetBinsPackagePassword
    {
        public static bool IS_CALLED_FROM_PACKER { get; set; } = false;
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
            miner.PluginSupportedAlgorithms = this; // dev fee, algo names
            // set internal settings
            if (MinerOptionsPackage != null) miner.MinerOptionsPackage = MinerOptionsPackage;
            if (MinerSystemEnvironmentVariables != null) miner.MinerSystemEnvironmentVariables = MinerSystemEnvironmentVariables;
            if (MinerReservedApiPorts != null) miner.MinerReservedApiPorts = MinerReservedApiPorts;
            if (MinerBenchmarkTimeSettings != null) miner.MinerBenchmarkTimeSettings = MinerBenchmarkTimeSettings;
            if (MinerCustomActionSettings != null) miner.MinerCustomActionSettings = MinerCustomActionSettings;
            return miner;
        }

        #endregion IMinerPlugin

        public abstract Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices);

        protected PluginMetaInfo PluginMetaInfo { get; set; } = null;

        #region IInitInternals
        public virtual void InitInternals()
        {
            (MinerSystemEnvironmentVariables, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "MinerSystemEnvironmentVariables.json"), MinerSystemEnvironmentVariables);
            (MinerOptionsPackage, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "MinerOptionsPackage.json"), MinerOptionsPackage);
            (MinerReservedApiPorts, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "MinerReservedPorts.json"), MinerReservedApiPorts);
            (GetApiMaxTimeoutConfig, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "MinerApiMaxTimeoutSetting.json"), GetApiMaxTimeoutConfig);
            (MinerBenchmarkTimeSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "MinerBenchmarkTimeSettings.json"), MinerBenchmarkTimeSettings);
            (MinersBinsUrlsSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "MinersBinsUrlsSettings.json"), MinersBinsUrlsSettings);
            (PluginSupportedAlgorithmsSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "PluginSupportedAlgorithmsSettings.json"), PluginSupportedAlgorithmsSettings);
            (MinerCustomActionSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "MinerCustomActionSettings.json"), MinerCustomActionSettings);
        }

        // internal settings
        protected MinerOptionsPackage MinerOptionsPackage { get; set; } = new MinerOptionsPackage { };
        protected MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables { get; set; } = new MinerSystemEnvironmentVariables { };
        protected MinerReservedPorts MinerReservedApiPorts { get; set; } = new MinerReservedPorts { };
        protected MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new MinerApiMaxTimeoutSetting { GeneralTimeout = new TimeSpan(0, 5, 0) };
        protected MinerBenchmarkTimeSettings MinerBenchmarkTimeSettings { get; set; } = new MinerBenchmarkTimeSettings { };

        protected MinersBinsUrlsSettings MinersBinsUrlsSettings { get; set; } = new MinersBinsUrlsSettings { };

        protected MinerCustomActionSettings MinerCustomActionSettings { get; set; } = new MinerCustomActionSettings { };

        public PluginSupportedAlgorithmsSettings PluginSupportedAlgorithmsSettings { get; set; } = new PluginSupportedAlgorithmsSettings();

        // we must define this for every miner plugin
        protected abstract PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings { get; }

        protected void InitInsideConstuctorPluginSupportedAlgorithmsSettings()
        {
            PluginSupportedAlgorithmsSettings = DefaultPluginSupportedAlgorithmsSettings;
            if (IS_CALLED_FROM_PACKER) return;
            (PluginSupportedAlgorithmsSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "internals", "PluginSupportedAlgorithmsSettings.json"), DefaultPluginSupportedAlgorithmsSettings);
        }

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
        public virtual (string binPath, string cwdPath) GetBinAndCwdPaths()
        {
            if (MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.ExePath == null || MinersBinsUrlsSettings.ExePath.Count == 0)
            {
                throw new Exception("Unable to return cwd and exe paths MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.Path == null || MinersBinsUrlsSettings.Path.Count == 0");
            }
            var paths = new List<string> { Paths.MinerPluginsPath(PluginUUID, "bins", $"{Version.Major}.{Version.Minor}" ) };
            paths.AddRange(MinersBinsUrlsSettings.ExePath);
            var binCwd = Path.Combine(paths.GetRange(0, paths.Count - 1).ToArray());
            var binPath = Path.Combine(paths.ToArray());
            return (binPath, binCwd);
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

        #region IGetMinerOptionsPackage
        MinerOptionsPackage IGetMinerOptionsPackage.GetMinerOptionsPackage() => MinerOptionsPackage;
        #endregion IGetMinerOptionsPackage
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
                if (algos.Count == 0) continue;
                ret[deviceType] = new HashSet<AlgorithmType>(algos.SelectMany(a => a.IDs)).ToList();
            }
            return ret;
        }

        public virtual List<Algorithm> GetSupportedAlgorithmsForDeviceType(DeviceType deviceType)
        {
            if (PluginSupportedAlgorithmsSettings.Algorithms?.ContainsKey(deviceType) ?? false)
            {
                var sass = PluginSupportedAlgorithmsSettings.Algorithms[deviceType];
                return sass.Select(sas => sas.ToAlgorithm(PluginUUID)).ToList();
            }
            return new List<Algorithm>(); // return empty
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
            return "";
        }

        public virtual double DevFee(params AlgorithmType[] algorithmTypes)
        {
            if (algorithmTypes.Length == 1)
            {
                var id = algorithmTypes[0];
                if (PluginSupportedAlgorithmsSettings.AlgorithmFees?.ContainsKey(id) ?? false)
                {
                    return PluginSupportedAlgorithmsSettings.AlgorithmFees[id];
                }
            }
            return PluginSupportedAlgorithmsSettings.DefaultFee;
        }
        #endregion IPluginSupportedAlgorithmsSettings

        protected Dictionary<AlgorithmType, ulong> GetCustomMinimumMemoryPerAlgorithm(DeviceType deviceType)
        {
            var ret = new Dictionary<AlgorithmType, ulong>();
            if (PluginSupportedAlgorithmsSettings.Algorithms?.ContainsKey(deviceType) ?? false)
            {
                var sass = PluginSupportedAlgorithmsSettings.Algorithms[deviceType];
                var customRAMLimits = sass.Where(sas => sas.NonDefaultRAMLimit.HasValue);
                foreach (var el in customRAMLimits)
                {
                    ret[el.IDs.First()] = el.NonDefaultRAMLimit.Value;
                }
            }
            return ret;
        }

        public IReadOnlyList<Algorithm> GetSupportedAlgorithmsForDevice(BaseDevice dev)
        {
            var deviceType = dev.DeviceType;
            var algorithms = GetSupportedAlgorithmsForDeviceType(deviceType);
            var gpu = dev as IGpuDevice;
            if (UnsafeLimits() || gpu == null) return algorithms;
            // GPU RAM filtering
            var ramLimits = GetCustomMinimumMemoryPerAlgorithm(deviceType);
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsListCustom(gpu.GpuRam, algorithms, ramLimits);
            return filteredAlgorithms;
        }

        public virtual string BinsPackagePassword
        {
            get
            {
                try
                {
                    return MinersBinsUrlsSettings?.BinsPackagePassword ?? null;
                }
                catch { }
                return null;
            }
        }
    }
}
