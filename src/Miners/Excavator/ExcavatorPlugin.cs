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

namespace Excavator
{
    public partial class ExcavatorPlugin : PluginBase, IDriverIsMinimumRecommended, IDriverIsMinimumRequired
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
            // TODO link
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.7.6.2",
                ExePath = new List<string> { "NHQM_v0.5.3.3", "excavator.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/NiceHashQuickMiner/releases/download/v0.5.3.3/NHQM_v0.5.3.3.zip"
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Excavator NVIDIA GPU miner from NiceHash",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(17, 2);

        public override string Name => "Excavator";

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => "27315fe0-3b03-11eb-b105-8d43d5bd63be";

        private bool TriedToDeleteQMFiles = false;

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
            // SM 6.0+
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 6).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var minDrivers = new Version(411, 0); // TODO
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithmsForDevice(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }
            try
            {
                var templatePath = CmdConfig.CommandFileTemplatePath(PluginUUID);
                var template = CmdConfig.CreateTemplate(supported.Select(p => p.Key.UUID));
                if (!File.Exists(templatePath) && template != null)
                {
                    File.WriteAllText(templatePath, template);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ExcavatorPlugin", $"GetSupportedAlgorithms create cmd template {e}");
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new Excavator(PluginUUID);
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
    }
}
