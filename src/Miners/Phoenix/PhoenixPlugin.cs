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
using System.Text;
using System.Threading.Tasks;

namespace Phoenix
{
    public class PhoenixPlugin : IMinerPlugin, IInitInternals
    {
        public PhoenixPlugin(string pluginUUID = "ac9c763f-c901-41ef-9df1-c80099c9f942")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 0);
        public string Name => "Phoenix";

        public string Author => "Domen Kirn Krefl";

        protected readonly Dictionary<int, int> _mappedIDs = new Dictionary<int, int>();

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 377.xx
            var minDrivers = new Version(377, 0);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var supportedGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 3 || dev is AMDDevice)
                .Cast<IGpuDevice>()
                .OrderBy(dev => dev.PCIeBusID)
                .ToList();

            var pcieID = 0;
            foreach (var gpu in supportedGpus)
            {
                _mappedIDs[gpu.PCIeBusID] = pcieID++;
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0 && gpu is CUDADevice cuda) supported.Add(cuda, algos);
                if (algos.Count > 0 && gpu is AMDDevice amd) supported.Add(amd, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(IGpuDevice gpu)
        {
            var algorithms = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto),
            };
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsList(gpu.GpuRam, algorithms);
            return filteredAlgorithms;
        }

        public IMiner CreateMiner()
        {
            return new Phoenix(PluginUUID, _mappedIDs)
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

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption> {}
        };
        #endregion Internal Settings
    }
}
