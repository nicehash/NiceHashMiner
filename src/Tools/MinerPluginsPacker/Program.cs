using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MinerPlugin;
using MinerPluginLoader;
using System.IO.Compression;
using MinerPluginToolkitV1;
using MinerPluginToolkitV1.Interfaces;
using NHM.Common.Enums;
using MinerPluginToolkitV1.Configs;

namespace MinerPluginsPacker
{
    class Program
    {
        internal class MajorMinorVersion
        {
            internal MajorMinorVersion(int major, int minor)
            {
                this.major = major;
                this.minor = minor;
            }
            public int major { get; private set; }
            public int minor { get; private set; }
        }

        internal class PluginPackageInfoForJson : PluginPackageInfo
        {
            public new MajorMinorVersion PluginVersion { get; set; }
        }

        private static Dictionary<string, List<string>> TransformToPluginPackageInfoSupportedDevicesAlgorithms(Dictionary<DeviceType, List<AlgorithmType>> supportedDevicesAlgorithms)
        {
            var ret = new Dictionary<string, List<string>>();
            foreach (var kvp in supportedDevicesAlgorithms)
            {
                var stringKey = kvp.Key.ToString();
                var stringAlgos = kvp.Value.Select(algo => algo.ToString()).ToList();
                ret[stringKey] = stringAlgos;
            }
            return ret;
        }

        private static List<PluginPackageInfoForJson> PluginPackageInfos = new List<PluginPackageInfoForJson>();

        private static string GetPluginPackageName(IMinerPlugin plugin)
        {
            // TODO workaround to check if it is built with the Toolkit
            var isToolkitMiner = plugin is IInitInternals;
            var versionStr = $"v{plugin.Version.Major}.{plugin.Version.Minor}";
            if (isToolkitMiner)
            {
                versionStr = $"{versionStr}_mptoolkitV1";
            }
            var pluginZipFileName = $"{plugin.Name}_{versionStr}_{plugin.PluginUUID}.zip";
            return pluginZipFileName;
        }

        private static void AddPluginToPluginPackageInfos(IMinerPlugin plugin)
        {
            var version = new MajorMinorVersion(plugin.Version.Major, plugin.Version.Minor);

            string pluginPackageURL = null;
            if (version.major == 3)
            {
                pluginPackageURL = "https://github.com/nicehash/NHM_MinerPluginsDownloads/releases/download/v3.x/" + GetPluginPackageName(plugin);
            }
            else
            {
                //throw new Exception("Plugin version not supported");
            }
            string minerPackageURL = null;
            if (plugin is IMinerBinsSource binsSource)
            {
                minerPackageURL = binsSource.GetMinerBinsUrlsForPlugin().FirstOrDefault();
            }

            // TODO binary version
            if (plugin is IGetMinerBinaryVersion binVersionGetter)
            {

            }
            PluginMetaInfo pluginMetaInfo = null;
            if (plugin is IGetPluginMetaInfo pluginMetaInfoGetter)
            {
                pluginMetaInfo = pluginMetaInfoGetter.GetPluginMetaInfo();
            }
            if (pluginMetaInfo == null) return;

            var packageInfo = new PluginPackageInfoForJson
            {
                PluginUUID = plugin.PluginUUID,
                PluginAuthor = "info@nicehash.com",
                PluginName = plugin.Name,
                PluginVersion = version,
                PluginPackageURL = pluginPackageURL,
                MinerPackageURL = minerPackageURL,
                SupportedDevicesAlgorithms = TransformToPluginPackageInfoSupportedDevicesAlgorithms(pluginMetaInfo.SupportedDevicesAlgorithms),
                // TODO enhance this with the bins version
                PluginDescription = pluginMetaInfo.PluginDescription
            };
            PluginPackageInfos.Add(packageInfo);
        }

        // TODO add more options 
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Set miner plugins root path");
                return;
            }

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pluginPackagesFolder = Path.Combine(exePath, "plugins_packages");
            var pluginsSearchRoot = args[0];

            // get all managed plugin dll's 
            var dllFiles = Directory.GetFiles(pluginsSearchRoot, "*.dll", SearchOption.AllDirectories)
                .Where(filePath => !filePath.Contains("MinerPlugin") && filePath.Contains("net45") && filePath.Contains("Release") && !filePath.Contains("bin")).ToList();

            var packedPlugins = new HashSet<string>();

            if (Directory.Exists(pluginPackagesFolder))
            {
                Console.WriteLine("Deleting old plugins packages");
                Directory.Delete(pluginPackagesFolder, true);
            }
            if (!Directory.Exists(pluginPackagesFolder))
            {
                Directory.CreateDirectory(pluginPackagesFolder);
            }

            foreach (var filePath in dllFiles)
            {
                var dllDir = Path.GetDirectoryName(filePath);
                var loaded = MinerPluginHost.LoadPlugin(dllDir);
                if (loaded.Count() == 0)
                {
                    // log what we couldn't load and continue
                    Console.WriteLine($"Skipping: {filePath}");
                    continue;
                }
                var newPlugins = MinerPluginHost.MinerPlugin
                    .Where(kvp => packedPlugins.Contains(kvp.Key) == false)
                    .Select(kvp => kvp.Value);

                foreach (var plugin in newPlugins)
                {
                    var pluginZipFileName = GetPluginPackageName(plugin);
                    var dllPackageZip = Path.Combine(pluginPackagesFolder, pluginZipFileName);
                    Console.WriteLine($"Packaging: {dllPackageZip}");
                    var fileName = Path.GetFileName(filePath);

                    using (var archive = ZipFile.Open(dllPackageZip, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(filePath, fileName);
                    }

                    packedPlugins.Add(plugin.PluginUUID);
                    AddPluginToPluginPackageInfos(plugin);
                }
            }

            // dump our plugin packages
            InternalConfigs.WriteFileSettings(Path.Combine(pluginPackagesFolder, "update.json"), PluginPackageInfos);
        }
    }
}
