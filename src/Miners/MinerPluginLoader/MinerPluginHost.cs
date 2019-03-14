using MinerPlugin;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MinerPluginLoader
{
    // TODO implement unloadable plugins
    public static class MinerPluginHost
    {
        public static Dictionary<string, IMinerPlugin> MinerPlugin { get; } = new Dictionary<string, IMinerPlugin>();
        private static Type pluginType = typeof(IMinerPlugin);

        public static void LoadPlugins(string dirPath, SearchOption searchOption)
        {
            if (!Directory.Exists(dirPath)) {
                // TODO directory doesn't exist
                return;
            }

            // get all managed plugin dll's 
            var dllFiles = Directory.GetFiles(dirPath, "*.dll", searchOption);
            var pluginDllFiles = dllFiles
                .Select(dllFile =>
                {
                    try
                    {
                        return Assembly.LoadFrom(dllFile);
                    }
                    catch (Exception e)
                    {
                        // TODO logging
                        return null;
                    }
                })
                .Where(assembly => assembly != null);
            var pluginTypes = pluginDllFiles
                .SelectMany(assembly => {
                    try
                    {
                        var concreteTypes = assembly.GetTypes().Where(type => !type.IsInterface && !type.IsAbstract);
                        return concreteTypes.Where(type => type.GetInterface(pluginType.FullName) != null);
                    }
                    catch (Exception e)
                    {
                        // TODO logging
                        return Enumerable.Empty<Type>();
                    }
                });

            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    var plugin = Activator.CreateInstance(pluginType) as IMinerPlugin;
                    if (MinerPlugin.ContainsKey(plugin.PluginUUID))
                    {
                        var existingPlugin = MinerPlugin[plugin.PluginUUID];
                        Console.WriteLine($"contains key {plugin.PluginUUID}");
                        Console.WriteLine($"existing {existingPlugin.Name} v{existingPlugin.Version}");
                        Console.WriteLine($"new {plugin.Name} v{plugin.Version}");
                    }
                    MinerPlugin[plugin.PluginUUID] = plugin;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception while loading plugin {e}");
                }
            }
        }
    }
}
