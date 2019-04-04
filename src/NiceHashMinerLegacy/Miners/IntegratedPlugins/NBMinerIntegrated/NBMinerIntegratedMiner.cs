using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class NBMinerIntegratedMiner : NBMiner.NBMiner
    {
        public NBMinerIntegratedMiner(string uuid, Dictionary<int, int> mappedCudaIDs) : base(uuid, mappedCudaIDs)
        { }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "nbminer");
            var binPath = Path.Combine(binCwd, "nbminer.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
