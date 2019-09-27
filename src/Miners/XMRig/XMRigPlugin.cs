using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XMRig
{
    public class XMRigPlugin : PluginBase
    {
        public XMRigPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://github.com/xmrig/xmrig current 3.1.3
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "3.1.3-msvc-win64",
                ExePath = new List<string> { "xmrig-3.1.3", "xmrig.exe" },
                Urls = new List<string>
                {
                    "https://github.com/xmrig/xmrig/releases/download/v3.1.3/xmrig-3.1.3-msvc-win64.zip" // original
                }
            };
        }

        public override string PluginUUID => "1046ea50-c261-11e9-8e4e-bb1e2c6e76b4";

        public override Version Version => new Version(3, 0);

        public override string Name => "XMRig";

        public override string Author => "domen.kirnkrefl@nicehash.com";

        protected override MinerBase CreateMinerBase()
        {
            return new XMRig(PluginUUID);
        }
        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var cpus = devices.Where(dev => dev is CPUDevice).Cast<CPUDevice>();
            foreach (var cpu in cpus) {
                supported.Add(cpu, new List<Algorithm>
                {
                    new Algorithm(PluginUUID, AlgorithmType.CryptoNightR)
                });
            }

            return supported;
        }
        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "xmrig.exe" });
        }
        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }
    }
}
