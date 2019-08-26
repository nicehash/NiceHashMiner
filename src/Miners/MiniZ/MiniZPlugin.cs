using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniZ
{
    public class MiniZPlugin : PluginBase, IDevicesCrossReference
    {
        // TODO CROSS REFF ko developerji fixanjo svoj --cuda-info output
        public MiniZPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://miniz.ch/usage/#command-line-arguments | https://miniz.ch/download/#latest-version current v1.5q2
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/1.9.1.12b/miniZ.zip",
                    "https://miniz.ch/?smd_process_download=1&download_id=2874", // original
                }
            };
        }
        public override string PluginUUID => "59bba2c0-b1ef-11e9-8e4e-bb1e2c6e76b4";

        public override Version Version => new Version(1,3);

        public override string Name => "MiniZ";

        public override string Author => "domen.kirnkrefl@nicehash.com";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        protected override MinerBase CreateMinerBase()
        {
            return new MiniZ(PluginUUID, _mappedDeviceIds);
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 411.31 - CUDA 10.0
            var minDrivers = new Version(411, 31);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice)
                .Cast<CUDADevice>();

            var pcieId = 0;
            foreach (var gpu in cudaGpus)
            {
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.ZHash),
                new Algorithm(PluginUUID, AlgorithmType.Beam),
                new Algorithm(PluginUUID, AlgorithmType.BeamV2)
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }
        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            // TODO will block
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "-ci");
            var mappedDevs = DevicesListParser.ParseMiniZOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedDeviceIds[uuid] = indexID;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "miniZ.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //no new version available
            return false;
        }
    }
}
