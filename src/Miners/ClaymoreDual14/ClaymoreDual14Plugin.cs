using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ClaymoreCommon;
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
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
        }

        public override string PluginUUID => "78d0bd8b-4d8f-4b7e-b393-e8ac6a83ae76";

        public override Version Version => new Version(2, 0);

        public override string Name => "ClaymoreDual14+";

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
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto),
            // duals disabled by default
#pragma warning disable 0618
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s) {Enabled = false },
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak) {Enabled = false },
#pragma warning restore 0618
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new ClaymoreDual14(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedIDs.Count == 0) return;
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var minerCwd = miner.GetBinAndCwdPaths().Item2;
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
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
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
            // error/bug in v1.0
            // because the previous miner plugin mapped wrong GPU indexes rebench everything
            try
            {
                var reBench = benchmarkedPluginVersion.Major == 1 && benchmarkedPluginVersion.Minor < 2;
                return reBench;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
