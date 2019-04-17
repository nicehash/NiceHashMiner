using MinerPluginToolkitV1.SgminerCommon;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SGMinerHub
{
    public class SGMiner : SGMinerBase
    {
        public SGMiner(string uuid, int openClAmdPlatformNum) : base(uuid, openClAmdPlatformNum)
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
                    case AlgorithmType.DaggerHashimoto:
                        return "ethash";
                    case AlgorithmType.X16R:
                        return "x16r";
                    default:
                        return "";
                }
            }
        }

        // this is 
        private string SGMinerName
        {
            get
            {
                switch (_algorithmType)
                {
                    case AlgorithmType.NeoScrypt:
                        return "sgminer_nh";
                    case AlgorithmType.Keccak:
                        return "sgminer_nh";
                    case AlgorithmType.DaggerHashimoto:
                        return "sgminer_gm";
                    case AlgorithmType.X16R:
                        return "avemore";
                    default:
                        return "";
                }
            }
        }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", SGMinerName);
            var binPath = Path.Combine(pluginRootBins, "sgminer.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }
    }
}
