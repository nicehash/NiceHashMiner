using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class CCMinerMTPIntegratedPlugin : CCMinersPluginBase, IGetApiMaxTimeoutV2
    {
        public override string PluginUUID => "CCMinerMTP";

        public override Version Version => new Version(1, 0);

        public override string Name => "CCMinerMTP";

        protected override string DirPath => "ccminer_mtp";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var reqCudaVer = Checkers.CudaVersion.CUDA_10_0_130;
            var isCompatible = Checkers.IsCudaCompatibleDriver(reqCudaVer, CUDADevice.INSTALLED_NVIDIA_DRIVERS);
            if (!isCompatible) return supported; // return emtpy

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = new List<Algorithm> {
                    new Algorithm(PluginUUID, AlgorithmType.MTP) { Enabled = false }
                };
                supported.Add(gpu, algorithms);
            }

            return supported;
        }

        #region IGetApiMaxTimeoutV2
        public override bool IsGetApiMaxTimeoutEnabled
        {
            get
            {
                if (_getApiMaxTimeoutConfig?.UseUserSettings ?? false) return _getApiMaxTimeoutConfig.Enabled;
                return true;
            }
        }
        public new TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            return MinerToolkit.ParseApiMaxTimeoutConfig(new TimeSpan(0,2,0), _getApiMaxTimeoutConfig, miningPairs);
        }
        #endregion IGetApiMaxTimeoutV2
    }
}
