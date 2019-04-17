using NiceHashMinerLegacy.Common;
using System;
using System.IO;
using XmrStak.Configs;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class XmrStakIntegratedMiner : XmrStak.XmrStak
    {
        public XmrStakIntegratedMiner(string uuid, int openClAmdPlatformNum, IXmrStakConfigHandler configHandler) : base(uuid, openClAmdPlatformNum, configHandler)
        { }

        public override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin", "xmr-stak");
            var binPath = Path.Combine(binCwd, "xmr-stak.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
