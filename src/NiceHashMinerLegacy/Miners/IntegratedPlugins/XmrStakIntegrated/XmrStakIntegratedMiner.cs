using NiceHashMinerLegacy.Common;
using System;
using System.IO;
using XmrStak.Configs;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class XmrStakIntegratedMiner : XmrStak.XmrStak
    {
        public XmrStakIntegratedMiner(string uuid, IXmrStakConfigHandler configHandler) : base(uuid, configHandler)
        { }

        protected override Tuple<string, string> GetBinAndCwdPaths()
        {
            var binCwd = Path.Combine(Paths.Root, "bin", "xmr-stak");
            var binPath = Path.Combine(binCwd, "xmr-stak.exe");
            return Tuple.Create(binPath, binCwd);
        }
    }
}
