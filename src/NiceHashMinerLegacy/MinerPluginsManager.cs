using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinerPlugin;
using MinerPlugin.Toolkit;
using MinerPluginLoader;
using NiceHashMiner.Algorithms;
using NiceHashMiner.Devices;

namespace NiceHashMiner
{
    public static class MinerPluginsManager
    {
        const string PluginsPath = "miner_plugins";

        public static void LoadMinerPlugins()
        {
            MinerPluginHost.LoadPlugins(PluginsPath);
            // get devices
            var allDevs = AvailableDevices.Devices;
            var baseDevices = allDevs.Select(dev => dev.PluginDevice);
            // examine all plugins and what to use
            foreach (var kvp in MinerPluginHost.MinerPlugin)
            {
                var pluginUuid = kvp.Key;
                var plugin = kvp.Value;
                var supported = plugin.GetSupportedAlgorithms(baseDevices);
                // check out the supported algorithms
                foreach (var pair in supported)
                {
                    var bd = pair.Key;
                    var algos = pair.Value;
                    var dev = AvailableDevices.GetDeviceWithUuid(bd.UUID);
                    var pluginAlgos = algos.Select(a => new PluginAlgorithm(a)).ToList();
                    dev.UpdatePluginAlgorithms(pluginUuid, pluginAlgos);
                }
            }
        }

        public static IMinerPlugin GetPluginWithUuid(string pluginUuid)
        {
            if (!MinerPluginHost.MinerPlugin.ContainsKey(pluginUuid)) return null;
            return MinerPluginHost.MinerPlugin[pluginUuid];
        }
    }
}
