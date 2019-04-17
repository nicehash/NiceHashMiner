using MinerPluginToolkitV1.SgminerCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class SGminerGMIntegratedMiner : SGMinerBase
    {
        public SGminerGMIntegratedMiner(string uuid, int openClAmdPlatformNum) : base(uuid, openClAmdPlatformNum)
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
                    case AlgorithmType.DaggerHashimoto:
                        return "ethash";
                    default:
                        return "";
                }
            }
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin", "sgminer-gm");
            var binPath = Path.Combine(binCwd, "sgminer.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
