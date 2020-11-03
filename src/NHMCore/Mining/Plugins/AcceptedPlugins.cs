using NHM.Common;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;

namespace NHMCore.Mining.Plugins
{
    static public class AcceptedPlugins
    {
        private static string AcceptedPluginsPath => Paths.ConfigsPath("AcceptedPlugins.json");
        private static List<string> AcceptedPluginUUIDs = new List<string>();

        static AcceptedPlugins()
        {
            var file = InternalConfigs.ReadFileSettings<List<string>>(AcceptedPluginsPath);
            if (file != null) AcceptedPluginUUIDs = file;
        }

        private static void CommitToFile() => InternalConfigs.WriteFileSettings(AcceptedPluginsPath, AcceptedPluginUUIDs);

        public static bool IsAccepted(string pluginUUID) => AcceptedPluginUUIDs.Contains(pluginUUID);

        public static void Add(string pluginUUID)
        {
            if (AcceptedPluginUUIDs.Contains(pluginUUID)) return;
            AcceptedPluginUUIDs.Add(pluginUUID);
            CommitToFile();
        }
        public static void Remove(string pluginUUID)
        {
            AcceptedPluginUUIDs.Remove(pluginUUID);
            CommitToFile();
        }
    }
}
