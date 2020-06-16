using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TRex
{
    public partial class TRexPlugin : PluginBase
    {
        public TRexPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            MinerBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://github.com/trexminer/T-Rex/releases 
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "0.15.7",
                ExePath = new List<string> { "t-rex.exe" },
                Urls = new List<string>
                {
                    "https://github.com/trexminer/T-Rex/releases/download/0.15.7/t-rex-0.15.7-win-cuda10.0.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "T-Rex is a versatile cryptocurrency mining software for NVIDIA devices.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "03f80500-94ec-11ea-a64d-17be303ea466";

        public override Version Version => new Version(11, 1);

        public override string Name => "TRex";

        public override string Author => "info@nicehash.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var isDriverSupported = Checkers.IsCudaCompatibleDriver(Checkers.CudaVersion.CUDA_10_0_130, CUDADevice.INSTALLED_NVIDIA_DRIVERS);
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 3 && isDriverSupported).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithmsForDevice(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new TRex(PluginUUID);
        }        

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "t-rex.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {}
            catch(Exception ex)
            {}
            return false;
        }
    }
}
