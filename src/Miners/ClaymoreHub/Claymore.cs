using MinerPluginToolkitV1.ClaymoreCommon;
using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaymoreHub
{
    public class Claymore : ClaymoreBase
    {
        public Claymore(string uuid) : base(uuid)
        {
        }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "EthDcrMiner64.exe");
            var binCwd = pluginRootBins;
            return Tuple.Create(binPath, binCwd);
        }
    }
}
