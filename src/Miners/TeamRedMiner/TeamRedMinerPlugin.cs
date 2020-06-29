using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using NHM.Common;

namespace TeamRedMiner
{
    public partial class TeamRedMinerPlugin : PluginBase
    {
        public TeamRedMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://github.com/todxx/teamredminer/releases
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "0.7.7",
                ExePath = new List<string> { "teamredminer-v0.7.7-win", "teamredminer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/todxx/teamredminer/releases/download/0.7.7/teamredminer-v0.7.7-win.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Miner for AMD gpus.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "01177a50-94ec-11ea-a64d-17be303ea466";

        public override Version Version => new Version(11, 4);

        public override string Name => "TeamRedMiner";

        public override string Author => "info@nicehash.com";

        public override bool CanGroup(MiningPair a, MiningPair b)
        {
            var canGroup = base.CanGroup(a, b);
            if (a.Device is AMDDevice aDev && b.Device is AMDDevice bDev && aDev.OpenCLPlatformID != bDev.OpenCLPlatformID)
            {
                // OpenCLPlatorm IDs must match
                return false;
            }
            return canGroup;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new TeamRedMiner(PluginUUID);
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // Get AMD GCN4+
            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "teamredminer.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() != 0)
                {
                    if (ids.Contains(AlgorithmType.KAWPOW) && benchmarkedPluginVersion.Major == 11 && benchmarkedPluginVersion.Minor < 2) return true;
                    if (ids.Contains(AlgorithmType.DaggerHashimoto) && benchmarkedPluginVersion.Major == 11 && benchmarkedPluginVersion.Minor < 4) return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            //no improvements for algorithm speeds in the new version - just stability improvements
            return false;
        }
    }
}
