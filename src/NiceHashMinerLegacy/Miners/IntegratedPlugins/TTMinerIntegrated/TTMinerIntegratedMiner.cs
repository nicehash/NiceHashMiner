using NiceHashMinerLegacy.Common;
using System;
using System.IO;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class TTMinerIntegratedMiner : TTMiner.TTMiner
    {
        public TTMinerIntegratedMiner(string uuid) : base(uuid)
        { }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "ttminer");
            var binPath = Path.Combine(binCwd, "TT-Miner.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
