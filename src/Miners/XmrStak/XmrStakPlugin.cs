using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XmrStak.Configs;

namespace XmrStak
{
    public class XmrStakPlugin : PluginBase, IXmrStakConfigHandler
    {
        public XmrStakPlugin()
        {
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            MinerSystemEnvironmentVariables = PluginInternalSettings.MinerSystemEnvironmentVariables;
        }

        public override string PluginUUID => "3d4e56b0-7238-11e9-b20c-f9f12eb6d835";

        public override Version Version => new Version(2, 0);
        public override string Name => "XmrStak";

        public override string Author => "stanko@nicehash.com";

        protected Dictionary<string, DeviceType> _registeredDeviceUUIDTypes = new Dictionary<string, DeviceType>();
        protected HashSet<AlgorithmType> _registeredAlgorithmTypes = new HashSet<AlgorithmType>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var devicesToAdd = new List<BaseDevice>();
            // AMD case check if we should check Gcn4
            var amdGpus = devices.Where(dev => dev is AMDDevice /* amd && Checkers.IsGcn4(amd)*/).Cast<AMDDevice>(); 
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 3).Cast<CUDADevice>();
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
                    _registeredDeviceUUIDTypes[dev.UUID] = dev.DeviceType;
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
                new Algorithm(PluginUUID, AlgorithmType.CryptoNightR) { Enabled = AMD_DisabledByDefault },
            };
            return algos;
        }

        protected override MinerBase CreateMinerBase()
        {
            return new XmrStak(PluginUUID, this);
        }

        public override bool CanGroup(MiningPair a, MiningPair b)
        {
            var canGroup = base.CanGroup(a, b);
            if (canGroup && a.Device is AMDDevice aDev && b.Device is AMDDevice bDev && aDev.OpenCLPlatformID != bDev.OpenCLPlatformID)
            {
                // OpenCLPlatorm IDs must match
                return false;
            }
            return canGroup;
        }

        private string GetMinerConfigsRoot()
        {
            return Path.Combine(Paths.MinerPluginsPath(), PluginUUID, "configs");
        }

        // these here are slightly different
        #region Internal settings
        public override void InitInternals()
        {
            base.InitInternals();

            var minerConfigPath = GetMinerConfigsRoot();
            if (!Directory.Exists(minerConfigPath)) return; // no settings

            var configFiles = Directory.GetFiles(minerConfigPath, "cached_*.json");
            var registeredDeviceTypes = _registeredDeviceUUIDTypes.Select(kvp => kvp.Value).Distinct();

            foreach (var deviceType in registeredDeviceTypes)
            {
                var uuids = _registeredDeviceUUIDTypes.Where(kvp => kvp.Value == deviceType).Select(kvp => kvp.Key);
                foreach (var algorithm in _registeredAlgorithmTypes)
                {
                    var cachedConfig = $"{algorithm.ToString()}_{deviceType.ToString()}";
                    var cachedConfigPath = configFiles.Where(path => path.Contains(cachedConfig)).FirstOrDefault();
                    if (string.IsNullOrEmpty(cachedConfigPath)) continue;

                    var cachedConfigContent = File.ReadAllText(cachedConfigPath);
                    try
                    {
                        switch (deviceType)
                        {
                            case DeviceType.CPU:
                                var cpuConfig = JsonConvert.DeserializeObject<CachedCpuSettings>(cachedConfigContent);
                                var isCpuSame = uuids.Except(cpuConfig.DeviceUUIDs).Count() == 0;
                                if (isCpuSame) SetCpuConfig(algorithm, cpuConfig.CachedConfig);
                                break;
                            case DeviceType.AMD:
                                var amdConfig = JsonConvert.DeserializeObject<CachedAmdSettings>(cachedConfigContent);
                                var isAmdSame = uuids.Except(amdConfig.DeviceUUIDs).Count() == 0;
                                if (isAmdSame) SetAmdConfig(algorithm, amdConfig.CachedConfig);
                                break;
                            case DeviceType.NVIDIA:
                                var nvidiaConfig = JsonConvert.DeserializeObject<CachedNvidiaSettings>(cachedConfigContent);
                                var isNvidiaSame = uuids.Except(nvidiaConfig.DeviceUUIDs).Count() == 0;
                                if (isNvidiaSame) SetNvidiaConfig(algorithm, nvidiaConfig.CachedConfig);
                                break;
                        }
                    }
                    catch (Exception)
                    { }
                }
            }
        }
        #endregion Internal settings



        #region Cached configs
        protected ConcurrentDictionary<AlgorithmType, CpuConfig> _cpuConfigs = new ConcurrentDictionary<AlgorithmType, CpuConfig>();
        protected ConcurrentDictionary<AlgorithmType, AmdConfig> _amdConfigs = new ConcurrentDictionary<AlgorithmType, AmdConfig>();
        protected ConcurrentDictionary<AlgorithmType, NvidiaConfig> _nvidiaConfigs = new ConcurrentDictionary<AlgorithmType, NvidiaConfig>();


        public bool HasConfig(DeviceType deviceType, AlgorithmType algorithmType)
        {
            switch (deviceType)
            {
                case DeviceType.CPU:
                    return GetCpuConfig(algorithmType) != null;
                case DeviceType.AMD:
                    return GetAmdConfig(algorithmType) != null;
                case DeviceType.NVIDIA:
                    return GetNvidiaConfig(algorithmType) != null;
            }
            return false;
        }

        private object _configsLock = new object();

        public void SaveMoveConfig(DeviceType deviceType, AlgorithmType algorithmType, string sourcePath)
        {
            lock(_configsLock)
            {
                try
                {
                    string destinationPath = Path.Combine(GetMinerConfigsRoot(), $"{algorithmType.ToString()}_{deviceType.ToString()}.txt");
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

                    var cachedFileSettings = $"cached_{algorithmType.ToString()}_{deviceType.ToString()}.json";
                    var cachedFileSettingsPath = Path.Combine(GetMinerConfigsRoot(), cachedFileSettings);
                    var uuids = _registeredDeviceUUIDTypes.Where(kvp => kvp.Value == deviceType).Select(kvp => kvp.Key).ToList();
                    object cachedSettings = null;
                    //TODO load and save 
                    switch (deviceType)
                    {
                        case DeviceType.CPU:
                            var cpuConfig = JsonConvert.DeserializeObject<CpuConfig>(readConfigContent);
                            SetCpuConfig(algorithmType, cpuConfig);
                            cachedSettings = new CachedCpuSettings
                            {
                                CachedConfig = cpuConfig,
                                DeviceUUIDs = uuids
                            };
                            break;
                        case DeviceType.AMD:
                            var amdConfig = JsonConvert.DeserializeObject<AmdConfig>(readConfigContent);
                            SetAmdConfig(algorithmType, amdConfig);
                            cachedSettings = new CachedAmdSettings
                            {
                                CachedConfig = amdConfig,
                                DeviceUUIDs = uuids
                            };
                            break;
                        case DeviceType.NVIDIA:
                            var nvidiaConfig = JsonConvert.DeserializeObject<NvidiaConfig>(readConfigContent);
                            SetNvidiaConfig(algorithmType, nvidiaConfig);
                            cachedSettings = new CachedNvidiaSettings
                            {
                                CachedConfig = nvidiaConfig,
                                DeviceUUIDs = uuids
                            };
                            break;
                    }
                    if (cachedSettings != null)
                    {
                        var header = "// This config file was autogenerated by NHML.";
                        header += "\n// \"DeviceUUIDs\" is used to check if we have same devices and should not be edited.";
                        header += "\n// \"CachedConfig\" can be edited as it is used as config template (edit this only if you know what you are doing)";
                        header += "\n// If \"DeviceUUIDs\" is different (new devices added or old ones removed) this file will be overwritten and \"CachedConfig\" will be set to defaults.";
                        header += "\n\n";
                        var jsonText = JsonConvert.SerializeObject(cachedSettings, Formatting.Indented);
                        var headerWithConfigs = header + jsonText;
                        InternalConfigs.WriteFileSettings(cachedFileSettingsPath, headerWithConfigs);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("XmrStakPlugin", $"SaveMoveConfig error: {e.Message}");
                }
            }
        }

        public CpuConfig GetCpuConfig(AlgorithmType algorithmType)
        {
            CpuConfig config = null;
            _cpuConfigs.TryGetValue(algorithmType, out config);
            return config;
        }

        public AmdConfig GetAmdConfig(AlgorithmType algorithmType)
        {
            AmdConfig config = null;
            _amdConfigs.TryGetValue(algorithmType, out config);
            return config;
        }

        public NvidiaConfig GetNvidiaConfig(AlgorithmType algorithmType)
        {
            NvidiaConfig config = null;
            _nvidiaConfigs.TryGetValue(algorithmType, out config);
            return config;
        }

        public void SetCpuConfig(AlgorithmType algorithmType, CpuConfig conf)
        {
            if (HasConfig(DeviceType.CPU, algorithmType)) return;
            _cpuConfigs.TryAdd(algorithmType, conf);
        }

        public void SetAmdConfig(AlgorithmType algorithmType, AmdConfig conf)
        {
            if (HasConfig(DeviceType.AMD, algorithmType)) return;
            _amdConfigs.TryAdd(algorithmType, conf);
        }

        public void SetNvidiaConfig(AlgorithmType algorithmType, NvidiaConfig conf)
        {
            if (HasConfig(DeviceType.NVIDIA, algorithmType)) return;
            _nvidiaConfigs.TryAdd(algorithmType, conf);
        }

        #endregion Cached configs

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "concrt140.dll", "libeay32.dll", "msvcp140.dll", "msvcp140_1.dll",
                "msvcp140_2.dll", "nvrtc-builtins64_100.dll", "nvrtc-builtins64_90.dll", "nvrtc64_100_0.dll", "nvrtc64_90.dll", "ssleay32.dll", "vccorlib140.dll", "vcruntime140.dll",
                "xmr-stak.exe", "xmrstak_cuda_backend.dll", "xmrstak_cuda_backend_cuda10_0.dll", "xmrstak_opencl_backend.dll"
            });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (benchmarkedPluginVersion.Major == 1 && benchmarkedPluginVersion.Minor == 1)
            {
                return true;
            }
            //no new version available
            return false;
        }
    }
}
