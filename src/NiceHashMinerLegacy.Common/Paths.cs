using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NiceHashMinerLegacy.Common
{
    public static class Paths
    {
        public static string Root = "";

        public static string MinerPluginsPath() {

            var path = Path.Combine(Root, "miner_plugins");
            return path;
        }
    }
}
