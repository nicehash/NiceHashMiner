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

namespace XmrStak
{
    public class XmrStakPlugin : IMinerPlugin, IInitInternals
    {
        public XmrStakPlugin(string pluginUUID = "b4cf2181-ca66-4d9c-83ba-cd5a7c6a7499")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 0);
        public string Name => "XmrStak";

        public string Author => "stanko@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var devicesToAdd = new List<BaseDevice>();
            // AMD case check if we should check Gcn4
            var amdGpus = devices.Where(dev => dev is AMDDevice /* amd && Checkers.IsGcn4(amd)*/).Cast<AMDDevice>(); 
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 5).Cast<CUDADevice>();
            var cpus = devices.Where(dev => dev is CPUDevice).Cast<CPUDevice>();

            // CUDA 9.2+ driver 397.44
            var mininumRequiredDriver = new Version(397, 44);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS >= mininumRequiredDriver)
            {
                devicesToAdd.AddRange(cudaGpus);
            }
            devicesToAdd.AddRange(amdGpus);
            devicesToAdd.AddRange(cpus);

            // CPU 
            foreach (var dev in devicesToAdd)
            {
                var algorithms = GetSupportedAlgorithms(dev);
                if (algorithms.Count > 0) supported.Add(dev, algorithms);
            }


            return supported;
        }

        private List<Algorithm> GetSupportedAlgorithms(BaseDevice dev)
        {
            // multiple OpenCL GPUs seem to freeze the whole system
            var AMD_DisabledByDefault = dev.DeviceType != DeviceType.AMD;
            var algos = new List<Algorithm>
            {
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightHeavy) { Enabled = AMD_DisabledByDefault },
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightV8) { Enabled = AMD_DisabledByDefault },
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightR) { Enabled = AMD_DisabledByDefault },
            };
            return algos;
        }

        public IMiner CreateMiner()
        {
            return new XmrStak(PluginUUID);
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }

        // these here are slightly different
        #region Internal settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;
        }

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {
            // we have same env vars for all miners now, check avemore env vars if they differ and use custom env vars instead of defaults
            DefaultSystemEnvironmentVariables = new Dictionary<string, string>()
            {
                { "XMRSTAK_NOWAIT", "1" }
            },
        };

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        { };
        #endregion Internal settings
    }
}
