using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Plugin
{
    // cross referenced local and online
    public class PluginPackageInfoCR : PluginPackageInfo
    {
        public bool Installed { get; set; }
        public PluginPackageInfo OnlineVersion { get; set; }

        public bool LatestVersion
        {
            get
            {
                if (OnlineVersion == null) return true;
                return OnlineVersion.PluginVersion >= PluginVersion;
            }
        }
    }
}
