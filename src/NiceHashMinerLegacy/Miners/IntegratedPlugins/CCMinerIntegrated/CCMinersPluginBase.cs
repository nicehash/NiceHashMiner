using MinerPlugin;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.SgminerCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.IO;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    abstract class CCMinersPluginBase : IMinerPlugin, IInitInternals, IntegratedPlugin
    {
        public bool Is3rdParty => false;

        protected abstract string DirPath { get; }

        public abstract string PluginUUID { get; }

        public abstract Version Version { get; }

        public abstract string Name { get; }

        public string Author => "stanko@nicehash.com";

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }
        
        public IMiner CreateMiner()
        {
            return new CCMinerIntegratedMiner(PluginUUID, DirPath)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        public abstract Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices);
        //public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        //{
        //    var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
        //    // 410.48
        //    var minimumNVIDIADriver = new Version(410, 48);
        //    if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minimumNVIDIADriver) return supported; // return emtpy

        //    // SM3.+ and CUDA 10 drivers
        //    var cudaGpus = devices.Where(dev => dev is CUDADevice cudaDev && cudaDev.SM_major > 3).Select(dev => (CUDADevice)dev);

        //    foreach (var gpu in cudaGpus)
        //    {
        //        supported.Add(gpu, GetSupportedAlgorithms());
        //    }

        //    cudaGpus = devices.Where(dev => dev is CUDADevice cudaDev && cudaDev.SM_major == 3).Select(dev => (CUDADevice)dev);
        //    foreach (var gpu in cudaGpus)
        //    {
        //        supported.Add(gpu, GetFilteredSupportedAlgorithms());
        //    }
        //    return supported;
        //}

        //IReadOnlyList<Algorithm> GetSupportedAlgorithms()
        //{
        //    return new List<Algorithm>{
        //        new Algorithm(PluginUUID, AlgorithmType.NeoScrypt),
        //        //new Algorithm(PluginUUID, AlgorithmType.Lyra2REv2_UNUSED),
        //        //new Algorithm(PluginUUID, AlgorithmType.Decred),
        //        //new Algorithm(PluginUUID, AlgorithmType.Lbry_UNUSED),
        //        //new Algorithm(PluginUUID, AlgorithmType.X11Gost_UNUSED),
        //        //new Algorithm(PluginUUID, AlgorithmType.Blake2s),
        //        //new Algorithm(PluginUUID, AlgorithmType.Sia_UNUSED),
        //        new Algorithm(PluginUUID, AlgorithmType.Keccak),
        //        new Algorithm(PluginUUID, AlgorithmType.Skunk),
        //        //new Algorithm(PluginUUID, AlgorithmType.Lyra2z_UNUSED),
        //        new Algorithm(PluginUUID, AlgorithmType.X16R),
        //        new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
        //    };
        //}

        //IReadOnlyList<Algorithm> GetFilteredSupportedAlgorithms()
        //{
        //    return new List<Algorithm>{
        //        new Algorithm(PluginUUID, AlgorithmType.Keccak),
        //        new Algorithm(PluginUUID, AlgorithmType.Skunk),
        //        new Algorithm(PluginUUID, AlgorithmType.X16R),
        //    };
        //}

        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);
            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU threads per call 8-25 (2^N + F, default: 0=auto). Decimals and multiple values are allowed for fine tuning
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ccminertpruvot_intensity",
                    ShortName = "-i",
                    LongName = "--intensity=",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                /// <summary>
                /// number of miner threads (default: number of nVidia GPUs in your system)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_threads",
                    ShortName = "-t",
                    LongName = "--threads=",
                }, 
                /// <summary>
                /// Set device threads scheduling mode (default: auto)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ccminertpruvot_cuda_schedule",
                    ShortName = "--cuda-schedule",
                },
                /// <summary>
                /// set process priority (default: 0 idle, 2 normal to 5 highest)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "0",
                },
                /// <summary>
                /// set process affinity to specific cpu core(s) mask
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ccminertpruvot_affinity",
                    ShortName = "--cpu-affinity",
                }
            }
        };
        #endregion Internal Settings
    }
}
