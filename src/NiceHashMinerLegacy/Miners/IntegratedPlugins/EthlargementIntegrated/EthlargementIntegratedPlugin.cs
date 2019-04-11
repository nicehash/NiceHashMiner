using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IntegratedPlugins
{
    class EthlargementIntegratedPlugin : Ethlargement.Ethlargement, IntegratedPlugin
    {
        public static EthlargementIntegratedPlugin Instance { get; } = new EthlargementIntegratedPlugin();
        EthlargementIntegratedPlugin() : base("Ethlargement")
        { }

        public bool Is3rdParty => true;

        public override string EthlargementBinPath()
        {
            var binCwd = Path.Combine(Paths.Root, "bin_3rdparty", "ethlargement", "OhGodAnETHlargementPill-r2.exe");
            return binCwd;
        }
    }
}
