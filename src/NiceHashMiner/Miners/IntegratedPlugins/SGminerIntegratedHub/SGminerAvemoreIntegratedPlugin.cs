using MinerPlugin;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Configs;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class SGminerAvemoreIntegratedPlugin : SGMinerPluginBase
    {
        const string _pluginUUIDName = "SGminerAvemore";

        public override string PluginUUID => _pluginUUIDName;

        public override Version Version => new Version(1, 1);

        public override string Name => _pluginUUIDName;

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var amdGpus = devices
                .Where(dev => dev is AMDDevice)
                .Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = new List<Algorithm> {
                    new Algorithm(PluginUUID, AlgorithmType.X16R)
                    {
                        ExtraLaunchParameters = "-X 256"
                    },
                };
                supported.Add(gpu, algorithms);
            }

            return supported;
        }

        protected override MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables { get; set; } = GetMinerSystemEnvironmentVariables(true);

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var miner = CreateMiner() as IBinAndCwdPathsGettter;
            if (miner == null) return Enumerable.Empty<string>();
            var pluginRootBinsPath = miner.GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "libcurl.dll", "libeay32.dll", "libidn-11.dll", "librtmp.dll",
                "libssh2.dll", "pdcurses.dll", "pthreadGC2.dll", "ssleay32.dll", "zlib1.dll", "sgminer.exe"
            });
        }
    }
}

