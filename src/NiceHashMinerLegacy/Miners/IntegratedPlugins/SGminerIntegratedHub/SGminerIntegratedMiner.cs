using MinerPluginToolkitV1.SgminerCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class SGminerIntegratedMiner : SGMinerBase
    {
        public SGminerIntegratedMiner(string uuid, int openClAmdPlatformNum) : base(uuid, openClAmdPlatformNum)
        {
        }

        protected override Dictionary<string, string> GetEnvironmentVariables()
        {
            if (MinerSystemEnvironmentVariables != null)
            {
                return MinerSystemEnvironmentVariables.DefaultSystemEnvironmentVariables;
            }
            return null;
        }

        protected override string AlgoName
        {
            get
            {
                switch (_algorithmType)
                {
                    // avemore
                    case AlgorithmType.X16R:
                        return "x16r";
                    // gm 
                    case AlgorithmType.DaggerHashimoto:
                        return "ethash";
                    default:
                        return "";
                }
            }
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            if (_uuid != "SGminerAvemore")
            {
                return base.GetBinAndCwdPaths();
            }
            // avemore is differently packed
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", "avermore-windows");
            var binPath = Path.Combine(pluginRootBins, "sgminer.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }
    }
}
