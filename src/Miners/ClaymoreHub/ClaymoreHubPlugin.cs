using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClaymoreHub
{

    public class ClaymoreHubPlugin : IMinerPlugin, IInitInternals
    {
        public string PluginUUID => "5e3b699e-2755-499c-bf4e-20d4aaef73df";

        public Version Version => new Version(1, 0);

        public string Name => "ClaymoreHub";

        public string Author => "Domen Kirn Krefl";


        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            var minDrivers = new Version(398, 26);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algorithms = GetSupportedAlgorithms(gpu).ToList();
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice gpu)
        {
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak);
        }
        private IEnumerable<Algorithm> GetSupportedAlgorithms(AMDDevice gpu)
        {
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s);
            yield return new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak);
        }

        public IMiner CreateMiner()
        {
            return new Claymore(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        protected static MinerOptionsPackage _minerOptionsPackage = Claymore.DefaultMinerOptionsPackage;

        #endregion Internal Settings
    }

}
