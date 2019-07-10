using System;
using System.Collections.Generic;

namespace NiceHashMiner.Plugin
{
    // cross referenced local and online
    public class PluginPackageInfoCR
    {
        public PluginPackageInfo OnlineInfo { get; set; }
        public PluginPackageInfo LocalInfo { get; set; }

        public bool HasNewerVersion
        {
            get
            {
                var localVer = LocalInfo?.PluginVersion;
                var onlineVer = OnlineInfo?.PluginVersion;
                if (localVer == null || onlineVer == null) return false;
                if (onlineVer.Major > localVer.Major) return true;
                return onlineVer.Major == localVer.Major && onlineVer.Minor > localVer.Minor;
            }
        }

        public bool Installed {
            get
            {
                return LocalInfo != null;
            }
        }

        public int OnlineSupportedDeviceCount { get; set; } = 0;

        // PluginPackageInfo region
        public string PluginUUID
        {
            get
            {
                var uuid = LocalInfo?.PluginUUID ?? OnlineInfo?.PluginUUID ?? "N/A";
                return uuid;
            }
        }

        public string PluginName
        {
            get
            {
                var name = LocalInfo?.PluginName ?? OnlineInfo?.PluginName ?? "N/A";
                return name;
            }
        }

        
        public Version PluginVersion
        {
            get
            {
                var ver = LocalInfo?.PluginVersion ?? OnlineInfo?.PluginVersion ?? new Version(0,0);
                return ver;
            }
        }

        public string PluginPackageURL
        {
            get
            {
                //var pluginURL = LocalInfo?.PluginPackageURL ?? OnlineInfo?.PluginPackageURL ?? "N/A";
                var pluginURL = OnlineInfo?.PluginPackageURL ?? "N/A";
                return pluginURL;
            }
        }

        public string MinerPackageURL
        {
            get
            {
                //var minerURL = LocalInfo?.MinerPackageURL ?? OnlineInfo?.MinerPackageURL ?? "N/A";
                var minerURL = OnlineInfo?.MinerPackageURL ?? "N/A";
                return minerURL;
            }
        }
        
        public Dictionary<string, List<string>> SupportedDevicesAlgorithms
        {
            get
            {
                var supportedDevicesAlgorithms = LocalInfo?.SupportedDevicesAlgorithms ?? OnlineInfo?.SupportedDevicesAlgorithms ?? new Dictionary<string, List<string>>();
                return supportedDevicesAlgorithms;
            }
        }
        
        public string PluginAuthor
        {
            get
            {
                var author = LocalInfo?.PluginAuthor ?? OnlineInfo?.PluginAuthor ?? "N/A";
                return author;
            }
        }

        
        public string PluginDescription
        {
            get
            {
                // prefer local over online
                var desc = LocalInfo?.PluginDescription ?? OnlineInfo?.PluginDescription ?? "N/A";
                return desc;
            }
        }
    }
}
