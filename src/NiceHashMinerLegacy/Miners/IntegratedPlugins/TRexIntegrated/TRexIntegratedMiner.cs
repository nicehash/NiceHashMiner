using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    public class TRexIntegratedMiner : TRex.TRex
    {
        public TRexIntegratedMiner(string uuid) : base(uuid)
        { }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "trex");
            var binPath = Path.Combine(binCwd, "t-rex.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
