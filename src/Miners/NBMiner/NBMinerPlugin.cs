using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;

namespace NBMiner
{
    public class NBMinerPlugin : PluginBase, IDevicesCrossReference
    {
        public NBMinerPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            // https://github.com/NebuTech/NBMiner/releases/ current 24.1 // TODO new version
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                Urls = new List<string>
                {
                    "https://github.com/NebuTech/NBMiner/releases/download/v24.1/NBMiner_24.1_Win.zip", // original
                }
            };
        }

        public override string PluginUUID => "6c07f7a0-7237-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(2, 1);
        public override string Name => "NBMiner";

        public override string Author => "Dillon Newell";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        private bool isSupportedVersion(int major, int minor)
        {
            var nbMinerSMSupportedVersions = new List<Version>
            {
                new Version(6,0),
                new Version(6,1),
                new Version(7,0),
                new Version(7,5),
            };
            var cudaDevSMver = new Version(major, minor);
            foreach (var supportedVer in nbMinerSMSupportedVersions)
            {
                if (supportedVer == cudaDevSMver) return true;
            }
            return false;
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 377.xx
            var minDrivers = new Version(377, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && isSupportedVersion(gpu.SM_major, gpu.SM_minor))
                .Cast<CUDADevice>()
                .OrderBy(dev => dev.PCIeBusID)
                .ToList();

            var pcieID = 0;
            foreach (var gpu in cudaGpus)
            {
                _mappedIDs[gpu.UUID] = pcieID;
                ++pcieID;
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                //new Algorithm(PluginUUID, AlgorithmType.GrinCuckaroo29),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                new Algorithm(PluginUUID, AlgorithmType.CuckooCycle),
                new Algorithm(PluginUUID, AlgorithmType.GrinCuckarood29),
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new NBMiner(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedIDs.Count == 0) return;
            // TODO will break
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--device-info-json -RUN");
            var mappedDevs = DevicesListParser.ParseNBMinerOutput(output, devices.ToList());

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nbminer.exe", "OhGodAnETHlargementPill-r2.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            return false;
        }
    }
}
