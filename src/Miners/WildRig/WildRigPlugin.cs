using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WildRig
{
    public class WildRigPlugin : PluginBase, IDevicesCrossReference
    {
        public WildRigPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://bitcointalk.org/index.php?topic=5023676 | https://github.com/andru-kun/wildrig-multi/releases
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "0.19.3",
                ExePath = new List<string> { "wildrig.exe" },
                Urls = new List<string>
                {
                    "https://github.com/andru-kun/wildrig-multi/releases/download/0.19.3/wildrig-multi-windows-0.19.3-beta.7z", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "WildRig is multi algo miner for AMD devices.",
                SupportedDevicesAlgorithms = PluginSupportedAlgorithms.SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "2edd8080-9cb6-11e9-a6b8-09e27549d5bb";

        public override Version Version => new Version(3, 3);

        public override string Name => "WildRig";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        protected override MinerBase CreateMinerBase()
        {
            return new WildRig(PluginUUID, _mappedIDs);
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices
                .Where(dev => dev is AMDDevice gpu && Checkers.IsGcn2(gpu))
                .Cast<AMDDevice>()
                .OrderBy(amd => amd.PCIeBusID);

            var pcieId = 0;
            foreach (var gpu in amdGpus)
            {
                _mappedIDs[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetSupportedAlgorithms(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = PluginSupportedAlgorithms.GetSupportedAlgorithmsAMD(PluginUUID);
            if (PluginSupportedAlgorithms.UnsafeLimits(PluginUUID)) return algorithms;
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "wildrig.exe" });
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            var minerBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--print-devices");
            var mappedDevs = DevicesListParser.ParseWildRigOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedIDs[uuid] = indexID;
            }
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            // nothing new
            return false;
        }
    }
}
