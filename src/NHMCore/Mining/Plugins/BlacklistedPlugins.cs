using NHM.Common;
using NHM.MinerPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;

namespace NHMCore.Mining.Plugins
{
    static class BlacklistedPlugins
    {
        private static string BlacklistedPluginsPath => Paths.ConfigsPath("BlacklistedPlugins.json");
        private static List<string> BlacklistedPluginUUIDs = new List<string>();
        
        // our dummy data class to keep it backward compatible when reading
        [Serializable]
        private class LegacyFileClass
        {
            public List<string> BlacklistedPluginUUIDs = new List<string>();
        }

        static BlacklistedPlugins()
        {
            var blacklistFile = InternalConfigs.ReadFileSettings<List<string>>(BlacklistedPluginsPath);
            if (blacklistFile != null)
            {
                BlacklistedPluginUUIDs = blacklistFile;
            }
            else
            {
                var blacklistFileOld = InternalConfigs.ReadFileSettings<LegacyFileClass>(BlacklistedPluginsPath);
                if (blacklistFileOld?.BlacklistedPluginUUIDs != null)
                    BlacklistedPluginUUIDs = blacklistFileOld.BlacklistedPluginUUIDs;
            }
            // commit to make sure legacy file is overwritten
            CommitToFile();
        }

        private static void CommitToFile() => InternalConfigs.WriteFileSettings(BlacklistedPluginsPath, BlacklistedPluginUUIDs);

        public static bool IsSupported(string uuid)
        {
            if (BlacklistedPluginUUIDs.Count == 0) return true;
            var isSupported = !BlacklistedPluginUUIDs.Contains(uuid);
            return isSupported;
        }

        public static void AddPluginToBlacklist(string uuid)
        {
            if (BlacklistedPluginUUIDs.Contains(uuid)) return;
            BlacklistedPluginUUIDs.Add(uuid);
            CommitToFile();
        }
        public static void RemovePluginFromBlacklist(string uuid)
        {
            BlacklistedPluginUUIDs.Remove(uuid);
            CommitToFile();
        }
    }
}
