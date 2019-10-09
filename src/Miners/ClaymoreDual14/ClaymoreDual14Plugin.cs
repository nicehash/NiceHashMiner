using MinerPluginToolkitV1;
using MinerPluginToolkitV1.ClaymoreCommon;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ClaymoreDual14
{
    public class ClaymoreDual14Plugin : PluginBase, IDevicesCrossReference
    {
        public ClaymoreDual14Plugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://bitcointalk.org/index.php?topic=1433925.0 current v15.0
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v15.0",
                ExePath = new List<string> { "Claymore's Dual Ethereum AMD+NVIDIA GPU Miner v15.0", "EthDcrMiner64.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/1.9.1.12b/Claymore.s.Dual.Ethereum.AMD+NVIDIA.GPU.Miner.v15.0.zip",
                    "https://mega.nz/#F!O4YA2JgD!n2b4iSHQDruEsYUvTQP5_w?64RGzCIb" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Miner for AMD and NVIDIA cards, supporting Dual mining.",
                SupportedDevicesAlgorithms = PluginSupportedAlgorithms.SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "70984aa0-7236-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(3, 1);

        public override string Name => "ClaymoreDual";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // map ids by bus ids
            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            var amds = gpus.Where(gpu => gpu is AMDDevice).OrderBy(gpu => gpu.PCIeBusID);
            var nvidias = gpus.Where(gpu => gpu is CUDADevice).OrderBy(gpu => gpu.PCIeBusID);

            // claymore orders AMDs first
            int claymoreIndex = -1;
            foreach (var amd in amds)
            {
                _mappedIDs[amd.UUID] = ++claymoreIndex;
            }
            foreach (var nvidia in nvidias)
            {
                _mappedIDs[nvidia.UUID] = ++claymoreIndex;
            }

            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= new Version(411, 31);
            var supportedGpus = gpus.Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported));

            foreach (var gpu in supportedGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
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

        private IEnumerable<Algorithm> GetSupportedAlgorithms(IGpuDevice gpu)
        {
            var algorithms = PluginSupportedAlgorithms.GetSupportedAlgorithmsGPU(PluginUUID).ToList();
            if (PluginSupportedAlgorithms.UnsafeLimits(PluginUUID)) return algorithms;
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            if(gpu is AMDDevice amd && (amd.Codename.ToLower().Contains("gfx10") || amd.Name.ToLower().Contains("navi")))
            {
                filteredAlgorithms = filteredAlgorithms.Where(algo => algo.IDs.Count == 1).ToList();
            }
            return filteredAlgorithms;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new ClaymoreDual14(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedIDs.Count == 0) return;
            var binAndCwdPaths = GetBinAndCwdPaths();
            var minerBinPath = binAndCwdPaths.Item1;
            var minerCwd = binAndCwdPaths.Item2;
            // no device list so 'start mining'
            var logFile = "noappend_cross_ref_devs.txt";
            var logFilePath = Path.Combine(minerCwd, logFile);
            var args = $"-mport 0 -benchmark 1 -wd 0 -colors 0 -dbg 1 -logfile {logFile}";
            var output = await MinerPluginToolkitV1.ClaymoreCommon.DevicesCrossReferenceHelpers.ReadLinesUntil(minerBinPath, minerCwd, args, logFilePath, new List<string> { "Total cards", "Stratum - connecting to" });
            var mappedDevs = DevicesListParser.ParseClaymoreDualOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedIDs[uuid] = indexID;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> {
                "cudart64_80.dll",
                "EthDcrMiner64.exe",
                "libcurl.dll",
                "msvcr110.dll",
                @"cuda10\cudart64_100.dll",
                @"cuda10\EthDcrMiner64.exe",
                @"RemoteManager\EthMan.exe",
                @"RemoteManager\libeay32.dll",
                @"RemoteManager\ssleay32.dll"
            });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }
    }
}
