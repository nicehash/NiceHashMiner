using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class GMinerIntegratedMiner : GMinerPlugin.GMiner
    {
        public GMinerIntegratedMiner(string uuid) : base(uuid)
        { }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "gminer");
            var binPath = Path.Combine(binCwd, "miner.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
