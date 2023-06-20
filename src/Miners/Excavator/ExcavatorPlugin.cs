using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NHM.MinerPluginToolkitV1.Interfaces;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Excavator
{
    public partial class ExcavatorPlugin : PluginBase, IDevicesCrossReference

    {
        public ExcavatorPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;

            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.8.5.1",
                ExePath = new List<string> { "NHQM_v0.6.5.1", "excavator.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/NiceHashQuickMiner/releases/download/v0.6.5.1/NHQM_v0.6.5.1.zip"
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Excavator NVIDIA/AMD GPU and CPU miner from NiceHash",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(19, 7);

        public override string PluginUUID => "27315fe0-3b03-11eb-b105-8d43d5bd63be";
        public override string Name => "Excavator";

        public override string Author => "info@nicehash.com";

        private bool TriedToDeleteQMFiles = false;
        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        private static readonly List<string> ImportantExcavatorFiles = new List<string>() { "excavator.exe", "EIO.dll", "IOMap64.sys", "WinRing0x64.sys" };

        public override void InitInternals()
        {
            base.InitInternals();
            if (!TriedToDeleteQMFiles)
            {
                TriedToDeleteQMFiles = true;
                var (_, pluginRootBinsPath) = GetBinAndCwdPaths();
                DeleteUnusedQMFiles(pluginRootBinsPath, ImportantExcavatorFiles);
            }
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = GetSupportedDevicesAndAlgorithms(devices);
            supported.ToList().ForEach(dev => _mappedDeviceIds[dev.Key.UUID] = dev.Key.ID);
            return supported;
        }

        private static Version NVIDIA_Min_Version = new Version(527, 41);
        private Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedDevicesAndAlgorithms(IEnumerable<BaseDevice> devices)
        {
            bool isNVIDIADriverGreaterThanMinVersion() => CUDADevice.INSTALLED_NVIDIA_DRIVERS >= NVIDIA_Min_Version;
            bool isSupportedGPU(BaseDevice gpu) =>
                gpu switch
                {
                    CUDADevice cuda => isNVIDIADriverGreaterThanMinVersion() && cuda.SM_major >= 6,
                    CPUDevice dev => gpu is CPUDevice,
                    _ => gpu is AMDDevice,
                };
            if (!isNVIDIADriverGreaterThanMinVersion()) Logger.Error("ExcavatorPlugin", $"Insufficient NVIDIA driver version. Installed {CUDADevice.INSTALLED_NVIDIA_DRIVERS} Required {NVIDIA_Min_Version}");
            return devices
                .Where(isSupportedGPU)
                .Select(gpu => (gpu, algos: GetSupportedAlgorithmsForDevice(gpu)))
                .Where(p => p.algos.Any())
                .ToDictionary(p => p.gpu, p => p.algos);
        }

        //private void CreateExcavatorCommandTemplate(IEnumerable<int> uuids, string algorithmName, string filename)
        //{
        //    try
        //    {
        //        var templatePath = CmdConfig.CommandFileTemplatePath(PluginUUID, filename);
        //        var template = CmdConfig.CreateTemplate(uuids, algorithmName);
        //        if (!File.Exists(templatePath) && template != null)
        //        {
        //            File.WriteAllText(templatePath, template);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Error("ExcavatorPlugin", $"CreateExcavatorCommandTemplate {e}");
        //    }
        //}

        protected override MinerBase CreateMinerBase()
        {
            return new Excavator(PluginUUID, _mappedDeviceIds);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var (_, pluginRootBinsPath) = GetBinAndCwdPaths();
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, ImportantExcavatorFiles);
        }

        private static void DeleteUnusedQMFiles(string binPath, List<string> filesToLeave)
        {
            DirectoryInfo getDirectoryInfo(string path)
            {
                try
                {
                    return new DirectoryInfo(binPath);
                }
                catch (Exception e)
                {
                    Logger.Error("ExcavatorPlugin", $"DeleteUnusedQMFiles: {e.Message}");
                }
                return null;
            };

            void deleteDirectoryInfo(DirectoryInfo dirInfo)
            {
                if (dirInfo == null) return;
                foreach (var file in dirInfo.GetFiles())
                {
                    try {
                        if (file.Name.Contains("cmd_")) continue;
                        if (!filesToLeave.Any(leaveFile => file.Name.Contains(leaveFile))) file.Delete();
                    }
                    catch (Exception e)
                    {
                        Logger.Error("ExcavatorPlugin", $"Delete file '{file}': {e.Message}");
                    }
                }

                foreach (var directory in dirInfo.GetDirectories())
                {
                    try {
                        directory.Delete(true);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("ExcavatorPlugin", $"Delete directory '{directory}': {e.Message}");
                    }
                }
            }

            try
            {
                if (Directory.Exists(binPath))
                {
                    deleteDirectoryInfo(getDirectoryInfo(binPath));
                }
            }
            catch (Exception e)
            {
                Logger.Error("ExcavatorPlugin", $"QM cleanup: {e.Message}");
            }
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                return benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 6;
            }
            catch (Exception e)
            {
                Logger.Error("ExcavatorPlugin", $"ShouldReBenchmarkAlgorithmOnDevice {e}");
            }
            return false;
        }

        public (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRecommended(BaseDevice device)
        {
            return DriverVersionChecker.CompareCUDADriverVersions(device, CUDADevice.INSTALLED_NVIDIA_DRIVERS, new Version(527, 41));
        }

        public (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRequired(BaseDevice device)
        {
            return DriverVersionChecker.CompareCUDADriverVersions(device, CUDADevice.INSTALLED_NVIDIA_DRIVERS, new Version(527, 41));
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            (var binPath, _) = GetBinAndCwdPaths();
            if (_mappedDeviceIds.Count == 0) return;

            var (minerBinPath, minerCwdPath) = GetBinAndCwdPaths();
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "-ld");
            var ts = DateTime.UtcNow.Ticks;
            var dumpFile = $"d{ts}.txt";
            try
            {
                File.WriteAllText(Path.Combine(minerCwdPath, dumpFile), output);
            }
            catch (Exception e)
            {
                Logger.Error("ExcavatorPlugin", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
            }
            var mappedDevs = DevicesListParser.ParseExcavatorOutput(output, devices);

            foreach (var (uuid, minerGpuId) in mappedDevs)
            {
                Logger.Info("ExcavatorPlugin", $"DevicesCrossReference '{uuid}' => {minerGpuId}");
                _mappedDeviceIds[uuid] = minerGpuId;
            }

            var mappedDevCPU = DevicesListParser.ParseExcavatorOutputCPU(output, devices);

            
            Logger.Info("ExcavatorPlugin", $"DevicesCrossReference '{mappedDevCPU.uuid}' => {mappedDevCPU.minerCpuId}");
            _mappedDeviceIds[mappedDevCPU.uuid] = mappedDevCPU.minerCpuId;
        }
    }
}
