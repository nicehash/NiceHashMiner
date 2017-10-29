﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using NiceHashMiner.Enums;
using System.Security.Cryptography;
using NiceHashMiner.Configs;
using NiceHashMiner.Configs.Data;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners;

namespace NiceHashMiner.Devices
{
    public class ComputeDevice
    {
        readonly public int ID;
        public int Index { get; protected set; }  // For socket control, unique
        // to identify equality;
        readonly public string Name; // { get; set; }
        // name count is the short name for displaying in moning groups
        readonly public string NameCount;
        public bool Enabled;

        readonly public DeviceGroupType DeviceGroupType;
        // CPU, NVIDIA, AMD
        readonly public DeviceType DeviceType;
        // UUID now used for saving
        public string UUID { get; protected set; }

        // used for Claymore indexing
        public int BusID { get; protected set; } = -1;
        public int IDByBus = -1;


        // CPU extras
        public int Threads { get; protected set; }
        public ulong AffinityMask { get; protected set; }

        // GPU extras
        public readonly ulong GpuRam;
        public readonly bool IsEtherumCapale;
        public static readonly ulong MEMORY_3GB = 3221225472;

        // sgminer extra quickfix
        //public readonly bool IsOptimizedVersion;
        public string Codename { get; protected set; }
        public string InfSection { get; protected set; }
        // amd has some algos not working with new drivers
        public bool DriverDisableAlgos { get; protected set; }

        protected List<Algorithm> AlgorithmSettings;

        public string BenchmarkCopyUUID { get; set; }

        public string TuningCopyUUID { get; set; }
        
        public virtual float Load { get { return 0; } }
        
        public virtual float Temp { get { return 0;  } }
        
        public virtual uint FanSpeed { get { return 0; } }

        // Ambiguous constructor
        protected ComputeDevice(int id, string name, bool enabled, DeviceGroupType group, bool ethereumCapable, DeviceType type, string nameCount, ulong gpuRAM) {
            ID = id;
            Name = name;
            Enabled = enabled;
            DeviceGroupType = group;
            IsEtherumCapale = ethereumCapable;
            DeviceType = type;
            NameCount = nameCount;
            GpuRam = gpuRAM;
        }

        // Fake dev
        public ComputeDevice(int id) {
            ID = id;
            Name = "fake_" + id;
            NameCount = Name;
            Enabled = true;
            DeviceType = DeviceType.CPU;
            DeviceGroupType = DeviceGroupType.NONE;
            IsEtherumCapale = false;
            //IsOptimizedVersion = false;
            Codename = "fake";
            UUID = GetUUID(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType);
            GpuRam = 0;
        }

        // CPU 
        public ComputeDevice(int id, string group, string name, int threads, ulong affinityMask, int CPUCount)
        {
            ID = id;
            Name = name;
            Threads = threads;
            AffinityMask = affinityMask;
            Enabled = true;
            DeviceGroupType = DeviceGroupType.CPU;
            DeviceType = DeviceType.CPU;
            NameCount = String.Format(International.GetText("ComputeDevice_Short_Name_CPU"), CPUCount);
            UUID = GetUUID(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType);
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            IsEtherumCapale = false;
            GpuRam = 0;
        }

        // GPU NVIDIA
        protected int _SM_major = -1;
        protected int _SM_minor = -1;
        public ComputeDevice(CudaDevice cudaDevice, DeviceGroupType group, int GPUCount) {
            _SM_major = cudaDevice.SM_major;
            _SM_minor = cudaDevice.SM_minor;
            ID = (int)cudaDevice.DeviceID;
            Name = cudaDevice.GetName();
            Enabled = true;
            DeviceGroupType = group;
            IsEtherumCapale = cudaDevice.IsEtherumCapable();
            DeviceType = DeviceType.NVIDIA;
            NameCount = String.Format(International.GetText("ComputeDevice_Short_Name_NVIDIA_GPU"), GPUCount);
            UUID = cudaDevice.UUID;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            GpuRam = cudaDevice.DeviceGlobalMemory;
        }

        public bool IsSM50() { return _SM_major == 5 && _SM_minor == 0; }

        // GPU AMD
        public ComputeDevice(AmdGpuDevice amdDevice, int GPUCount, bool isDetectionFallback) {
            ID = amdDevice.DeviceID;
            BusID = amdDevice.BusID;
            DeviceGroupType = DeviceGroupType.AMD_OpenCL;
            Name = amdDevice.DeviceName;
            Enabled = true;
            IsEtherumCapale = amdDevice.IsEtherumCapable();
            DeviceType = DeviceType.AMD;
            NameCount = String.Format(International.GetText("ComputeDevice_Short_Name_AMD_GPU"), GPUCount);
            if (isDetectionFallback) {
                UUID = GetUUID(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType);
            } else {
                UUID = amdDevice.UUID;
            }
            // sgminer extra
            //IsOptimizedVersion = amdDevice.UseOptimizedVersion;
            Codename = amdDevice.Codename;
            InfSection = amdDevice.InfSection;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            DriverDisableAlgos = amdDevice.DriverDisableAlgos;
            GpuRam = amdDevice.DeviceGlobalMemory;
        }

        // combines long and short name
        public string GetFullName() {
            return String.Format(International.GetText("ComputeDevice_Full_Device_Name"), NameCount, Name);
        }

        public Algorithm GetAlgorithm(Algorithm modelAlgo) {
            return GetAlgorithm(modelAlgo.MinerBaseType, modelAlgo.NiceHashID, modelAlgo.SecondaryNiceHashID);
        }
        public Algorithm GetAlgorithm(MinerBaseType MinerBaseType, AlgorithmType AlgorithmType, AlgorithmType SecondaryAlgorithmType) {
            int toSetIndex = this.AlgorithmSettings.FindIndex((a) => a.NiceHashID == AlgorithmType 
            && a.MinerBaseType == MinerBaseType 
            && a.SecondaryNiceHashID == SecondaryAlgorithmType);
            if (toSetIndex > -1) {
                return this.AlgorithmSettings[toSetIndex];
            }
            return null;
        }

        //public Algorithm GetAlgorithm(string algoID) {
        //    int toSetIndex = this.AlgorithmSettings.FindIndex((a) => a.AlgorithmStringID == algoID);
        //    if (toSetIndex > -1) {
        //        return this.AlgorithmSettings[toSetIndex];
        //    }
        //    return null;
        //}

        public void CopyBenchmarkSettingsFrom(ComputeDevice copyBenchCDev) {
            foreach (var copyFromAlgo in copyBenchCDev.AlgorithmSettings) {
                var setAlgo = GetAlgorithm(copyFromAlgo);
                if (setAlgo != null) {
                    setAlgo.BenchmarkSpeed = copyFromAlgo.BenchmarkSpeed;
                    setAlgo.ExtraLaunchParameters = copyFromAlgo.ExtraLaunchParameters;
                    setAlgo.LessThreads = copyFromAlgo.LessThreads;
                    if (setAlgo is DualAlgorithm dualSA && copyFromAlgo is DualAlgorithm dualCFA) {
                        dualSA.SecondaryBenchmarkSpeed = dualCFA.SecondaryBenchmarkSpeed;
                    }
                }
            }
        }

        public void CopyTuningSettingsFrom(ComputeDevice copyTuningCDev) {
            foreach (var copyFromAlgo in copyTuningCDev.AlgorithmSettings.OfType<DualAlgorithm>()) {
                if (GetAlgorithm(copyFromAlgo) is DualAlgorithm setAlgo) {
                    setAlgo.IntensitySpeeds = copyFromAlgo.IntensitySpeeds;
                    setAlgo.SecondaryIntensitySpeeds = copyFromAlgo.SecondaryIntensitySpeeds;
                    setAlgo.TuningStart = copyFromAlgo.TuningStart;
                    setAlgo.TuningEnd = copyFromAlgo.TuningEnd;
                    setAlgo.TuningInterval = copyFromAlgo.TuningInterval;
                    setAlgo.TuningEnabled = copyFromAlgo.TuningEnabled;
                    setAlgo.IntensityUpToDate = false;
                }
            }
        }

        #region Config Setters/Getters
        // settings
        // setters
        public void SetFromComputeDeviceConfig(ComputeDeviceConfig config) {
            if (config != null && config.UUID == UUID) {
                this.Enabled = config.Enabled;
            }
        }
        public void SetAlgorithmDeviceConfig(DeviceBenchmarkConfig config) {
            if (config != null && config.DeviceUUID == UUID && config.AlgorithmSettings != null) {
                this.AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
                foreach (var conf in config.AlgorithmSettings) {
                    var setAlgo = GetAlgorithm(conf.MinerBaseType, conf.NiceHashID, conf.SecondaryNiceHashID);
                    if (setAlgo != null) {
                        setAlgo.BenchmarkSpeed = conf.BenchmarkSpeed;
                        setAlgo.ExtraLaunchParameters = conf.ExtraLaunchParameters;
                        setAlgo.Enabled = conf.Enabled;
                        setAlgo.LessThreads = conf.LessThreads;
                        if (setAlgo is DualAlgorithm dualSA) {
                            dualSA.SecondaryBenchmarkSpeed = conf.SecondaryBenchmarkSpeed;
                            if (config.DualAlgorithmSettings != null) {
                                var dualConf = config.DualAlgorithmSettings.Find(a => a.SecondaryNiceHashID == dualSA.SecondaryNiceHashID);
                                if (dualConf != null) {
                                    dualConf.FixSettingsBounds();
                                    dualSA.IntensitySpeeds = dualConf.IntensitySpeeds;
                                    dualSA.SecondaryIntensitySpeeds = dualConf.SecondaryIntensitySpeeds;
                                    dualSA.TuningEnabled = dualConf.TuningEnabled;
                                    dualSA.TuningStart = dualConf.TuningStart;
                                    dualSA.TuningEnd = dualConf.TuningEnd;
                                    dualSA.TuningInterval = dualConf.TuningInterval;
                                }
                            }
                        }
                    }
                }
            }
        }
        // getters
        public ComputeDeviceConfig GetComputeDeviceConfig() {
            ComputeDeviceConfig ret = new ComputeDeviceConfig();
            ret.Enabled = this.Enabled;
            ret.Name = this.Name;
            ret.UUID = this.UUID;
            return ret;
        }
        public DeviceBenchmarkConfig GetAlgorithmDeviceConfig() {
            DeviceBenchmarkConfig ret = new DeviceBenchmarkConfig();
            ret.DeviceName = this.Name;
            ret.DeviceUUID = this.UUID;
            // init algo settings
            foreach (var algo in this.AlgorithmSettings) {
                // create/setup
                AlgorithmConfig conf = new AlgorithmConfig();
                conf.Name = algo.AlgorithmStringID;
                conf.NiceHashID = algo.NiceHashID;
                conf.MinerBaseType = algo.MinerBaseType;
                conf.MinerName = algo.MinerName; // TODO probably not needed
                conf.BenchmarkSpeed = algo.BenchmarkSpeed;
                conf.ExtraLaunchParameters = algo.ExtraLaunchParameters;
                conf.Enabled = algo.Enabled;
                conf.LessThreads = algo.LessThreads;
                // insert
                ret.AlgorithmSettings.Add(conf);
                if (algo is DualAlgorithm dualAlgo) {
                    conf.SecondaryNiceHashID = dualAlgo.SecondaryNiceHashID;
                    conf.SecondaryBenchmarkSpeed = dualAlgo.SecondaryBenchmarkSpeed;

                    DualAlgorithmConfig dualConf = new DualAlgorithmConfig();
                    dualConf.Name = algo.AlgorithmStringID;
                    dualConf.SecondaryNiceHashID = dualAlgo.SecondaryNiceHashID;
                    dualConf.IntensitySpeeds = dualAlgo.IntensitySpeeds;
                    dualConf.SecondaryIntensitySpeeds = dualAlgo.SecondaryIntensitySpeeds;
                    dualConf.TuningEnabled = dualAlgo.TuningEnabled;
                    dualConf.TuningStart = dualAlgo.TuningStart;
                    dualConf.TuningEnd = dualAlgo.TuningEnd;
                    dualConf.TuningInterval = dualAlgo.TuningInterval;

                    ret.DualAlgorithmSettings.Add(dualConf);
                }
            }
            return ret;
        }
        #endregion Config Setters/Getters

        public List<Algorithm> GetAlgorithmSettings() {
            // hello state
            var algos = GetAlgorithmSettingsThirdParty(ConfigManager.GeneralConfig.Use3rdPartyMiners);

            var retAlgos = MinerPaths.GetAndInitAlgorithmsMinerPaths(algos, this);;

            // NVIDIA
            if (this.DeviceGroupType == DeviceGroupType.NVIDIA_5_x || this.DeviceGroupType == DeviceGroupType.NVIDIA_6_x) {
                retAlgos = retAlgos.FindAll((a) => a.MinerBaseType != MinerBaseType.nheqminer);
            } else if (this.DeviceType == DeviceType.NVIDIA) {
                retAlgos = retAlgos.FindAll((a) => a.MinerBaseType != MinerBaseType.eqm);
            }

            // sort by algo
            retAlgos.Sort((a_1, a_2) => (a_1.NiceHashID - a_2.NiceHashID) != 0 ? 
                (a_1.NiceHashID - a_2.NiceHashID) : ((a_1.MinerBaseType - a_2.MinerBaseType) != 0 ?
                (a_1.MinerBaseType - a_2.MinerBaseType) : (a_1.SecondaryNiceHashID - a_2.SecondaryNiceHashID)));

            return retAlgos;
        }

        public List<Algorithm> GetAlgorithmSettingsFastest() {
            // hello state
            var algosTmp = GetAlgorithmSettings();
            Dictionary<AlgorithmType, Algorithm> sortDict = new Dictionary<AlgorithmType, Algorithm>();
            foreach (var algo in algosTmp) {
                var algoKey = algo.NiceHashID;
                if (sortDict.ContainsKey(algoKey)) {
                    if (sortDict[algoKey].BenchmarkSpeed < algo.BenchmarkSpeed) {
                        sortDict[algoKey] = algo;
                    }
                } else {
                    sortDict[algoKey] = algo;
                }
            }
            List<Algorithm> retAlgos = new List<Algorithm>();
            foreach (var fastestAlgo in sortDict.Values) {
                retAlgos.Add(fastestAlgo);
            }

            return retAlgos;
        }

        private List<Algorithm> GetAlgorithmSettingsThirdParty(Use3rdPartyMiners use3rdParty) {
            if (use3rdParty == Use3rdPartyMiners.YES) {
                return this.AlgorithmSettings;
            }
            var third_party_miners = new List<MinerBaseType>() { MinerBaseType.Claymore, MinerBaseType.OptiminerAMD, MinerBaseType.EWBF, MinerBaseType.Prospector };

            return this.AlgorithmSettings.FindAll((a) => third_party_miners.IndexOf(a.MinerBaseType) == -1);
        }
        
        // static methods
        
        protected static string GetUUID(int id, string group, string name, DeviceGroupType deviceGroupType) {
            var SHA256 = new SHA256Managed();
            var hash = new StringBuilder();
            string mixedAttr = id.ToString() + group + name + ((int)deviceGroupType).ToString();
            byte[] hashedBytes = SHA256.ComputeHash(Encoding.UTF8.GetBytes(mixedAttr), 0, Encoding.UTF8.GetByteCount(mixedAttr));
            foreach (var b in hashedBytes) {
                hash.Append(b.ToString("x2"));
            }
            // GEN indicates the UUID has been generated and cannot be presumed to be immutable
            return "GEN-" + hash.ToString();
        }

        internal bool IsAlgorithmSettingsInitialized() {
            return this.AlgorithmSettings != null;
        }
    }
}
