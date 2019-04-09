using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XmrStak.Configs;

namespace XmrStak
{
    public class XmrStakPlugin : IMinerPlugin, IInitInternals, IXmrStakConfigHandler
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

        protected Dictionary<string, DeviceType> _registeredDeviceUUIDTypes;
        protected HashSet<AlgorithmType> _registeredAlgorithmTypes;
        protected Dictionary<bool, Dictionary<string, AlgorithmType>> _configExists;

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
                if (algorithms.Count > 0)
                {
                    supported.Add(dev, algorithms);
                    _registeredDeviceUUIDTypes.Add(dev.UUID, dev.DeviceType);
                    foreach (var algorithm in algorithms)
                    {
                        _registeredAlgorithmTypes.Add(algorithm.FirstAlgorithmType);
                    }
                }
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
            return new XmrStak(PluginUUID, this);
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

            var configsRoot = Path.Combine(pluginRoot, "configsOrWhatever");
            foreach (string file in Directory.GetFiles(configsRoot, "*.json"))
            {
                foreach(var device in _registeredDeviceUUIDTypes)
                {
                    foreach(var algorithm in _registeredAlgorithmTypes)
                    {

                    }
                }
            }
        }

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {};

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {};
        #endregion Internal settings



        #region Cached configs
        protected Dictionary<AlgorithmType, CpuConfig> _cpuConfigs = new Dictionary<AlgorithmType, CpuConfig>();
        protected Dictionary<AlgorithmType, AmdConfig> _amdConfigs = new Dictionary<AlgorithmType, AmdConfig>();
        protected Dictionary<AlgorithmType, NvidiaConfig> _nvidiaConfigs = new Dictionary<AlgorithmType, NvidiaConfig>();


        public bool HasConfig(DeviceType deviceType, AlgorithmType algorithmType)
        {
            switch (deviceType)
            {
                case DeviceType.CPU:
                    return _cpuConfigs.ContainsKey(algorithmType);
                case DeviceType.AMD:
                    return _amdConfigs.ContainsKey(algorithmType);
                case DeviceType.NVIDIA:
                    return _nvidiaConfigs.ContainsKey(algorithmType);
            }
            return false;
        }

        public void SaveMoveConfig(DeviceType deviceType, AlgorithmType algorithmType, string sourcePath)
        {
            string destinationPath = Path.Combine(Paths.MinerPluginsPath(), PluginUUID, "configs", $"{algorithmType.ToString()}_{deviceType.ToString()}.txt");
            try
            {
                var dirPath = Path.GetDirectoryName(destinationPath);
                if (Directory.Exists(dirPath) == false)
                {
                    Directory.CreateDirectory(dirPath);
                }

                var readConfigContent = File.ReadAllText(sourcePath);
                // make it JSON 
                readConfigContent = "{" + readConfigContent + "}";
                // remove old if any
                if (File.Exists(destinationPath)) File.Delete(destinationPath);
                // move to path
                File.Move(sourcePath, destinationPath);

                //TODO load and save 
                switch(deviceType)
                {
                    case DeviceType.CPU:
                        if (GetCpuConfig(algorithmType) == null) {
                            CpuConfig cpuConfig = JsonConvert.DeserializeObject<CpuConfig>(readConfigContent);
                            _cpuConfigs[algorithmType] = cpuConfig;
                        }

                        break;
                    case DeviceType.AMD:
                        if (GetAmdConfig(algorithmType) == null)
                        {
                            AmdConfig amdConfig = JsonConvert.DeserializeObject<AmdConfig>(readConfigContent);
                            _amdConfigs[algorithmType] = amdConfig;
                        }
                        break;
                    case DeviceType.NVIDIA:
                        if (GetNvidiaConfig(algorithmType) == null)
                        {
                            NvidiaConfig nvidiaConfig = JsonConvert.DeserializeObject<NvidiaConfig>(readConfigContent);
                            _nvidiaConfigs[algorithmType] = nvidiaConfig;
                        }
                        break;
                }
            }
            catch (Exception)
            { }
        }

        public CpuConfig GetCpuConfig(AlgorithmType algorithmType)
        {
            return _cpuConfigs[algorithmType];
        }

        public AmdConfig GetAmdConfig(AlgorithmType algorithmType)
        {
            return _amdConfigs[algorithmType];
        }

        public NvidiaConfig GetNvidiaConfig(AlgorithmType algorithmType)
        {
            return _nvidiaConfigs[algorithmType];
        }

        #endregion Cached configs
    }
}
