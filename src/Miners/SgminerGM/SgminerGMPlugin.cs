using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SgminerGM
{
    public abstract class SgminerGMPlugin : PluginBase
    {
        public SgminerGMPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://github.com/nicehash/sgminer-gm/releases current v5.5.5-8
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                Urls = new List<string>
                {
                    "https://github.com/nicehash/sgminer-gm/releases/download/5.5.5-8/sgminer-5.5.5-gm-nicehash-8-windows-amd64.zip",
                }
            };
        }

        //public override string PluginUUID => "MISSING";

        public override Version Version => new Version(2, 0);
        public override string Name => "SGminerGM";

        public override string Author => "stanko@nicehash.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            const ulong MinDaggerHashimotoMemory = 3UL << 30; // 3GB
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var amdGpus = devices
                .Where(dev => dev is AMDDevice amdGpu && amdGpu.GpuRam > MinDaggerHashimotoMemory)
                .Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = new List<Algorithm> {
                    new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto)
                    {
                        ExtraLaunchParameters = " --remove-disabled --xintensity 512 -w 192 -g 1"
                    },
                };
                var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
                if (filteredAlgorithms.Count > 0) supported.Add(gpu, filteredAlgorithms);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new SgminerGM(PluginUUID);
        }

        public override bool CanGroup(MiningPair a, MiningPair b)
        {
            var canGroup = base.CanGroup(a, b);
            if (canGroup && a.Device is AMDDevice aDev && b.Device is AMDDevice bDev && aDev.OpenCLPlatformID != bDev.OpenCLPlatformID)
            {
                // OpenCLPlatorm IDs must match
                return false;
            }
            return canGroup;
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "sgminer.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }
    }
}
