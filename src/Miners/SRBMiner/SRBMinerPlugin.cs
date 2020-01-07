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
using System.Threading.Tasks;

namespace SRBMiner
{
    public partial class SRBMinerPlugin : PluginBase, IDevicesCrossReference
    {
        public SRBMinerPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://www.srbminer.com/download.html current v1.9.3
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.9.3",
                ExePath = new List<string> { "SRBMiner-CN-V1-9-3", "SRBMiner-CN.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/v1.0/SRBMiner-CN-V1-9-3.7z",                                                 
                    "https://mega.nz/#F!qVIgxAwB!kKmgCDICmQwbdVvMb-tAag?WQggXSQa", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "SRBMiner AMD GPU Miner is a Windows software made for mining cryptocurrencies based on Cryptonight algorithm.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(5, 0);

        public override string Name => "SRBMiner";

        public override string Author => "info@nicehash.com";

        public override string PluginUUID => "85f507c0-b2ba-11e9-8e4e-bb1e2c6e76b4";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // Get AMD GCN2+
            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn2(gpu)).Cast<AMDDevice>();

            int indexAMD = -1;
            foreach (var gpu in amdGpus.Where(gpu => gpu is AMDDevice))
            {
                _mappedDeviceIds[gpu.UUID] = ++indexAMD;
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

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
            return new SRBMiner(PluginUUID, _mappedDeviceIds);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            // TODO will block
            var minerBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--listdevices");
            var mappedDevs = DevicesListParser.ParseSRBMinerOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedDeviceIds[uuid] = indexID;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "SRBMiner-CN.exe", "WinIo64.sys" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            // no new version
            return false;
        }
    }
}
