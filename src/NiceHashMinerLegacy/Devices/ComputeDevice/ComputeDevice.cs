using System;
using NiceHashMiner.Configs;
using NiceHashMiner.Configs.Data;
using NiceHashMiner.Miners.Grouping;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Common.Device;
using NHM.UUID;

namespace NiceHashMiner.Devices
{
    public class ComputeDevice
    {
        // migrate ComputeDevice to BaseDevice
        public BaseDevice PluginDevice { get; protected set; }

        public readonly int ID;

        public int Index { get; protected set; } // For socket control, unique

        // to identify equality;
        public string Name { get; private set; } // => PluginDevice.Name;

        // name count is the short name for displaying in moning groups
        public readonly string NameCount;
        public bool Enabled { get; protected set; }

        // disabled state check
        public bool IsDisabled => (!Enabled || State == DeviceState.Disabled);

        public DeviceState State { get; set; } = DeviceState.Stopped;

        // CPU, NVIDIA, AMD
        public readonly DeviceType DeviceType;

        // UUID now used for saving
        public string Uuid => PluginDevice.UUID;

        public string B64Uuid
        {
            get
            {
                //UUIDs
                //RIG - 0
                //CPU - 1
                //GPU - 2 // NVIDIA
                //AMD - 3
                // types 

                int type = 1; // assume type is CPU
                if (DeviceType == DeviceType.NVIDIA)
                {
                    type = 2;
                }
                else if (DeviceType == DeviceType.AMD)
                {
                    type = 3;
                }
                var b64Web = UUID.GetB64UUID(Uuid);
                return $"{type}-{b64Web}";
            }
        }

        //// used for Claymore indexing
        //public int BusID { get; protected set; } = -1;
        ////public int IDByBus = -1;


        // CPU extras
        public int Threads { get; protected set; }
        public ulong AffinityMask { get; protected set; }

        // GPU extras
        public readonly ulong GpuRam;

        // copy pasted from CUDA Compute device
        public uint PowerTarget { get; protected set; }
        public PowerLevel PowerLevel { get; protected set; }

        // sgminer extra quickfix
        //public readonly bool IsOptimizedVersion;
        //public string Codename { get; protected set; }
        //public string InfSection { get; protected set; }

        public List<Algorithm> AlgorithmSettings { get; protected set; } = new List<Algorithm>();

        public double MinimumProfit { get; set; }

        public string BenchmarkCopyUuid { get; set; }

        public virtual float Load => -1;
        public virtual float Temp => -1;
        public virtual int FanSpeed => -1;
        public virtual double PowerUsage => -1;

        // Ambiguous constructor
        protected ComputeDevice(int id, string name, bool enabled, DeviceType type, string nameCount, ulong gpuRam)
        {
            ID = id;
            Name = name;
            SetEnabled(enabled);
            DeviceType = type;
            NameCount = nameCount;
            GpuRam = gpuRam;
        }

        public void SetEnabled(bool isEnabled)
        {
            Enabled = isEnabled;
            State = isEnabled ? DeviceState.Stopped : DeviceState.Disabled;
        }

        // combines long and short name
        public string GetFullName()
        {
            return string.Format(Translations.Tr("{0} {1}"), NameCount, Name);
        }
         
        // TODO double check adding and removing plugin algos
        public void UpdatePluginAlgorithms(string pluginUuid, IList<PluginAlgorithm> pluginAlgos)
        {
            var pluginUuidAlgos = AlgorithmSettings
                .Where(algo => algo is PluginAlgorithm pAlgo && pAlgo.BaseAlgo.MinerID == pluginUuid)
                .Cast<PluginAlgorithm>();

            // filter out old plugin algorithms if any
            if (pluginUuidAlgos.Count() > 0)
            {
                AlgorithmSettings = AlgorithmSettings.Where(algo => pluginUuidAlgos.Contains(algo) == false).ToList();
            }

            // keep old algorithms with settings and filter out obsolete ones
            var newAlgorithmIDs = pluginAlgos.Select(algo => algo.AlgorithmStringID);
            var oldAlgosWithSettings = pluginUuidAlgos.Where(algo => newAlgorithmIDs.Contains(algo.AlgorithmStringID));

            // filter out old algorithms with settings and keep only brand new ones
            var oldAlgosWithSettingsIDs = oldAlgosWithSettings.Select(algo => algo.AlgorithmStringID).ToList();
            var newPluginAlgos = pluginAlgos.Where(algo => oldAlgosWithSettingsIDs.Contains(algo.AlgorithmStringID) == false);
            
            // add back old ones that are in the new module
            if (oldAlgosWithSettings.Count() > 0) AlgorithmSettings.AddRange(oldAlgosWithSettings);
            // add new ones 
            //if (newPluginAlgos.Count() > 0) AlgorithmSettings.AddRange(newPluginAlgos);
            var newPluginAlgosList = newPluginAlgos.ToList();
            foreach (var pluginAlgo in newPluginAlgos)
            {
                AlgorithmSettings.Add(pluginAlgo);
            }
        }

        public void RemovePluginAlgorithms(string pluginUUID)
        {
            var toRemove = AlgorithmSettings.Where(algo => algo is PluginAlgorithm pAlgo && pAlgo.BaseAlgo.MinerID == pluginUUID);
            if (toRemove.Count() == 0) return;
            var newList = AlgorithmSettings.Where(algo => toRemove.Contains(algo) == false).ToList();
            AlgorithmSettings = newList;
        }

        public void CopyBenchmarkSettingsFrom(ComputeDevice copyBenchCDev)
        {
            foreach (var copyFromAlgo in copyBenchCDev.AlgorithmSettings)
            { 
                var setAlgo = AlgorithmSettings.Where(a => a.AlgorithmStringID == copyFromAlgo.AlgorithmStringID).FirstOrDefault();
                if (setAlgo != null)
                {
                    setAlgo.BenchmarkSpeed = copyFromAlgo.BenchmarkSpeed;
                    setAlgo.ExtraLaunchParameters = copyFromAlgo.ExtraLaunchParameters;
                    setAlgo.PowerUsage = copyFromAlgo.PowerUsage;
                }
            }
        }

        public Algorithm GetAlgorithm(string minerUUID, params AlgorithmType[] ids)
        {
            return AlgorithmSettings.Where(a => a.MinerUUID == minerUUID && a.IDs.Except(ids).Count() == 0).FirstOrDefault();
        }

        #region Config Setters/Getters
        
        public void SetDeviceConfig(DeviceConfig config)
        {
            if (config == null || config.DeviceUUID != Uuid) return;
            // set device settings
            Enabled = config.Enabled;
            MinimumProfit = config.MinimumProfit;
            PowerTarget = config.PowerTarget;
            PowerLevel = config.PowerLevel;


            if (config.PluginAlgorithmSettings == null) return;
            // plugin algorithms
            var pluginAlgos = AlgorithmSettings.Where(algo => algo is PluginAlgorithm).Cast<PluginAlgorithm>();
            foreach (var pluginConf in config.PluginAlgorithmSettings)
            {
                var pluginConfAlgorithmIDs = pluginConf.GetAlgorithmIDs();
                var pluginAlgo = pluginAlgos
                    .Where(pAlgo => pluginConf.PluginUUID == pAlgo.BaseAlgo.MinerID && pluginConfAlgorithmIDs.Except(pAlgo.BaseAlgo.IDs).Count() == 0)
                    .FirstOrDefault();
                if (pluginAlgo == null) continue;
                // set plugin algo
                pluginAlgo.Speeds = pluginConf.Speeds;
                pluginAlgo.Enabled = pluginConf.Enabled;
                pluginAlgo.ExtraLaunchParameters = pluginConf.ExtraLaunchParameters;
                pluginAlgo.PowerUsage = pluginConf.PowerUsage;
                pluginAlgo.ConfigVersion = pluginConf.GetVersion();
            }
        }

        public DeviceConfig GetDeviceConfig()
        {
            var ret = new DeviceConfig
            {
                DeviceName = Name,
                DeviceUUID = Uuid,
                Enabled = Enabled,
                MinimumProfit = MinimumProfit,
                PowerLevel = PowerLevel,
                PowerTarget = PowerTarget
            };
            // init algo settings
            foreach (var algo in AlgorithmSettings)
            {
                if (algo is PluginAlgorithm pluginAlgo)
                {
                    var pluginConf = new PluginAlgorithmConfig
                    {
                        Name = pluginAlgo.PluginName,
                        PluginUUID = pluginAlgo.BaseAlgo.MinerID,
                        AlgorithmIDs = string.Join("-", pluginAlgo.BaseAlgo.IDs.Select(id => id.ToString())),
                        Enabled = pluginAlgo.Enabled,
                        ExtraLaunchParameters = pluginAlgo.ExtraLaunchParameters,
                        PluginVersion = $"{pluginAlgo.PluginVersion.Major}.{pluginAlgo.PluginVersion.Minor}",
                        PowerUsage = pluginAlgo.PowerUsage,
                        Speeds = pluginAlgo.Speeds
                    };
                    ret.PluginAlgorithmSettings.Add(pluginConf);
                }
            }

            return ret;
        }

        #endregion Config Setters/Getters

        // static methods
        internal bool IsAlgorithmSettingsInitialized()
        {
            return AlgorithmSettings != null;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ComputeDevice) obj);
        }

        protected bool Equals(ComputeDevice other)
        {
            return ID == other.ID && DeviceType == other.DeviceType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ID;
                hashCode = (hashCode * 397) ^ (int) DeviceType;
                return hashCode;
            }
        }

        public static bool operator ==(ComputeDevice left, ComputeDevice right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ComputeDevice left, ComputeDevice right)
        {
            return !Equals(left, right);
        }
    }
}
