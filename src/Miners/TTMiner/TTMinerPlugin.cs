using System;
using System.Collections.Generic;
using System.Linq;
using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;

namespace TTMiner
{
    public class TTMinerPlugin : IMinerPlugin, IInitInternals
    {
        public Version Version => new Version(1, 1);
        public string Name => "TTMiner";
        public string Author => "stanko@nicehash.com";

        public string PluginUUID => "a61acd93-2dfd-4604-a0d2-15547640a64e";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 398.26
            var minDrivers = new Version(398, 26);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.MTP),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
            };
        }

        public IMiner CreateMiner()
        {
            return new TTMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage
            };
        }

        public bool CanGroup((BaseDevice device, Algorithm algorithm) a, (BaseDevice device, Algorithm algorithm) b)
        {
            return a.algorithm.FirstAlgorithmType == b.algorithm.FirstAlgorithmType;
        }

        public void InitInternals()
        {
            // TODO implement internals MinerOptionSettings
        }

        // 
        private static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParamters,
                    ID = "ttminer_intensity",
                    ShortName = "-i",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParamters,
                    ID = "ttminer_intensity_grid",
                    ShortName = "-ig",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParamters,
                    ID = "ttminer_grid_size",
                    ShortName = "-gs",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
            }
        };
    }
}
