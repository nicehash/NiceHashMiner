using NiceHashMinerLegacy.Common;
using System;
using System.IO;


namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class TeamRedMinerIntegratedMiner : TeamRedMiner.TeamRedMiner
    {
        public TeamRedMinerIntegratedMiner(string uuid, int openClAmdPlatformNum) : base(uuid, openClAmdPlatformNum)
        { }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "teamredminer");
            var binPath = Path.Combine(binCwd, "teamredminer.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
