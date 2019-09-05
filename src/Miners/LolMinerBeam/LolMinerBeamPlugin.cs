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

namespace LolMinerBeam
{
    public class LolMinerBeamPlugin : PluginBase, IDevicesCrossReference
    {
        public LolMinerBeamPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
            // https://github.com/Lolliedieb/lolMiner-releases/releases | https://bitcointalk.org/index.php?topic=4724735.0 current 0.8.8
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                Urls = new List<string>
                {
                    "https://github.com/Lolliedieb/lolMiner-releases/releases/download/0.8.8/lolMiner_v088_Win64.zip", // original source
                }
            };
        }

        public override Version Version => new Version(2, 4);

        public override string Name => "LolMinerBeam";

        public override string Author => "domen.kirnkrefl@nicehash.com";

        public override string PluginUUID => "435f0820-7237-11e9-b20c-f9f12eb6d835";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // NVIDIA backend is NOT CUDA but OpenCL!!!!
            //CUDA 9.0+: minimum drivers 384.xx
            var minDrivers = new Version(384, 0);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;

            var gpus = devices
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported))
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            var pcieId = 0;
            foreach (var gpu in gpus)
            {
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu as BaseDevice, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(BaseDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 2 && gpu.IsOpenCLBackendEnabled;
            return isSupported && isDriverSupported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(IGpuDevice gpu)
        {
            var isAMD = gpu is AMDDevice;
            List<Algorithm> algorithms;
            if (isAMD)
            {
                algorithms = new List<Algorithm>
                {
                    new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31),
                    new Algorithm(PluginUUID, AlgorithmType.GrinCuckarood29),
                    new Algorithm(PluginUUID, AlgorithmType.BeamV2),
                };
            }
            else
            {
                // NVIDIA OpenCL backend stability is questionable
                algorithms = new List<Algorithm>
                {
                    new Algorithm(PluginUUID, AlgorithmType.GrinCuckatoo31) { Enabled = false },
                    new Algorithm(PluginUUID, AlgorithmType.GrinCuckarood29) { Enabled = false },
                    new Algorithm(PluginUUID, AlgorithmType.BeamV2) { Enabled = false },
                };
            }
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new LolMinerBeam(PluginUUID, _mappedDeviceIds);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            // TODO will block
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return;
            var minerBinPath = miner.GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "--benchmark BEAM --longstats 60 --devices -1", new List<string> { "Start Benchmark..." });
            var mappedDevs = DevicesListParser.ParseLolMinerOutput(output, devices.ToList());

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "lolMiner.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (ids.Count() == 0) return false;
            if (benchmarkedPluginVersion.Major == 2 && benchmarkedPluginVersion.Minor < 4)
            {
                if (device.DeviceType == DeviceType.NVIDIA && ids.FirstOrDefault() == AlgorithmType.BeamV2) return true;
            }
            return false;
        }
    }
}
