using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BMiner
{
    public class BMinerPlugin : PluginBase
    {
        // mandatory constructor
        public BMinerPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://www.bminer.me/releases/ current v 15.7.6
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "15.7.6",
                ExePath = new List<string> { "bminer-lite-v15.7.6-f585663", "bminer.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/1.9.1.12/bminer-lite-v15.7.6-f585663.zip",
                    "https://www.bminercontent.com/releases/bminer-lite-v15.7.6-f585663-amd64.zip" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Bminer is a cryptocurrency miner that runs on modern AMD / NVIDIA GPUs.",
                SupportedDevicesAlgorithms = PluginSupportedAlgorithms.SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "e5fbd330-7235-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(3, 1);
        public override string Name => "BMiner";

        public override string Author => "info@nicehash.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();
            foreach (var gpu in amdGpus)
            {
                var algorithms = GetAMDSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }
            // CUDA 9.2+ driver 397.44
            var mininumRequiredDriver = new Version(397, 44);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS >= mininumRequiredDriver)
            {
                var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();
                foreach (var gpu in cudaGpus)
                {
                    var algos = GetCUDASupportedAlgorithms(gpu).ToList();
                    if (algos.Count > 0) supported.Add(gpu, algos);
                }
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetCUDASupportedAlgorithms(CUDADevice gpu)
        {
            var algorithms = PluginSupportedAlgorithms.GetSupportedAlgorithmsNVIDIA(PluginUUID).ToList();
            if (PluginSupportedAlgorithms.UnsafeLimits(PluginUUID)) return algorithms;
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        private IEnumerable<Algorithm> GetAMDSupportedAlgorithms(AMDDevice gpu)
        {
            var algorithms = PluginSupportedAlgorithms.GetSupportedAlgorithmsAMD(PluginUUID).ToList();
            if (PluginSupportedAlgorithms.UnsafeLimits(PluginUUID)) return algorithms;
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new BMiner(PluginUUID, PluginSupportedAlgorithms.AlgorithmName, PluginSupportedAlgorithms.DevFee);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "bminer.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() == 0) return false;
                if (benchmarkedPluginVersion.Major == 2 && benchmarkedPluginVersion.Minor < 3)
                {
                    // v15.7.6 https://www.bminercontent.com/releases/bminer-lite-v15.7.6-f585663-amd64.zip
                    if (ids.FirstOrDefault() == AlgorithmType.GrinCuckatoo31) return true;
                    if (device.Name.Contains("RTX") && ids.FirstOrDefault() == AlgorithmType.GrinCuckarood29) return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
