using NiceHashMinerLegacy.Common;
using System;
using System.IO;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class EWBFIntegratedMiner : EWBF.EwbfMiner
    {
        public EWBFIntegratedMiner(string uuid) : base(uuid)
        { }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "ewbf_144");
            var binPath = Path.Combine(binCwd, "miner.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
