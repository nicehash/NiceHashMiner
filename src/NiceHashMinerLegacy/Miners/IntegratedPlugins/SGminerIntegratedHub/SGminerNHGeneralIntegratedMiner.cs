using MinerPluginToolkitV1.SgminerCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class SGminerNHGeneralIntegratedMiner : SGMinerBase
    {
        public SGminerNHGeneralIntegratedMiner(string uuid, int openClAmdPlatformNum) : base(uuid, openClAmdPlatformNum)
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
                    case AlgorithmType.NeoScrypt:
                        return "neoscrypt";
                    case AlgorithmType.Keccak:
                        return "keccak";
                    default:
                        return "";
                }
            }
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin", "sgminer-5-6-0-general");
            var binPath = Path.Combine(binCwd, "sgminer.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
