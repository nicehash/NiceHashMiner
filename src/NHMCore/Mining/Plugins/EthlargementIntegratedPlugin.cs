using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using NHMCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace NHMCore.Mining.Plugins
{
    public class EthlargementIntegratedPlugin : NotifyChangedBase, IMinerPlugin, IInitInternals, IBackgroundService, IBinaryPackageMissingFilesChecker, IMinerBinsSource, IGetPluginMetaInfo
    {
        public static EthlargementIntegratedPlugin Instance { get; } = new EthlargementIntegratedPlugin();

        public string PluginUUID => "Ethlargement";

        public bool IsSystemElevated => Helpers.IsElevated;
        public bool SystemContainsSupportedDevicesNotSystemElevated => SystemContainsSupportedDevices && !Helpers.IsElevated;

        public Version Version => new Version(15, 0);
        public string Name => "Ethlargement";

        public string Author => "info@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // return empty
            return new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
        }

        public bool ServiceEnabled { get; set; } = false;

        // register in GetSupportedAlgorithms and filter in InitInternals
        private static Dictionary<string, string> _registeredSupportedDevices = new Dictionary<string, string>();

        private bool IsServiceDisabled => !IsInstalled && !ServiceEnabled && _registeredSupportedDevices.Count > 0;

        private static Dictionary<string, AlgorithmType> _devicesUUIDActiveAlgorithm = new Dictionary<string, AlgorithmType>();

        private static bool ShouldRun => _devicesUUIDActiveAlgorithm.Values.Any(Instance.IsSupportedAlgorithm);

        public bool SystemContainsSupportedDevices => _registeredSupportedDevices.Count > 0;

        public bool IsInstalled
        {
            get
            {
                try
                {
                    // TODO if we relly on installed package with just checking the executable existance we will most likely fail somewhere along the line
                    return File.Exists(_ethlargementBinPath);
                }
                catch (Exception e)
                {
                    Logger.Error("ETHLARGEMENT", $"Ethlargement IsInstalled: {e.Message}");
                    return false;
                }
            }
        }

        public void Remove()
        {
            Stop();
            System.Threading.Thread.Sleep(500);
            try
            {
                File.Delete(_ethlargementBinPath);
            }
            catch (Exception e)
            {
                Logger.Error("ETHLARGEMENT", $"Ethlargement Remove: {e.Message}");
            }
        }

        private static object _startStopLock = new object();

        public void Start(IEnumerable<MiningPair> miningPairs)
        {
            lock (_startStopLock)
            {
                if (IsServiceDisabled)
                {
                    StopEthlargementProcess();
                    return;
                }

                // check if any mining pair is supported and set current active 
                var supportedUUIDs = _registeredSupportedDevices.Select(kvp => kvp.Key);
                var supportedPairs = miningPairs.Where(pair => supportedUUIDs.Contains(pair.Device.UUID) && !ShouldIgnoreMinerPluginUUIDs(pair.Algorithm.MinerID));
                if (supportedPairs.Count() == 0) return;

                foreach (var pair in supportedPairs)
                {
                    var uuid = pair.Device.UUID;
                    var algorithmType = pair.Algorithm.FirstAlgorithmType;
                    _devicesUUIDActiveAlgorithm[uuid] = algorithmType;
                }

                if (ShouldRun)
                {
                    StartEthlargementProcess();
                }
                else
                {
                    StopEthlargementProcess();
                }
            }
        }

        public void Stop(IEnumerable<MiningPair> miningPairs = null)
        {
            lock (_startStopLock)
            {
                if (IsServiceDisabled)
                {
                    StopEthlargementProcess();
                    return;
                }

                var stopAll = miningPairs == null;
                // stop all
                if (stopAll)
                {
                    // TODO STOP Ethlargement
                    var keys = _devicesUUIDActiveAlgorithm.Keys.ToArray();
                    foreach (var key in keys) _devicesUUIDActiveAlgorithm[key] = AlgorithmType.NONE;
                    StopEthlargementProcess();
                }
                else
                {
                    // check if any mining pair is supported and set current active 
                    var supportedUUIDs = _registeredSupportedDevices.Select(kvp => kvp.Key);
                    var supportedPairs = miningPairs
                        .Where(pair => supportedUUIDs.Contains(pair.Device.UUID))
                        .Select(pair => pair.Device.UUID).ToArray();
                    if (supportedPairs.Count() == 0) return;

                    foreach (var uuid in supportedPairs)
                    {
                        _devicesUUIDActiveAlgorithm[uuid] = AlgorithmType.NONE;
                    }
                    if (!ShouldRun)
                    {
                        StopEthlargementProcess();
                    }
                }
            }
        }


        private string EthlargementOldBinPath()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", $"{11}.{1}");
            var binPath = Path.Combine(pluginRootBins, "OhGodAnETHlargementPill-r2.exe");
            return binPath;
        }

        public static string BinName = "ETHlargementPill-r2.exe";

        public virtual string EthlargementBinPath()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", $"{Version.Major}.{Version.Minor}");
            var binPath = Path.Combine(pluginRootBins, BinName);
            return binPath;
        }

        public virtual string EthlargementCwdPath()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", $"{Version.Major}.{Version.Minor}");
            return pluginRootBins;
        }


        #region Ethlargement Process

        private static string _ethlargementBinPath = "";
        private static string _ethlargementCwdPath = "";

        private static Process _ethlargementProcess = null;

        private static bool IsEthlargementProcessRunning()
        {
            try
            {
                if (_ethlargementProcess == null) return false;
                return Process.GetProcessById(_ethlargementProcess.Id) != null;
            }
            catch
            {
                return false;
            }
        }

        private async static void ExitEvent(object sender, EventArgs e)
        {
            _ethlargementProcess = null;
            await Task.Delay(1000);
            // TODO add delay and check if it is running
            // lock and check
            if (ShouldRun)
            {
                StartEthlargementProcess();
            }
        }

        private static void StartEthlargementProcess()
        {
            if (IsEthlargementProcessRunning() == true) return;

            _ethlargementProcess = MinerToolkit.CreateMiningProcess(_ethlargementBinPath, _ethlargementCwdPath, "", null);
            _ethlargementProcess.Exited += ExitEvent;

            try
            {
                if (_ethlargementProcess.Start())
                {
                    Logger.Info("ETHLARGEMENT", "Starting ethlargement...");
                    //Helpers.ConsolePrint("ETHLARGEMENT", "Starting ethlargement...");
                }
                else
                {
                    Logger.Info("ETHLARGEMENT", "Couldn't start ethlargement");
                    //Helpers.ConsolePrint("ETHLARGEMENT", "Couldn't start ethlargement");
                }
            }
            catch (Exception e)
            {
                Logger.Error("ETHLARGEMENT", $"Ethlargement wasn't able to start: {e.Message}");
            }
        }

        private static void StopEthlargementProcess()
        {
            if (IsEthlargementProcessRunning() == false) return;
            try
            {
                _ethlargementProcess.Exited -= ExitEvent;
                _ethlargementProcess.CloseMainWindow();
                if (!_ethlargementProcess.WaitForExit(10 * 1000))
                {
                    _ethlargementProcess.Kill();
                }

                _ethlargementProcess.Close();
                _ethlargementProcess = null;
            }
            catch (Exception e)
            {
                Logger.Error("ETHLARGEMENT", $"Ethlargement wasn't able to stop: {e.Message}");
            }
        }

        #endregion Ethlargement Process

        #region IMinerPlugin stubs
        public IMiner CreateMiner()
        {
            return null;
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return false;
        }
        #endregion IMinerPlugin stubs

        #region Internal settings

        public virtual void InitInternals(){} // STUB

        public void InitAndCheckSupportedDevices(IEnumerable<BaseDevice> devices)
        {
            // set ethlargement path
            _ethlargementBinPath = EthlargementBinPath();
            _ethlargementCwdPath = EthlargementCwdPath();

            // copy EthLargement binary 
            try
            {
                var oldPath = EthlargementOldBinPath();
                if (File.Exists(oldPath) && !File.Exists(_ethlargementBinPath))
                {
                    if (!Directory.Exists(_ethlargementCwdPath)) Directory.CreateDirectory(_ethlargementCwdPath);
                    File.Copy(oldPath, _ethlargementBinPath);
                }
            }
            catch
            {}


            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var fileMinersBinsUrlsSettings = InternalConfigs.InitInternalSetting(pluginRoot, MinersBinsUrlsSettings, "MinersBinsUrlsSettings.json");
            if (fileMinersBinsUrlsSettings != null) MinersBinsUrlsSettings = fileMinersBinsUrlsSettings;

            var readFromFileEnvSysVars = InternalConfigs.InitInternalSetting(pluginRoot, _ethlargementSettings, "EthlargementSettings.json");
            if (readFromFileEnvSysVars != null && readFromFileEnvSysVars.UseUserSettings) _ethlargementSettings = readFromFileEnvSysVars;

            // Filter out supported ones
            _registeredSupportedDevices = new Dictionary<string, string>();
            devices.Where(dev => IsSupportedDeviceName(dev.Name)).ToList().ForEach(dev => _registeredSupportedDevices[dev.UUID] = dev.Name);
            OnPropertyChanged(nameof(SystemContainsSupportedDevices));
            OnPropertyChanged(nameof(SystemContainsSupportedDevicesNotSystemElevated));
        }

        public class SupportedDevicesSettings : IInternalSetting
        {
            [JsonProperty("use_user_settings")]
            public bool UseUserSettings { get; set; } = false;

            [JsonProperty("supported_device_names")]
            public List<string> SupportedDeviceNames { get; set; } = new List<string> { "1080", "1080 Ti", "Titan Xp", "TITAN Xp" };

            [JsonProperty("supported_algorithms")]
            public List<AlgorithmType> SupportedAlgorithms { get; set; } = new List<AlgorithmType> { AlgorithmType.DaggerHashimoto };

            [JsonProperty("ignore_miner_uuids")]
            public List<string> IgnoreMinerPluginUUIDs { get; set; } = new List<string>{ };
            // "f683f550-94eb-11ea-a64d-17be303ea466" NBMiner
        }

        protected SupportedDevicesSettings _ethlargementSettings = new SupportedDevicesSettings{};

        protected bool IsSupportedAlgorithm(AlgorithmType algorithmType)
        {
            try
            {
                return _ethlargementSettings?.SupportedAlgorithms?.Contains(algorithmType) ?? false;
            }
            catch
            {
                return false;
            }
        }

        protected bool IsSupportedDeviceName(string deviceName)
        {
            try
            {
                return _ethlargementSettings?.SupportedDeviceNames?.Any(supportedPart => deviceName.Contains(supportedPart)) ?? false;
            }
            catch
            {
                return false;
            }
        }

        protected bool ShouldIgnoreMinerPluginUUIDs(string pluginUUID)
        {
            try
            {
                return _ethlargementSettings?.IgnoreMinerPluginUUIDs?.Contains(pluginUUID) ?? false;
            }
            catch
            {
                return false;
            }
        }

        

        protected MinersBinsUrlsSettings MinersBinsUrlsSettings { get; set; } = new MinersBinsUrlsSettings
        {
            Urls = new List<string> { "https://github.com/Virosa/ETHlargementPill/raw/master/ETHlargementPill-r2.exe" } 
        };
        #endregion Internal settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            if (SystemContainsSupportedDevices)
            {
                return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles("", new List<string> { EthlargementBinPath() });
            }
            return Enumerable.Empty<string>();
        }

        #region IMinerBinsSource
        public IEnumerable<string> GetMinerBinsUrlsForPlugin()
        {
            if (MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.Urls == null) return Enumerable.Empty<string>();
            return MinersBinsUrlsSettings.Urls;
        }

        PluginMetaInfo IGetPluginMetaInfo.GetPluginMetaInfo()
        {
            return new PluginMetaInfo
            {
                PluginDescription = "ETHlargement increases DaggerHashimoto hashrate for NVIDIA 1080, 1080 Ti and Titan Xp GPUs.", 
                SupportedDevicesAlgorithms = new Dictionary<DeviceType, List<AlgorithmType>> { { DeviceType.NVIDIA, new List<AlgorithmType> { AlgorithmType.DaggerHashimoto } } }
            };
        }

        #endregion IMinerBinsSource
    }
}
