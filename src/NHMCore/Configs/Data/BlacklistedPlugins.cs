using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs.Data
{
    [Serializable]
    public class BlacklistedPlugins
    {
        public List<string> BlacklistedPluginUUIDs;

        public static BlacklistedPlugins Instance { get; } = new BlacklistedPlugins();
        private BlacklistedPlugins()
        {
            BlacklistedPluginUUIDs = new List<string>();
        }

        public bool IsSupported(string uuid)
        {
            if (BlacklistedPluginUUIDs.Count == 0) return true;
            var isSupported = !BlacklistedPluginUUIDs.Contains(uuid);
            return isSupported;
        }

        public void AddPluginToBlacklist(string uuid)
        {
            if (BlacklistedPluginUUIDs.Contains(uuid)) return;
            BlacklistedPluginUUIDs.Add(uuid);
            ConfigManager.BlacklistPluginsCommit();
        }
        public void RemovePluginFromBlacklist(string uuid)
        {
            BlacklistedPluginUUIDs.Remove(uuid);
            ConfigManager.BlacklistPluginsCommit();
        }
    }
}
