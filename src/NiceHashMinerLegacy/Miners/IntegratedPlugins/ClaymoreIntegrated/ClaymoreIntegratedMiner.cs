using MinerPluginToolkitV1.ClaymoreCommon;
using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class ClaymoreIntegratedMiner : ClaymoreBase
    {
        public ClaymoreIntegratedMiner(string uuid) : base(uuid)
        { }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "claymore_dual");
            var binPath = Path.Combine(binCwd, "EthDcrMiner64.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
