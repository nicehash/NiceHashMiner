using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Plugin
{
    // cross referenced local and online
    public class PluginPackageInfoCR
    {
        public PluginPackageInfo OnlineVersion { get; set; }
        public PluginPackageInfo LocalVersion { get; set; }

        public bool HasNewerVersion
        {
            get
            {
                // maybe there is no online version
                if (OnlineVersion == null || LocalVersion == null) return false;
                if (OnlineVersion.PluginVersion.Major > LocalVersion.PluginVersion.Major) return true;
                return OnlineVersion.PluginVersion.Minor > LocalVersion.PluginVersion.Minor;
            }
        }

        public bool Installed {
            get
            {
                return LocalVersion != null;
            }
        }

        // PluginPackageInfo region
        public string PluginUUID
        {
            get
            {
                if (LocalVersion != null) return LocalVersion.PluginUUID;
                if (OnlineVersion != null) return OnlineVersion.PluginUUID;
                return "N/A";
            }
        }

        public string PluginName
        {
            get
            {
                if (LocalVersion != null) return LocalVersion.PluginName;
                if (OnlineVersion != null) return OnlineVersion.PluginName;
                return "N/A";
            }
        }

        
        public Version PluginVersion
        {
            get
            {
                if (LocalVersion != null) return LocalVersion.PluginVersion;
                if (OnlineVersion != null) return OnlineVersion.PluginVersion;
                return null;
            }
        }

        public string PluginPackageURL
        {
            get
            {
                //if (LocalVersion != null) return LocalVersion.PluginPackageURL;
                if (OnlineVersion != null) return OnlineVersion.PluginPackageURL;
                return "N/A";
            }
        }

        public string MinerPackageURL
        {
            get
            {
                //if (LocalVersion != null) return LocalVersion.MinerPackageURL;
                if (OnlineVersion != null) return OnlineVersion.MinerPackageURL;
                return "N/A";
            }
        }
        
        public Dictionary<string, List<string>> SupportedDevicesAlgorithms
        {
            get
            {
                if (LocalVersion != null) return LocalVersion.SupportedDevicesAlgorithms;
                if (OnlineVersion != null) return OnlineVersion.SupportedDevicesAlgorithms;
                return null;
            }
        }
        
        public string PluginAuthor
        {
            get
            {
                if (LocalVersion != null) return LocalVersion.PluginName;
                if (OnlineVersion != null) return OnlineVersion.PluginName;
                return "N/A";
            }
        }

        
        public string PluginDescription
        {
            get
            {
                if (LocalVersion != null) return LocalVersion.PluginDescription;
                if (OnlineVersion != null) return OnlineVersion.PluginDescription;
                return "N/A";
            }
        }
    }
}
