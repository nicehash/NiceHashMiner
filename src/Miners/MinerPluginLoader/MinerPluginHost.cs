using MinerPlugin;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NiceHashMinerLegacy.Common;

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

        public static IEnumerable<string> LoadPlugins(string pluginsRootDirPath)
        {
            if (!Directory.Exists(pluginsRootDirPath))
            {
                Logger.Info("MinerPluginHost", $"Plugins root directory doesn't exist: {pluginsRootDirPath}");
                // TODO directory doesn't exist
                return Enumerable.Empty<string>();
            }

            // this can throw
            var pluginDirectories = Directory.GetDirectories(pluginsRootDirPath)
                .Where(dir => HasTempPrefix(dir) == false);

            var loadedPlugins = new List<string>();
            foreach (var pluginDirectory in pluginDirectories)
            {
                var loaded = LoadPlugin(pluginDirectory);
                if (loaded.Count() > 0) loadedPlugins.AddRange(loaded);
            }

            Logger.Info("MinerPluginHost", $"Plugins successfully loaded");
            return loadedPlugins;
        }

        public static IEnumerable<string> LoadPlugin(string pluginDirPath)
        {
            if (!Directory.Exists(pluginDirPath)) {
                Logger.Info("MinerPluginHost", $"Plugins path doesn't exist: {pluginDirPath}");
                return Enumerable.Empty<string>();
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
                        Logger.Info("MinerPluginHost", $"Error occured while loading dll files: {e.Message}");
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
                        Logger.Info("MinerPluginHost", $"Error occured while transforming dlls to plugins: {e.Message}");
                        return Enumerable.Empty<Type>();
                    }
                });

            var loadedPlugins = new List<string>();
            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    var plugin = Activator.CreateInstance(pluginType) as IMinerPlugin;
                    if (MinerPlugin.ContainsKey(plugin.PluginUUID))
                    {
                        var existingPlugin = MinerPlugin[plugin.PluginUUID];
                        Logger.Info("MinerPluginHost", $"Already existing plugin {plugin.PluginUUID}");
                        Logger.Info("MinerPluginHost", $"Old name {existingPlugin.Name} and version {existingPlugin.Version}\r\n new name {plugin.Name} and version {plugin.Version}");
                    }
                    MinerPlugin[plugin.PluginUUID] = plugin;
                    loadedPlugins.Add(plugin.PluginUUID);
                }
                catch (Exception e)
                {
                    Logger.Error("MinerPluginHost", $"Error occured while loading plugin: {e.Message}");
                }
            }
            return loadedPlugins;
        }
    }
}
