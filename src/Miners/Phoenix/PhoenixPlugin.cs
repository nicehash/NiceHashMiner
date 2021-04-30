using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Phoenix
{
    public partial class PhoenixPlugin : PluginBase //, IDevicesCrossReference
    {
        public PhoenixPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://bitcointalk.org/index.php?topic=2647654.0 || new one: https://bitcointalk.org/index.php?topic=2647654.0
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "5.5c",
                ExePath = new List<string> { "PhoenixMiner_5.5c_Windows", "PhoenixMiner.exe" },
                Urls = new List<string>
                {
                    //"https://mega.nz/folder/2VskDJrI#lsQsz1CdDe8x5cH3L8QaBw/file/OBNSQJIR" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Phoenix Miner is fast Ethash miner that supports both AMD and Nvidia cards(including in mixed mining rigs).",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "fa369d10-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 0);
        public override string Name => "Phoenix";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // cleanup all 
            var pluginDir = Paths.MinerPluginsPath(PluginUUID);
            Directory.EnumerateDirectories(pluginDir)
                .ToList()
                .ForEach(dirPath => {
                    try
                    {
                        Directory.Delete(dirPath, true);
                    }
                    catch
                    { }
                });
            Directory.EnumerateFiles(pluginDir)
                .ToList()
                .ForEach(filePath => {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    { }
                });
            // return empty 
            return new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // map ids by bus ids
            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            int indexAMD = -1;
            foreach (var gpu in gpus.Where(gpu => gpu is AMDDevice))
            {
                _mappedIDs[gpu.UUID] = ++indexAMD;
            }

            int indexNVIDIA = -1;
            foreach (var gpu in gpus.Where(gpu => gpu is CUDADevice))
            {
                _mappedIDs[gpu.UUID] = ++indexNVIDIA;
            }

            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= new Version(377, 0);
            var supportedGpus = gpus.Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported));

            foreach (var gpu in supportedGpus)
            {
                var algorithms = GetSupportedAlgorithmsForDevice(gpu as BaseDevice);
                if (algorithms.Count > 0) supported.Add(gpu as BaseDevice, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(IGpuDevice dev)
        {
            var isSupported = dev is AMDDevice;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(IGpuDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 3;
            return isSupported && isDriverSupported;
        }

        public override bool CanGroup(MiningPair a, MiningPair b)
        {
            var isSameDeviceType = a.Device.DeviceType == b.Device.DeviceType;
            if (!isSameDeviceType) return false;
            return base.CanGroup(a, b);
        }

        protected override MinerBase CreateMinerBase()
        {
            return new Phoenix(PluginUUID, _mappedIDs);
        }

        //public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        //{
        //    if (_mappedIDs.Count == 0) return;

        //    var ts = DateTime.UtcNow.Ticks;
        //    var crossRefferenceList = new (DeviceType deviceType, string parameters, string dumpFile)[]
        //    {
        //        (DeviceType.AMD,    "-list -gbase 0 -amd",    $"d{ts}_AMD.txt"),
        //        (DeviceType.NVIDIA, "-list -gbase 0 -nvidia", $"d{ts}_NVIDIA.txt"),
        //    };
        //    var crossRefRunParams = crossRefferenceList
        //        .Where(p => devices.Any(dev => dev.DeviceType == p.deviceType))
        //        .Select(p => (p.parameters, p.dumpFile));

        //    var (minerBinPath, minerCwdPath) = GetBinAndCwdPaths();
        //    var crossRefOutputs = new List<string> { };
        //    // exec await sequentially
        //    foreach (var (parameters, dumpFile) in crossRefRunParams)
        //    {
        //        var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, parameters);
        //        crossRefOutputs.Add(output);
        //        await Task.Delay(TimeSpan.FromSeconds(1));
        //        try
        //        {
        //            File.WriteAllText(Path.Combine(minerCwdPath, dumpFile), output);
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Error("PhoenixPlugin", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
        //        }
        //    }
        //    var mappedDevs = crossRefOutputs.SelectMany(output => DevicesListParser.ParsePhoenixOutput(output, devices));
        //    foreach (var (uuid, gpuId) in mappedDevs)
        //    {
        //        Logger.Info("PhoenixPlugin", $"DevicesCrossReference '{uuid}' => {gpuId}");
        //        _mappedIDs[uuid] = gpuId;
        //    }
        //}

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "PhoenixMiner.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                return benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 6;
            }
            catch (Exception e)
            {
                Logger.Error("PhoenixPlugin", $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
