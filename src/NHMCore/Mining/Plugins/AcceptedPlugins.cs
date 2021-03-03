using NHM.Common;
using NHM.Common.Configs;
using System.Collections.Generic;

namespace NHMCore.Mining.Plugins
{
    static public class AcceptedPlugins
    {
        private static string AcceptedPluginsPath => Paths.ConfigsPath("AcceptedPlugins.json");
        private static readonly List<string> AcceptedPluginUUIDs = new List<string>();

        static AcceptedPlugins()
        {
            (AcceptedPluginUUIDs, _) = InternalConfigs.GetDefaultOrFileSettings(AcceptedPluginsPath, new List<string> { });
        }

        private static void CommitToFile() => InternalConfigs.WriteFileSettings(AcceptedPluginsPath, AcceptedPluginUUIDs);

        // excavator is not 3rd party miner
        private static readonly List<string> _alwaysAccepted = new List<string> { "27315fe0-3b03-11eb-b105-8d43d5bd63be", "VC_REDIST_x64_2015_2019" };

        public static bool IsAccepted(string pluginUUID) => AcceptedPluginUUIDs.Contains(pluginUUID) || _alwaysAccepted.Contains(pluginUUID);

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
