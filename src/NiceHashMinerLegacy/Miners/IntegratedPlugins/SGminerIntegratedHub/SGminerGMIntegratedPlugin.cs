using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class SGminerGMIntegratedPlugin : SGMinerPluginBase
    {
        const string _pluginUUIDName = "SGminerGM";

        public override string PluginUUID => _pluginUUIDName;

        public override Version Version => new Version(1, 0);

        public override string Name => _pluginUUIDName;


        public override IMiner CreateMiner()
        {
            return new SGminerGMIntegratedMiner(PluginUUID, AMDDevice.OpenCLPlatformID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            const ulong MinDaggerHashimotoMemory = 3UL << 30; // 3GB
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var amdGpus = devices
                .Where(dev => dev is AMDDevice amdGpu && amdGpu.GpuRam > MinDaggerHashimotoMemory)
                .Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = new List<Algorithm> {
                    new Algorithm(PluginUUID, AlgorithmType.DaggerHashimoto)
                    {
                        ExtraLaunchParameters = " --remove-disabled --xintensity 512 -w 192 -g 1"
                    },
                };
                supported.Add(gpu, algorithms);
            }

            return supported;
        }
    }
}
