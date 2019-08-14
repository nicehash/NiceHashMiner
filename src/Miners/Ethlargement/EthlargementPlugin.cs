using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
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
using System.Threading.Tasks;

namespace Ethlargement
{
    public class Ethlargement : IMinerPlugin, IInitInternals, IBackroundService, IBinaryPackageMissingFilesChecker, IMinerBinsSource
    {
        public virtual string PluginUUID => "efd40691-618c-491a-b328-e7e020bda7a3";

        public Version Version => new Version(1, 2);
        public string Name => "Ethlargement";

        public string Author => "stanko@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            foreach (var dev in devices)
            {
                _registeredSupportedDevices[dev.UUID] = dev.Name;
            }
            // return empty
            return new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
        }

        public bool ServiceEnabled { get; set; } = false;

        // register in GetSupportedAlgorithms and filter in InitInternals
        private static Dictionary<string, string> _registeredSupportedDevices = new Dictionary<string, string>();

        private static List<AlgorithmType> _supportedAlgorithms = new List<AlgorithmType> { AlgorithmType.DaggerHashimoto, AlgorithmType.MTP };

        private bool IsServiceDisabled => !ServiceEnabled && _registeredSupportedDevices.Count > 0;

        private static Dictionary<string, AlgorithmType> _devicesUUIDActiveAlgorithm = new Dictionary<string, AlgorithmType>();

        private static object _startStopLock = new object();

        public void Start(IEnumerable<MiningPair> miningPairs)
        {
            lock (_startStopLock)
            {
                if (IsServiceDisabled) return;

                // check if any mining pair is supported and set current active 
                var supportedUUIDs = _registeredSupportedDevices.Select(kvp => kvp.Key);
                var supportedPairs = miningPairs.Where(pair => supportedUUIDs.Contains(pair.Device.UUID));
                if (supportedPairs.Count() == 0) return; 

                foreach (var pair in supportedPairs)
                {
                    var uuid = pair.Device.UUID;
                    var algorithmType = pair.Algorithm.FirstAlgorithmType;
                    _devicesUUIDActiveAlgorithm[uuid] = algorithmType;
                }
                var shouldRun = _devicesUUIDActiveAlgorithm.Any(kvp => _supportedAlgorithms.Contains(kvp.Value));
                if (shouldRun)
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
                if (IsServiceDisabled) return;

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
                    var shouldRun = _devicesUUIDActiveAlgorithm.Any(kvp => _supportedAlgorithms.Contains(kvp.Value));
                    if (!shouldRun)
                    {
                        StopEthlargementProcess();
                    }
                }
            }
        }

        public virtual string EthlargementBinPath()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "OhGodAnETHlargementPill-r2.exe");
            return binPath;
        }


        #region Ethlargement Process

        private static string _ethlargementBinPath = "";

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
            var shouldRun = _devicesUUIDActiveAlgorithm.Any(kvp => kvp.Value == AlgorithmType.DaggerHashimoto);
            if (shouldRun)
            {
                StartEthlargementProcess();
            }
        }

        private static void StartEthlargementProcess()
        {
            if (IsEthlargementProcessRunning() == true) return;

            _ethlargementProcess = new Process
            {
                StartInfo =
                {
                    FileName = _ethlargementBinPath,
                    //CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };
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
                Logger.Info("ETHLARGEMENT", $"Ethlargement wasn't able to start: {e.Message}");
                //Helpers.ConsolePrint("ETHLARGEMENT", ex.Message);
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
                Logger.Info("ETHLARGEMENT", $"Ethlargement wasn't able to stop: {e.Message}");

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

        public void InitInternals()
        {
            // set ethlargement path
            _ethlargementBinPath = EthlargementBinPath();

            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var fileMinersBinsUrlsSettings = InternalConfigs.InitInternalSetting(pluginRoot, MinersBinsUrlsSettings, "MinersBinsUrlsSettings.json");
            if (fileMinersBinsUrlsSettings != null) MinersBinsUrlsSettings = fileMinersBinsUrlsSettings;

            var readFromFileEnvSysVars = InternalConfigs.InitInternalSetting(pluginRoot, _ethlargementSettings, "EthlargementSettings.json");
            if (readFromFileEnvSysVars != null && readFromFileEnvSysVars.UseUserSettings) _ethlargementSettings = readFromFileEnvSysVars;

            // Filter out supported ones
            var supportedDevicesNames = _ethlargementSettings.SupportedDeviceNames;
            if (supportedDevicesNames != null)
            {
                Func<string, bool> isSupportedName = (string name) => supportedDevicesNames.Any(supportedPart => name.Contains(supportedPart));

                var unsupportedDevicesUUIDs = _registeredSupportedDevices.Where(kvp => !isSupportedName(kvp.Value)).Select(kvp => kvp.Key).ToArray();
                foreach (var removeKey in unsupportedDevicesUUIDs)
                {
                    _registeredSupportedDevices.Remove(removeKey);
                }
            }
            if (_ethlargementSettings.SupportedAlgorithms != null) _supportedAlgorithms = _ethlargementSettings.SupportedAlgorithms;
        }

        protected SupportedDevicesSettings _ethlargementSettings = new SupportedDevicesSettings
        {
            SupportedDeviceNames = new List<string> { "1080", "1080 Ti", "Titan Xp" },
            SupportedAlgorithms = _supportedAlgorithms
        };

        protected MinersBinsUrlsSettings MinersBinsUrlsSettings { get; set; } = new MinersBinsUrlsSettings {
            Urls = new List<string> { "https://github.com/nicehash/NiceHashMinerTest/releases/download/1.9.1.5/Ethlargement.7z" } // link not original
        };
        #endregion Internal settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles("", new List<string> { EthlargementBinPath() });
        }

        #region IMinerBinsSource
        public IEnumerable<string> GetMinerBinsUrlsForPlugin()
        {
            if (MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.Urls == null) return Enumerable.Empty<string>();
            return MinersBinsUrlsSettings.Urls;
        }
        #endregion IMinerBinsSource
    }
}
