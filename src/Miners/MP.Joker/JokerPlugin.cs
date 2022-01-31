using MP.Joker.Settings;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MP.Joker.PluginEngines;
using InternalConfigsCommon = NHM.Common.Configs.InternalConfigs;

namespace MP.Joker
{
    public partial class JokerPlugin : PluginBase // , IDevicesCrossReference
    {
        public JokerPlugin(string name = "UserPlugin01")
        {
            _name = name;
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                UseUserSettings = true,
                BinVersion = "unknown",
                ExePath = new List<string> { "executableFileName.exe" },
                Urls = new List<string> { }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Unknown",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
            (MinerSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "MinerSettings.json"), new MinerSettings {});
        }

        private int _openClAmdPlatformNum = 0;
        private MinerSettings MinerSettings;
        PluginEngine PluginEngine;

        public override Version Version => new Version(16, 0);

        private readonly string _name;
        public override string Name => _name;

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => Name;

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public override void InitInternals()
        {
            base.InitInternals();
            var exeName = MinersBinsUrlsSettings?.ExePath.LastOrDefault();
            PluginEngine = GuessMinerBinaryPluginEngine(exeName);
            Logger.Info(Name, $"PluginEngine {PluginEngine}");
        }

        private static int GetMinerDeviceID(IEnumerable<DeviceMappings> userDevices, string deviceUUID)
        {
            var targetDevice = userDevices?.FirstOrDefault(d => d.DeviceUUID == deviceUUID);
            return targetDevice?.MinerDeviceId ?? -1;
        }

        private static DeviceMappings BaseDeviceToDeviceMappings(BaseDevice baseDevice)
        {
            return new DeviceMappings
            {
                Compatible = false,
                DeviceName = baseDevice.Name,
                DeviceUUID = baseDevice.UUID,
                Pcie = baseDevice is IGpuDevice gpu ? gpu.PCIeBusID : -1,
                MinerDeviceId = baseDevice.ID
            };
        }

        private static IReadOnlyList<DeviceMappings> DefaultDeviceMappings(IEnumerable<BaseDevice> devices)
        {
            return devices.Select(BaseDeviceToDeviceMappings).ToArray();
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var (devicesSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "Devices.json"), DefaultDeviceMappings(devices));
            var compatibleDevices = devicesSettings.Where(d => d.Compatible).ToArray();
            var supported = devices
                .Where(d => compatibleDevices.Any(cd => cd.DeviceUUID == d.UUID))
                .Where(d => GetMinerDeviceID(devicesSettings, d.UUID) != -1)
                .Select(d => (device: d, algorithms: GetSupportedAlgorithmsForDevice(d)))
                .Where(p => p.algorithms.Count > 0)
                .ToDictionary(p => p.device, p => p.algorithms);

            foreach (var deviceUUID in supported.Keys.Select(d => d.UUID))
            {
                _mappedDeviceIds[deviceUUID] = GetMinerDeviceID(devicesSettings, deviceUUID);
            }

            _openClAmdPlatformNum = devices
                .Where(d => d is AMDDevice)
                .Where(d => _mappedDeviceIds.ContainsKey(d.UUID))
                .Cast<AMDDevice>()
                .Select(d => d.OpenCLPlatformID)
                .FirstOrDefault();

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new JokerMiner(PluginUUID, _mappedDeviceIds, MinerSettings, PluginEngine, _openClAmdPlatformNum);
        }

        #region IBinAndCwdPathsGettter
        public override (string binPath, string cwdPath) GetBinAndCwdPaths()
        {
            if (MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.ExePath == null || MinersBinsUrlsSettings.ExePath.Count == 0)
            {
                throw new Exception("Unable to return cwd and exe paths MinersBinsUrlsSettings == null || MinersBinsUrlsSettings.Path == null || MinersBinsUrlsSettings.Path.Count == 0");
            }
            var paths = new List<string> { Paths.MinerPluginsPath(PluginUUID, "bins") };
            paths.AddRange(MinersBinsUrlsSettings.ExePath);
            var binCwd = Path.Combine(paths.GetRange(0, paths.Count - 1).ToArray());
            var binPath = Path.Combine(paths.ToArray());
            return (binPath, binCwd);
        }
        #endregion IBinAndCwdPathsGettter



//#warning thinks about this one here... we might get away without using it
//        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
//        {
//            //if (_mappedDeviceIds.Count == 0) return;

//            //var (minerBinPath, minerCwdPath) = GetBinAndCwdPaths();
//            //var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--list-devices --nocolor=on");
//            //var ts = DateTime.UtcNow.Ticks;
//            //var dumpFile = $"d{ts}.txt";
//            //try
//            //{
//            //    File.WriteAllText(Path.Combine(minerCwdPath, dumpFile), output);
//            //}
//            //catch (Exception e)
//            //{
//            //    Logger.Error("LolMinerPlugin", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
//            //}
//            //var mappedDevs = DevicesListParser.ParseLolMinerOutput(output, devices);

//            //foreach (var (uuid, minerGpuId) in mappedDevs)
//            //{
//            //    Logger.Info("LolMinerPlugin", $"DevicesCrossReference '{uuid}' => {minerGpuId}");
//            //    _mappedDeviceIds[uuid] = minerGpuId;
//            //}
//        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            return Enumerable.Empty<string>();
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }
    }
}
