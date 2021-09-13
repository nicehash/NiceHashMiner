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
    public partial class JokerPlugin : PluginBase, IDevicesCrossReference
    {
        public JokerPlugin()
        {
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
            // TODO here add read binary path and 
            (Devices, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "Devices.json"), new List<DeviceMappings> {
                new DeviceMappings
                {
                    DeviceUUID = "FAMD-7c779e99-4b58-47e1-825c-0a4d6c01a70d",
                    Pcie = -1,
                    MinerDeviceId = 0,
                }
            });
            (MinerSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "MinerSettings.json"), new MinerSettings {});
            (_, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.MinerPluginsPath(PluginUUID, "Test.json"), new List<string> { "1", "2", "3" });
        }

        private IReadOnlyList<DeviceMappings> Devices;

        private MinerSettings MinerSettings;
        PluginEngine PluginEngine;

        public override Version Version => new Version(16, 0);

        public override string Name => "UserPlugin01";

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

        private static bool IsDeviceSupported(IEnumerable<DeviceMappings> userDevices, BaseDevice nhmDevice)
        {
            if (userDevices == null || nhmDevice == null) return false;
            if (nhmDevice is IGpuDevice gpu && userDevices.Any(d => d.Pcie == gpu.PCIeBusID)) return true;
            return userDevices.Any(d => d.DeviceUUID == nhmDevice.UUID);
        }

        private static int GetMinerDeviceID(IEnumerable<DeviceMappings> userDevices, string deviceUUID)
        {
            var targetDevice = userDevices?.FirstOrDefault(d => d.DeviceUUID == deviceUUID);
            return targetDevice?.MinerDeviceId ?? -1;
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = devices
                .Where(d => IsDeviceSupported(Devices, d))
                .Where(d => GetMinerDeviceID(Devices, d.UUID) != -1)
                .Select(d => (device: d, algorithms: GetSupportedAlgorithmsForDevice(d)))
                .Where(p => p.algorithms.Count > 0)
                .ToDictionary(p => p.device, p => p.algorithms);

            foreach (var deviceUUID in supported.Keys.Select(d => d.UUID))
            {
                _mappedDeviceIds[deviceUUID] = GetMinerDeviceID(Devices, deviceUUID);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new JokerMiner(PluginUUID, _mappedDeviceIds, MinerSettings, PluginEngine);
        }

#warning thinks about this one here... we might get away without using it
        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            //if (_mappedDeviceIds.Count == 0) return;

            //var (minerBinPath, minerCwdPath) = GetBinAndCwdPaths();
            //var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--list-devices --nocolor=on");
            //var ts = DateTime.UtcNow.Ticks;
            //var dumpFile = $"d{ts}.txt";
            //try
            //{
            //    File.WriteAllText(Path.Combine(minerCwdPath, dumpFile), output);
            //}
            //catch (Exception e)
            //{
            //    Logger.Error("LolMinerPlugin", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
            //}
            //var mappedDevs = DevicesListParser.ParseLolMinerOutput(output, devices);

            //foreach (var (uuid, minerGpuId) in mappedDevs)
            //{
            //    Logger.Info("LolMinerPlugin", $"DevicesCrossReference '{uuid}' => {minerGpuId}");
            //    _mappedDeviceIds[uuid] = minerGpuId;
            //}
        }

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
