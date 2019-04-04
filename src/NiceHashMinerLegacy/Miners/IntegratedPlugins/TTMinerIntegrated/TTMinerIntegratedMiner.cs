using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class TTMinerIntegratedMiner : TTMiner.TTMiner
    {
        public TTMinerIntegratedMiner(string uuid) : base(uuid)
        { }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "ttminer");
            var binPath = Path.Combine(binCwd, "TT-Miner.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
