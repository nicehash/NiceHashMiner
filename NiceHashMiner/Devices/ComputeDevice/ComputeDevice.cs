﻿using NiceHashMiner.Configs;
using NiceHashMiner.Configs.Data;
using NiceHashMiner.Enums;
using NiceHashMiner.Miners.Grouping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NiceHashMiner.Devices
{
    public class ComputeDevice
    {
        public readonly int ID;

        public int Index { get; protected set; } // For socket control, unique
        // to identify equality;
        public readonly string Name; // { get; set; }
        // name count is the short name for displaying in moning groups
        public readonly string NameCount;
        public bool Enabled;
        public readonly DeviceGroupType DeviceGroupType;
        // CPU, NVIDIA, AMD
        public readonly DeviceType DeviceType;
        // UUID now used for saving
        public string Uuid { get; protected set; }
        // used for Claymore indexing
        public int BusID { get; protected set; } = -1;
        public int IDByBus = -1;


        // CPU extras
        public int Threads { get; protected set; }
        public ulong AffinityMask { get; protected set; }

        // GPU extras
        public readonly ulong GpuRam;
        public readonly bool IsEtherumCapale;
        public static readonly ulong Memory3Gb = 3221225472;

        // sgminer extra quickfix
        //public readonly bool IsOptimizedVersion;
        public string Codename { get; protected set; }

        public string InfSection { get; protected set; }

        // amd has some algos not working with new drivers
        public bool DriverDisableAlgos { get; protected set; }

        protected List<Algorithm> AlgorithmSettings;

        public string BenchmarkCopyUuid { get; set; }

        public virtual float Load => 0;
        public virtual float Temp => 0;
        public virtual uint FanSpeed => 0;

        // Ambiguous constructor
        protected ComputeDevice(int id, string name, bool enabled, DeviceGroupType group, bool ethereumCapable,
            DeviceType type, string nameCount, ulong gpuRam)
        {
            ID = id;
            Name = name;
            Enabled = enabled;
            DeviceGroupType = group;
            IsEtherumCapale = ethereumCapable;
            DeviceType = type;
            NameCount = nameCount;
            GpuRam = gpuRam;
        }

        // Fake dev
        public ComputeDevice(int id)
        {
            ID = id;
            Name = "fake_" + id;
            NameCount = Name;
            Enabled = true;
            DeviceType = DeviceType.CPU;
            DeviceGroupType = DeviceGroupType.NONE;
            IsEtherumCapale = false;
            //IsOptimizedVersion = false;
            Codename = "fake";
            Uuid = GetUuid(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType);
            GpuRam = 0;
        }

        // CPU 
        public ComputeDevice(int id, string group, string name, int threads, ulong affinityMask, int cpuCount)
        {
            ID = id;
            Name = name;
            Threads = threads;
            AffinityMask = affinityMask;
            Enabled = true;
            DeviceGroupType = DeviceGroupType.CPU;
            DeviceType = DeviceType.CPU;
            NameCount = string.Format(International.GetText("ComputeDevice_Short_Name_CPU"), cpuCount);
            Uuid = GetUuid(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType);
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            IsEtherumCapale = false;
            GpuRam = 0;
        }

        // GPU NVIDIA
        protected int SMMajor = -1;

        protected int SMMinor = -1;

        public ComputeDevice(CudaDevice cudaDevice, DeviceGroupType group, int gpuCount)
        {
            SMMajor = cudaDevice.SM_major;
            SMMinor = cudaDevice.SM_minor;
            ID = (int) cudaDevice.DeviceID;
            Name = cudaDevice.GetName();
            Enabled = true;
            DeviceGroupType = group;
            IsEtherumCapale = cudaDevice.IsEtherumCapable();
            DeviceType = DeviceType.NVIDIA;
            NameCount = string.Format(International.GetText("ComputeDevice_Short_Name_NVIDIA_GPU"), gpuCount);
            Uuid = cudaDevice.UUID;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            GpuRam = cudaDevice.DeviceGlobalMemory;
        }

        public bool IsSM50()
        {
            return SMMajor == 5 && SMMinor == 0;
        }

        // GPU AMD
        public ComputeDevice(AmdGpuDevice amdDevice, int gpuCount, bool isDetectionFallback)
        {
            ID = amdDevice.DeviceID;
            BusID = amdDevice.BusID;
            DeviceGroupType = DeviceGroupType.AMD_OpenCL;
            Name = amdDevice.DeviceName;
            Enabled = true;
            IsEtherumCapale = amdDevice.IsEtherumCapable();
            DeviceType = DeviceType.AMD;
            NameCount = string.Format(International.GetText("ComputeDevice_Short_Name_AMD_GPU"), gpuCount);
            Uuid = isDetectionFallback
                ? GetUuid(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType)
                : amdDevice.UUID;
            // sgminer extra
            //IsOptimizedVersion = amdDevice.UseOptimizedVersion;
            Codename = amdDevice.Codename;
            InfSection = amdDevice.InfSection;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            DriverDisableAlgos = amdDevice.DriverDisableAlgos;
            GpuRam = amdDevice.DeviceGlobalMemory;
        }

        // combines long and short name
        public string GetFullName()
        {
            return string.Format(International.GetText("ComputeDevice_Full_Device_Name"), NameCount, Name);
        }

        public Algorithm GetAlgorithm(MinerBaseType minerBaseType, AlgorithmType algorithmType,
            AlgorithmType secondaryAlgorithmType)
        {
            var toSetIndex = AlgorithmSettings.FindIndex(a =>
                a.NiceHashID == algorithmType && a.MinerBaseType == minerBaseType &&
                a.SecondaryNiceHashID == secondaryAlgorithmType);
            return toSetIndex > -1 ? AlgorithmSettings[toSetIndex] : null;
        }

        //public Algorithm GetAlgorithm(string algoID) {
        //    int toSetIndex = this.AlgorithmSettings.FindIndex((a) => a.AlgorithmStringID == algoID);
        //    if (toSetIndex > -1) {
        //        return this.AlgorithmSettings[toSetIndex];
        //    }
        //    return null;
        //}

        public void CopyBenchmarkSettingsFrom(ComputeDevice copyBenchCDev)
        {
            foreach (var copyFromAlgo in copyBenchCDev.AlgorithmSettings)
            {
                var setAlgo = GetAlgorithm(copyFromAlgo.MinerBaseType, copyFromAlgo.NiceHashID,
                    copyFromAlgo.SecondaryNiceHashID);
                if (setAlgo != null)
                {
                    setAlgo.BenchmarkSpeed = copyFromAlgo.BenchmarkSpeed;
                    setAlgo.SecondaryBenchmarkSpeed = copyFromAlgo.SecondaryBenchmarkSpeed;
                    setAlgo.ExtraLaunchParameters = copyFromAlgo.ExtraLaunchParameters;
                    setAlgo.LessThreads = copyFromAlgo.LessThreads;
                }
            }
        }

        #region Config Setters/Getters

        // settings
        // setters
        public void SetFromComputeDeviceConfig(ComputeDeviceConfig config)
        {
            if (config != null && config.UUID == Uuid)
            {
                Enabled = config.Enabled;
            }
        }

        public void SetAlgorithmDeviceConfig(DeviceBenchmarkConfig config)
        {
            if (config != null && config.DeviceUUID == Uuid && config.AlgorithmSettings != null)
            {
                AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
                foreach (var conf in config.AlgorithmSettings)
                {
                    var setAlgo = GetAlgorithm(conf.MinerBaseType, conf.NiceHashID, conf.SecondaryNiceHashID);
                    if (setAlgo != null)
                    {
                        setAlgo.BenchmarkSpeed = conf.BenchmarkSpeed;
                        setAlgo.SecondaryBenchmarkSpeed = conf.SecondaryBenchmarkSpeed;
                        setAlgo.ExtraLaunchParameters = conf.ExtraLaunchParameters;
                        setAlgo.Enabled = conf.Enabled;
                        setAlgo.LessThreads = conf.LessThreads;
                    }
                }
            }
        }

        // getters
        public ComputeDeviceConfig GetComputeDeviceConfig()
        {
            var ret = new ComputeDeviceConfig
            {
                Enabled = Enabled,
                Name = Name,
                UUID = Uuid
            };
            return ret;
        }

        public DeviceBenchmarkConfig GetAlgorithmDeviceConfig()
        {
            var ret = new DeviceBenchmarkConfig
            {
                DeviceName = Name,
                DeviceUUID = Uuid
            };
            // init algo settings
            foreach (var algo in AlgorithmSettings)
            {
                // create/setup
                var conf = new AlgorithmConfig
                {
                    Name = algo.AlgorithmStringID,
                    NiceHashID = algo.NiceHashID,
                    SecondaryNiceHashID = algo.SecondaryNiceHashID,
                    MinerBaseType = algo.MinerBaseType,
                    MinerName = algo.MinerName,
                    BenchmarkSpeed = algo.BenchmarkSpeed,
                    SecondaryBenchmarkSpeed = algo.SecondaryBenchmarkSpeed,
                    ExtraLaunchParameters = algo.ExtraLaunchParameters,
                    Enabled = algo.Enabled,
                    LessThreads = algo.LessThreads
                };
                // TODO probably not needed
                // insert
                ret.AlgorithmSettings.Add(conf);
            }
            return ret;
        }

        #endregion Config Setters/Getters

        public List<Algorithm> GetAlgorithmSettings()
        {
            // hello state
            var algos = GetAlgorithmSettingsThirdParty(ConfigManager.GeneralConfig.Use3rdPartyMiners);

            var retAlgos = MinerPaths.GetAndInitAlgorithmsMinerPaths(algos, this);
            ;

            // NVIDIA
            if (DeviceGroupType == DeviceGroupType.NVIDIA_5_x || DeviceGroupType == DeviceGroupType.NVIDIA_6_x)
            {
                retAlgos = retAlgos.FindAll(a => a.MinerBaseType != MinerBaseType.nheqminer);
            }
            else if (DeviceType == DeviceType.NVIDIA)
            {
                retAlgos = retAlgos.FindAll(a => a.MinerBaseType != MinerBaseType.eqm);
            }

            // sort by algo
            retAlgos.Sort((a1, a2) =>
                (a1.NiceHashID - a2.NiceHashID) != 0
                    ? (a1.NiceHashID - a2.NiceHashID)
                    : (a1.MinerBaseType - a2.MinerBaseType));

            return retAlgos;
        }

        public List<Algorithm> GetAlgorithmSettingsFastest()
        {
            // hello state
            var algosTmp = GetAlgorithmSettings();
            var sortDict = new Dictionary<AlgorithmType, Algorithm>();
            foreach (var algo in algosTmp)
            {
                var algoKey = algo.NiceHashID;
                if (sortDict.ContainsKey(algoKey))
                {
                    if (sortDict[algoKey].BenchmarkSpeed < algo.BenchmarkSpeed)
                    {
                        sortDict[algoKey] = algo;
                    }
                }
                else
                {
                    sortDict[algoKey] = algo;
                }
            }

            return sortDict.Values.ToList();
        }

        private List<Algorithm> GetAlgorithmSettingsThirdParty(Use3rdPartyMiners use3rdParty)
        {
            if (use3rdParty == Use3rdPartyMiners.YES)
            {
                return AlgorithmSettings;
            }
            var thirdPartyMiners = new List<MinerBaseType>
            {
                MinerBaseType.Claymore,
                MinerBaseType.OptiminerAMD,
                MinerBaseType.EWBF,
                MinerBaseType.Prospector,
                MinerBaseType.dtsm
            };

            return AlgorithmSettings.FindAll(a => thirdPartyMiners.IndexOf(a.MinerBaseType) == -1);
        }

        // static methods

        protected static string GetUuid(int id, string group, string name, DeviceGroupType deviceGroupType)
        {
            var sha256 = new SHA256Managed();
            var hash = new StringBuilder();
            var mixedAttr = id + group + name + (int) deviceGroupType;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(mixedAttr), 0,
                Encoding.UTF8.GetByteCount(mixedAttr));
            foreach (var b in hashedBytes)
            {
                hash.Append(b.ToString("x2"));
            }
            // GEN indicates the UUID has been generated and cannot be presumed to be immutable
            return "GEN-" + hash;
        }

        internal bool IsAlgorithmSettingsInitialized()
        {
            return AlgorithmSettings != null;
        }
    }
}
