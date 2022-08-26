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
#if LHR_BUILD_ON
    public partial class ExcavatorPlugin : PluginBase, IDevicesCrossReference, IDriverIsMinimumRecommended, IDriverIsMinimumRequired
#else
    public partial class ExcavatorPlugin : PluginBase, IDevicesCrossReference
#endif
    {
        public ExcavatorPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
#if LHR_BUILD_ON
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.7.7.6",
                ExePath = new List<string> { "NHQM_v0.5.5.0", "excavator.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/NiceHashQuickMiner/releases/download/v0.5.5.0/NHQM_v0.5.5.0.zip"
                }
            };
#else
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.7.6.5",
                ExePath = new List<string> { "NHQM_v0.5.3.6", "excavator.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/NiceHashQuickMiner/releases/download/v0.5.3.6/NHQM_v0.5.3.6.zip"
                }
            };
#endif
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Excavator NVIDIA/AMD GPU miner from NiceHash",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

#if LHR_BUILD_ON
        public override Version Version => new Version(17, 2);
#else
        public override Version Version => new Version(16, 3);
#endif

        public override string PluginUUID => "27315fe0-3b03-11eb-b105-8d43d5bd63be";
        public override string Name => "Excavator";

        public override string Author => "info@nicehash.com";

        private bool TriedToDeleteQMFiles = false;
        protected readonly Dictionary<string, string> _mappedDeviceIds = new Dictionary<string, string>();

        private static readonly List<string> ImportantExcavatorFiles = new List<string>() { "excavator.exe", "EIO.dll", "IOMap64.sys" };

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
            supported.ToList().ForEach(dev => _mappedDeviceIds[dev.Key.UUID] = dev.Key.UUID);
            return supported;
        }

        private static Version NVIDIA_Min_Version = new Version(411, 0);
        private Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedDevicesAndAlgorithms(IEnumerable<BaseDevice> devices)
        {
            bool isNVIDIADriverGreaterThanMinVersion() => CUDADevice.INSTALLED_NVIDIA_DRIVERS >= NVIDIA_Min_Version;
            bool isSupportedGPU(BaseDevice gpu) =>
                gpu switch
                {
                    CUDADevice cuda => isNVIDIADriverGreaterThanMinVersion() && cuda.SM_major >= 6,
                    _ => gpu is AMDDevice,
                };
            if (!isNVIDIADriverGreaterThanMinVersion()) Logger.Error("ExcavatorPlugin", $"Insufficient NVIDIA driver version. Installed {CUDADevice.INSTALLED_NVIDIA_DRIVERS} Required {NVIDIA_Min_Version}");
            return devices
                .Where(isSupportedGPU)
                .Select(gpu => (gpu, algos: GetSupportedAlgorithmsForDevice(gpu)))
                .Where(p => p.algos.Any())
                .ToDictionary(p => p.gpu, p => p.algos);
        }

        private void CreateExcavatorCommandTemplate(IEnumerable<string> uuids)
        {
            try
            {
                var templatePath = CmdConfig.CommandFileTemplatePath(PluginUUID);
                var template = CmdConfig.CreateTemplate(uuids);
                if (!File.Exists(templatePath) && template != null)
                {
                    File.WriteAllText(templatePath, template);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ExcavatorPlugin", $"CreateExcavatorCommandTemplate {e}");
            }
        }

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
            return DriverVersionChecker.CompareCUDADriverVersions(device, CUDADevice.INSTALLED_NVIDIA_DRIVERS, new Version(461, 33));
        }

        public (DriverVersionCheckType ret, Version minRequired) IsDriverMinimumRequired(BaseDevice device)
        {
            return DriverVersionChecker.CompareCUDADriverVersions(device, CUDADevice.INSTALLED_NVIDIA_DRIVERS, new Version(411, 31));
        }

        private async Task<string> QueryExcavatorForDevices(string binPath)
        {
            string result = string.Empty;
            var tempQueryPort = FreePortsCheckerManager.GetAvaliablePortFromSettings();
            var startInfo = new ProcessStartInfo
            {
                FileName = binPath,
                Arguments = $"-wp {tempQueryPort}",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            void killProcess(Process handle)
            {
                try
                {
                    var isRunning = !handle?.HasExited ?? false;
                    if (!isRunning) return;
                    handle.CloseMainWindow();
                    var hasExited = handle?.WaitForExit(1000) ?? false;
                    if (!hasExited) handle.Kill();
                }
                catch (Exception e)
                {
                    Logger.Error("ExcavatorPlugin", $"Unable to get handle: {e.Message}");
                }
            };

            using var excavatorHandle = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            using var ct = new CancellationTokenSource(30 * 1000);
            using var client = new HttpClient();
            excavatorHandle.Start();
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var address = $"http://localhost:{tempQueryPort}";
                    var response = await client.GetAsync(address + @"/api?command={""id"":1,""method"":""devices.get"",""params"":[]}");
                    if (!response.IsSuccessStatusCode) continue;
                    result = await response.Content.ReadAsStringAsync();
                    killProcess(excavatorHandle);
                    break;
                }
                catch { }
                await Task.Delay(1000, ct.Token);
            }
            killProcess(excavatorHandle);
            return result;
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            (var binPath, _) = GetBinAndCwdPaths();
            try
            {
                var gpus = devices.Where(dev => dev is IGpuDevice)
                                       .Cast<IGpuDevice>();
                var queryResult = await QueryExcavatorForDevices(binPath);
                if (queryResult == string.Empty)
                {
                    Logger.Error("ExcavatorPlugin", "Initial excavator uuid query failed, only NVIDIA gpus will work");
                    return;
                }
                var serialized = JsonConvert.DeserializeObject<DeviceListApiResponse>(queryResult);
                foreach (var serializedDev in serialized.devices)
                {
                    var targetGpu = gpus.FirstOrDefault(gpu => serializedDev.details.bus_id == gpu.PCIeBusID);
                    if (targetGpu == null) continue; 
                    _mappedDeviceIds[targetGpu.UUID] = serializedDev.uuid;
                }
                CreateExcavatorCommandTemplate(_mappedDeviceIds.Values);
            }
            catch (Exception e)
            {
                Logger.Error("ExcavatorPlugin", "DevicesCrossReference: " + e.Message);
            }
        }
    }
}
