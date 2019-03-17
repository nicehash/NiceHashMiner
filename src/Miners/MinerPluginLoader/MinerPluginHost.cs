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
        private static Type _pluginType = typeof(IMinerPlugin);

        private static HashSet<string> _tempDirPrefixes = new HashSet<string> { "installing", "backup", "temp" };

        private static bool HasTempPrefix(string dirPath)
        {
            return _tempDirPrefixes.Where(prefix => dirPath.Contains(prefix)).Count() > 0;
        }

        public static int LoadPlugins(string pluginsRootDirPath)
        {
            if (!Directory.Exists(pluginsRootDirPath))
            {
                // TODO directory doesn't exist
                return 0;
            }

            // this can throw
            var pluginDirectories = Directory.GetDirectories(pluginsRootDirPath)
                .Where(dir => HasTempPrefix(dir) == false);

            var loadedPlugins = 0;
            foreach (var pluginDirectory in pluginDirectories)
            {
                loadedPlugins += LoadPlugin(pluginDirectory);
            }

            return loadedPlugins;
        }

        public static int LoadPlugin(string pluginDirPath)
        {
            if (!Directory.Exists(pluginDirPath)) {
                // TODO directory doesn't exist
                return 0;
            }

            // get all managed plugin dll's 
            var dllFiles = Directory.GetFiles(pluginDirPath, "*.dll");
            var pluginDllFiles = dllFiles
                .Select(dllFile =>
                {
                    try
                    {
                        // read raw assembly and load that
                        byte[] dllBytes = File.ReadAllBytes(dllFile);
                        return Assembly.Load(dllBytes);

                        // lock the file 
                        //return Assembly.LoadFrom(dllFile);
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
                        return concreteTypes.Where(type => type.GetInterface(_pluginType.FullName) != null);
                    }
                    catch (Exception e)
                    {
                        // TODO logging
                        return Enumerable.Empty<Type>();
                    }
                });

            var loadedPlugins = 0;
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
                    loadedPlugins++;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception while loading plugin {e}");
                }
            }
            return loadedPlugins;
        }
    }
}
