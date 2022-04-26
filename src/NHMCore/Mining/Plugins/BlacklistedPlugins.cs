using NHM.Common;
using NHM.Common.Configs;
using System;
using System.Linq;
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

        public static bool IsNotBlacklisted(string pluginUUID) => !BlacklistedPluginUUIDs.Contains(pluginUUID);

        public static void AddToBlacklist(string pluginUUID)
        {
            if (BlacklistedPluginUUIDs.Contains(pluginUUID)) return;
            BlacklistedPluginUUIDs.Add(pluginUUID);
            CommitToFile();
        }
        public static void RemoveFromBlacklist(string pluginUUID)
        {
            BlacklistedPluginUUIDs.Remove(pluginUUID);
            CommitToFile();
        }




        private static IReadOnlyList<string> _skipDownload = new string[] {
            "e294f620-94eb-11ea-a64d-17be303ea466",
            "e7a58030-94eb-11ea-a64d-17be303ea466",
            "eda6abd0-94eb-11ea-a64d-17be303ea466",
            "fa369d10-94eb-11ea-a64d-17be303ea466",
            "fd45fff0-94eb-11ea-a64d-17be303ea466",
            "03f80500-94ec-11ea-a64d-17be303ea466",
            "074d4a80-94ec-11ea-a64d-17be303ea466",
            "01177a50-94ec-11ea-a64d-17be303ea466",
            "0a07d6a0-94ec-11ea-a64d-17be303ea466",
            "1484c660-94ec-11ea-a64d-17be303ea466",
        };
        public static bool IsDownloadPermaBan(string pluginUUID) => _skipDownload.Contains(pluginUUID);
    }
}
