using MinerPlugin;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MinerPluginLoader
{
    public static class MinerPluginHost
    {
        public static Dictionary<string, IMinerPlugin> MinerPlugin { get; } = new Dictionary<string, IMinerPlugin>();
        private static Type pluginType = typeof(IMinerPlugin);

        public static void LoadPlugins(string dirPath)
        {
            if (!Directory.Exists(dirPath)) {
                // TODO directory doesn't exist
                return;
            }

            var dllFiles = Directory.GetFiles(dirPath, "*.dll");
            var pluginTypes = dllFiles
                .Select(dllFile => Assembly.LoadFrom(dllFile))
                .Where(assembly => assembly != null)
                .SelectMany(assembly => {
                    var concreteTypes = assembly.GetTypes().Where(type => !type.IsInterface && !type.IsAbstract);
                    return concreteTypes.Where(type => type.GetInterface(pluginType.FullName) != null);
                });

            foreach (var pluginType in pluginTypes)
            {
                var plugin = (IMinerPlugin)Activator.CreateInstance(pluginType);
                MinerPlugin.Add(plugin.PluginUUID, plugin);
            }
        }
    }
}
