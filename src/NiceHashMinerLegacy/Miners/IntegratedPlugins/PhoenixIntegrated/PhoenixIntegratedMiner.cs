using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class PhoenixIntegratedMiner : Phoenix.Phoenix
    {
        public PhoenixIntegratedMiner(string uuid, Dictionary<int, int> mappedIDs) : base(uuid, mappedIDs)
        { }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "phoenix");
            var binPath = Path.Combine(binCwd, "PhoenixMiner.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
