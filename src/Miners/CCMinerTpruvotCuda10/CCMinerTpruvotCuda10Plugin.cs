using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.IO;
using MinerPluginToolkitV1.Configs;
using NiceHashMinerLegacy.Common;

namespace CCMinerTpruvotCuda10
{
    public class CCMinerTpruvotCuda10Plugin : IMinerPlugin, IInitInternals
    {
        public Version Version => new Version(1, 1);

        public string Name => "CCMinerTpruvotCuda10";

        public string Author => "stanko@nicehash.com";

        public string PluginUUID => "563960f0-4990-11e9-87d3-6b57d758e2c6 ";

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public IMiner CreateMiner()
        {
            return new CCMinerTpruvotCuda10Miner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // 410.48
            var minimumNVIDIADriver = new Version(410, 48);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minimumNVIDIADriver)
            {
                // TODO log
                return supported; // return emtpy
            }
            // SM3.+ and CUDA 10 drivers
            var cudaGpus = devices.Where(dev => dev is CUDADevice cudaDev && cudaDev.SM_major >= 3).Select(dev => (CUDADevice)dev);

            foreach (var gpu in cudaGpus)
            {
                supported.Add(gpu, GetSupportedAlgorithms());
            }

            return supported;
        }

        IReadOnlyList<Algorithm> GetSupportedAlgorithms()
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.NeoScrypt),
                //new Algorithm(PluginUUID, AlgorithmType.Lyra2REv2_UNUSED),
                //new Algorithm(PluginUUID, AlgorithmType.Decred),
                //new Algorithm(PluginUUID, AlgorithmType.Lbry_UNUSED),
                //new Algorithm(PluginUUID, AlgorithmType.X11Gost_UNUSED),
                //new Algorithm(PluginUUID, AlgorithmType.Blake2s),
                //new Algorithm(PluginUUID, AlgorithmType.Sia_UNUSED),
                new Algorithm(PluginUUID, AlgorithmType.Keccak),
                new Algorithm(PluginUUID, AlgorithmType.Skunk),
                //new Algorithm(PluginUUID, AlgorithmType.Lyra2z_UNUSED),
                new Algorithm(PluginUUID, AlgorithmType.X16R),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
            };
        }

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
