using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1;

namespace CpuMinerOpt
{
    public class CPUMinerPlugin : PluginBase
    {
        public CPUMinerPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            // https://bitcointalk.org/index.php?topic=1326803.0 | https://github.com/JayDDee/cpuminer-opt/releases current v3.9.7
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                Urls = new List<string>
                {
                    "https://github.com/JayDDee/cpuminer-opt/releases/download/v3.9.7/cpuminer-opt-3.9.7-windows.zip", // original
                }
            };
        }

        public override string PluginUUID => "92fceb00-7236-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(2, 0);

        public override string Name => "cpuminer-opt";

        public override string Author => "stanko@nicehash.com";

        protected override MinerBase CreateMinerBase()
        {
            return new CpuMiner(PluginUUID);
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var cpus = devices.Where(dev => dev is CPUDevice).Select(dev => (CPUDevice)dev);
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var cpu in cpus)
            {
                supported.Add(cpu, GetSupportedAlgorithms());
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms()
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.Lyra2Z),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
                new Algorithm(PluginUUID, AlgorithmType.X16R)
            };
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> {
                "cpuminer-avx2.exe", "cpuminer-zen.exe",
                "libcrypto-1_1-x64.dll", "libcurl-4.dll", "libgcc_s_seh-1.dll", "libstdc++-6.dll", "libwinpthread-1.dll", "zlib1.dll"
            });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }
    }
}
