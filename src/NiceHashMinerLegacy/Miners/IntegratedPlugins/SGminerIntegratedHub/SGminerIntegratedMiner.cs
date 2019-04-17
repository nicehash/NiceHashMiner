using MinerPluginToolkitV1.SgminerCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;

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
                    // nh general
                    case AlgorithmType.NeoScrypt:
                        return "neoscrypt";
                    case AlgorithmType.Keccak:
                        return "keccak";
                    default:
                        return "";
                }
            }
        }
    }
}
