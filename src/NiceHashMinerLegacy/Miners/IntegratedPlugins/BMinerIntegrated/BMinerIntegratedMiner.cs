using NiceHashMinerLegacy.Common;
using System;
using System.IO;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class BMinerIntegratedMiner : BMiner.BMiner
    {
        public BMinerIntegratedMiner(string uuid) : base(uuid)
        { }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "bminer");
            var binPath = Path.Combine(binCwd, "bminer.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
