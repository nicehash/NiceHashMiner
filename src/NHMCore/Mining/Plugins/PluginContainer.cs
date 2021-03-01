using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.MinerPluginToolkitV1.Interfaces;
using NHMCore.Switching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NHMCore.Mining.Plugins
{
    // interfaces were used only to implement the container methods
    public class PluginContainer /*: IMinerPlugin , IGetApiMaxTimeout, IDevicesCrossReference, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IInitInternals*/
    {
        private static List<PluginContainer> _pluginContainers = new List<PluginContainer>();
        private static List<PluginContainer> _brokenPluginContainers = new List<PluginContainer>();
        internal static object _lock = new object();
        internal static IReadOnlyList<PluginContainer> PluginContainers
        {
            get
            {
                lock (_lock)
                {
                    return _pluginContainers.ToList();
                }
            }
        }
        internal static IReadOnlyList<PluginContainer> BrokenPluginContainers
        {
            get
            {
                lock (_lock)
                {
                    return _brokenPluginContainers.ToList();
                }
            }
        }

        public static void AddPluginContainer(PluginContainer newPlugin)
        {
            lock (_lock)
            {
                _pluginContainers.Add(newPlugin);
            }
        }

        public static void RemovePluginContainer(PluginContainer removePlugin)
        {
            lock (_lock)
            {
                _pluginContainers.Remove(removePlugin);
                removePlugin.RemoveAlgorithmsFromDevices();
            }
        }

        private static void SetAsBroken(PluginContainer brokenPlugin)
        {
            lock (_lock)
            {
                _pluginContainers.Remove(brokenPlugin);
                _brokenPluginContainers.Add(brokenPlugin);
                brokenPlugin.RemoveAlgorithmsFromDevices();
            }
        }

        internal static PluginContainer Create(IMinerPlugin plugin)
        {
            var newPlugin = new PluginContainer(plugin);
            AddPluginContainer(newPlugin);
            return newPlugin;
        }

        private PluginContainer(IMinerPlugin plugin)
        {
            _plugin = plugin;
        }
        private IMinerPlugin _plugin = null;

        private string _logTag = null;
        private string LogTag
        {
            get
            {
                if (_logTag == null)
                {
                    string name = "unknown";
                    string uuid = "unknown";
                    try
                    {
                        name = _plugin.Name;
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        uuid = _plugin.PluginUUID;
                    }
                    catch (Exception)
                    {
                    }
                    _logTag = $"PluginContainer-UUID({uuid})-Name({name})";
                }
                return _logTag;
            }
        }

        public bool Enabled
        {
            get
            {
                return IsCompatible && !IsBroken && IsInitialized;
            }
        }
        public bool IsBroken { get; private set; } = false;
        public bool IsCompatible { get; private set; } = false;
        public bool IsInitialized { get; private set; } = false;
        // algos from and for the plugin
        private Dictionary<BaseDevice, IReadOnlyList<Algorithm>> _cachedAlgorithms { get; } = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
        // algos for NiceHashMiner Client
        private Dictionary<string, List<AlgorithmContainer>> _cachedNiceHashMinerAlgorithms { get; } = new Dictionary<string, List<AlgorithmContainer>>();

        public bool InitPluginContainer()
        {
            if (IsInitialized) return true;
            IsInitialized = true;
            try
            {
                // set identity
                PluginUUID = _plugin.PluginUUID;
                Version = _plugin.Version;
                Name = _plugin.Name;
                Author = _plugin.Author;

                // get devices
                var baseDevices = AvailableDevices.Devices.Select(dev => dev.BaseDevice);
                // get devs 
                // register and add 
                var allDevUUIDs = baseDevices.Select(d => d.UUID);
                var hasDeviceWithSupportedAlgos = false;
                var supported = _plugin.GetSupportedAlgorithms(baseDevices);
                // who knows who wrote this don't blindly trust the content
                if (supported != null)
                {
                    foreach (var kvp in supported)
                    {
                        var baseDev = kvp.Key;
                        var baseAlgos = kvp.Value;
                        if (baseDev == null || baseAlgos == null)
                        {
                            // TODO this is something we should consider harmfull ???
                            continue;
                        }
                        if (allDevUUIDs.Contains(baseDev.UUID) == false)
                        {
                            Logger.Error(LogTag, $"InitPluginContainer plugin GetSupportedAlgorithms returned an unknown device {baseDev.UUID}. Setting plugin as broken");
                            SetAsBroken(this);
                            return false;
                        }
                        if (baseAlgos.Count == 0)
                        {
                            continue;
                        }
                        hasDeviceWithSupportedAlgos = true;
                        _cachedAlgorithms[baseDev] = baseAlgos;
                    }
                }
                // dependencies and services are considered compatible or if we have algorithms on any device
                IsCompatible = (_plugin is IBackgroundService) || (_plugin is IPluginDependency) || hasDeviceWithSupportedAlgos;
                if (!IsCompatible) return false;

                // transform 
                foreach (var deviceAlgosPair in _cachedAlgorithms)
                {
                    var deviceUUID = deviceAlgosPair.Key.UUID;
                    var algos = deviceAlgosPair.Value
                        .Select(a => new AlgorithmContainer(a, this, AvailableDevices.GetDeviceWithUuid(deviceUUID)))
                        .ToList();
                    _cachedNiceHashMinerAlgorithms[deviceUUID] = algos;
                }

                // Ethlargement extra check
                if (_plugin == EthlargementIntegratedPlugin.Instance)
                {
                    IsCompatible = EthlargementIntegratedPlugin.Instance.SystemContainsSupportedDevices;
                }
            }
            catch (Exception e)
            {
                SetAsBroken(this);
                Logger.Error(LogTag, $"InitPluginContainer error: {e.Message}");
                return false;
            }


            return true;
        }

        private bool _initInternalsCalled = false;
        public void AddAlgorithmsToDevices()
        {
            var payingRates = NHSmaData.CurrentPayingRatesSnapshot();
            CheckExec(nameof(AddAlgorithmsToDevices), () =>
            {
                if (!_initInternalsCalled && _plugin is IInitInternals impl)
                {
                    _initInternalsCalled = true;
                    impl.InitInternals();
                }
                // update settings for algos per device
                var devices = _cachedNiceHashMinerAlgorithms.Keys.Select(uuid => AvailableDevices.GetDeviceWithUuid(uuid)).Where(d => d != null);
                foreach (var dev in devices)
                {
                    var algos = _cachedNiceHashMinerAlgorithms[dev.Uuid];
                    foreach (var algo in algos)
                    {
                        algo.UpdateEstimatedProfit(payingRates);
                        // try get data from configs
                        var pluginConf = dev.GetPluginAlgorithmConfig(algo.AlgorithmStringID);
                        if (pluginConf == null) continue;
                        // set plugin algo
                        algo.Speeds = pluginConf.Speeds;
                        algo.Enabled = pluginConf.Enabled;
                        algo.ExtraLaunchParameters = pluginConf.ExtraLaunchParameters;
                        algo.PowerUsage = pluginConf.PowerUsage;
                        algo.ConfigVersion = pluginConf.GetVersion();
                        // check if re-bench is needed
                        var isReBenchmark = ShouldReBenchmarkAlgorithmOnDevice(dev.BaseDevice, algo.ConfigVersion, algo.IDs);
                        if (isReBenchmark)
                        {
                            Logger.Info(LogTag, $"Algorithms {algo.AlgorithmStringID} SET TO RE-BENCHMARK");
                        }
                        algo.IsReBenchmark = isReBenchmark;
                    }
                    // finally update algorithms
                    // remove old
                    dev.RemovePluginAlgorithms(PluginUUID);
                    dev.AddPluginAlgorithms(algos);
                }
            });
        }

        public void RemoveAlgorithmsFromDevices()
        {
            var devices = _cachedNiceHashMinerAlgorithms.Keys.Select(uuid => AvailableDevices.GetDeviceWithUuid(uuid)).Where(d => d != null);

            // remove
            foreach (var dev in devices)
            {
                dev.RemovePluginAlgorithms(PluginUUID);
            }
        }

        #region IMinerPlugin

        public Version Version { get; private set; }

        public string Name { get; private set; }

        public string Author { get; private set; }

        public string PluginUUID { get; private set; }

        public bool CanGroupAlgorithmContainer(AlgorithmContainer a, AlgorithmContainer b)
        {
            return CanGroup(a.ToMiningPair(), b.ToMiningPair());
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return CheckExec(nameof(CanGroup), () => _plugin.CanGroup(a, b), false);
        }

        public IMiner CreateMiner()
        {
            return CheckExec(nameof(CreateMiner), () => _plugin.CreateMiner(), null);
        }


        #endregion IMinerPlugin

        public bool HasMisingBinaryPackageFiles()
        {
            return CheckBinaryPackageMissingFiles().Any();
        }

        #region IBinaryPackageMissingFilesChecker
        private IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var defaultRet = Enumerable.Empty<string>();
            if (_plugin is IBinaryPackageMissingFilesChecker impl)
            {
                return CheckExec(nameof(impl.CheckBinaryPackageMissingFiles), () => impl.CheckBinaryPackageMissingFiles(), defaultRet);
            }
            return defaultRet;
        }
        #endregion IBinaryPackageMissingFilesChecker

        #region IDevicesCrossReference

        public bool HasDevicesCrossReference()
        {
            return _plugin is IDevicesCrossReference;
        }

#warning Check this with NVIDIA and AMD driver recovery 
        private bool _devicesCrossReference = false;
        public Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (!_devicesCrossReference && _plugin is IDevicesCrossReference impl)
            {
                _devicesCrossReference = true;
                return CheckExec(nameof(impl.DevicesCrossReference), () => impl.DevicesCrossReference(devices), Task.CompletedTask);
            }
            return Task.CompletedTask;
        }
        #endregion IDevicesCrossReference

        #region IGetApiMaxTimeout/IGetApiMaxTimeoutV2
        public TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            if (_plugin is IGetApiMaxTimeoutV2 impl)
            {
                var enabled = CheckExec(nameof(impl.IsGetApiMaxTimeoutEnabled) + "V2", () => impl.IsGetApiMaxTimeoutEnabled, false);
                if (!enabled)
                {
                    // 10 years for a timeout this is basically like being disabled
                    return new TimeSpan(3650, 0, 30, 0);
                }
                return CheckExec(nameof(impl.GetApiMaxTimeout) + "V2", () => impl.GetApiMaxTimeout(miningPairs), new TimeSpan(0, 30, 0));
            }
            // make default 30minutes
            return new TimeSpan(0, 30, 0);
        }
        #endregion IGetApiMaxTimeout/IGetApiMaxTimeoutV2

        #region IReBenchmarkChecker
        private bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (_plugin is IReBenchmarkChecker impl)
            {
                return impl.ShouldReBenchmarkAlgorithmOnDevice(device, benchmarkedPluginVersion, ids);
            }
            return false;
        }
        #endregion IReBenchmarkChecker

        public IEnumerable<string> GetMinerBinsUrls()
        {
            if (_plugin is IMinerBinsSource impl)
            {
                return CheckExec(nameof(impl.GetMinerBinsUrlsForPlugin), () => impl.GetMinerBinsUrlsForPlugin(), Enumerable.Empty<string>());
            }
            return Enumerable.Empty<string>();
        }

        public string GetBinsPackagePassword()
        {
            if (_plugin is IGetBinsPackagePassword impl)
            {
                return CheckExec(nameof(impl.BinsPackagePassword), () => impl.BinsPackagePassword, null);
            }
            return null;
        }

        public MinerOptionsPackage GetMinerOptionsPackage()
        {
            try
            {
                if (_plugin is IGetMinerOptionsPackage get) return get.GetMinerOptionsPackage();

                Type typecontroller = typeof(NHM.MinerPluginToolkitV1.PluginBase);
                var propInfo = typecontroller.GetProperty("MinerOptionsPackage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty);
                var propInfo2 = typecontroller.GetProperty("MinerOptionsPackage");
                if (propInfo != null)
                {
                    var ret = (MinerOptionsPackage)propInfo.GetValue(this._plugin);
                    return ret;
                }
            }
            catch
            {
            }
            return null;
        }

        // generic checker
        #region Generic Safe Checkers
        private T CheckExec<T>(string functionName, Func<T> function, T defaultRet)
        {
            if (IsBroken)
            {
                Logger.Error(LogTag, $"Plugin is broken returning from {functionName}");
                return defaultRet;
            }

            try
            {
                return function();
            }
            catch (Exception e)
            {
                SetAsBroken(this);
                Logger.Error(LogTag, $"{functionName} error: {e.Message}");
                IsBroken = true;
                return defaultRet;
            }
        }

        private void CheckExec(string functionName, Action function)
        {
            // fake return
            CheckExec(functionName, () =>
            {
                function();
                return "success";
            }, "fail");
        }
        #endregion Generic Safe Checkers
    }
}
