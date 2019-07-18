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

namespace Phoenix
{
    public class PhoenixPlugin : PluginBase, IDevicesCrossReference
    {
        public PhoenixPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://bitcointalk.org/index.php?topic=2647654.0 current 4.2c
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/1.9.1.5/PhoenixMiner_4.2c_Windows.7z",
                    "https://mega.nz/#F!2VskDJrI!lsQsz1CdDe8x5cH3L8QaBw?6UV1FQSZ" // original
                }
            };
        }

        public override string PluginUUID => "ac9c763f-c901-41ef-9df1-c80099c9f942";

        public override Version Version => new Version(2, 0);
        public override string Name => "Phoenix";

        public override string Author => "domen.kirnkrefl@nicehash.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // map ids by bus ids
            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            int claymoreIndex = -1;
            foreach (var gpu in gpus)
            {
                _mappedIDs[gpu.UUID] = ++claymoreIndex;
            }

            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= new Version(377, 0);
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
            var isSupported = dev is AMDDevice gpu;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(IGpuDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 3;
            return isSupported && isDriverSupported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(IGpuDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto) { Enabled = false },
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new Phoenix(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedIDs.Count == 0) return;
            // TODO will block
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "-list");
            var mappedDevs = DevicesListParser.ParsePhoenixOutput(output, devices);

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedIDs[uuid] = indexID;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "PhoenixMiner.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            // error/bug in v1.0
            // because the previous miner plugin mapped wrong GPU indexes rebench everything
            return false;
        }
    }
}
