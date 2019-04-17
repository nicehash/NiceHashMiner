using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class SGminerNHGeneralIntegratedPlugin : SGMinerPluginBase
    {
        const string _pluginUUIDName = "SGminerNHGeneral";

        public override string PluginUUID => _pluginUUIDName;

        public override Version Version => new Version(1, 0);

        public override string Name => _pluginUUIDName;

        public override IMiner CreateMiner()
        {
            return new SGminerNHGeneralIntegratedMiner(PluginUUID, AMDDevice.OpenCLPlatformID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var amdGpus = devices
                .Where(dev => dev is AMDDevice)
                .Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = new List<Algorithm> {
                    new Algorithm(PluginUUID, AlgorithmType.Keccak)
                    {
                        ExtraLaunchParameters = " --keccak-unroll 0 --hamsi-expand-big 4 --remove-disabled --intensity 15"
                    },
                };
                supported.Add(gpu, algorithms);
            }

            return supported;
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "sgminer" });
        }
    }
}
