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

        private static void CheckPluginMetaData(IMinerPlugin plugin)
        {
            if (plugin is IPluginSupportedAlgorithmsSettings pluginSettings)
            {
                var supportedDevicesAlgorithms = pluginSettings.SupportedDevicesAlgorithmsDict();
                var supportedDevicesAlgorithmsCount = supportedDevicesAlgorithms.Select(dict => dict.Value.Count).Sum();
                if (supportedDevicesAlgorithmsCount == 0) throw new Exception($"{plugin.Name}-{plugin.PluginUUID} NO algorithms");

                foreach (var kvp in supportedDevicesAlgorithms)
                {
                    foreach (var algo in kvp.Value)
                    {
                        var name = pluginSettings.AlgorithmName(algo);
                        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                        {
                            throw new Exception($"{plugin.Name}-{plugin.PluginUUID} Invalid name '{name}' for algorithm type '{algo.ToString()}'");
                        }
                    }
                }
            }
        }

        private static int[] _supportedMajorverLinks = new int[] { 3, 4, 5, 6, 7 };
        private static bool IsMajorVersionLinkSupported(int major)
        {
            return _supportedMajorverLinks.Contains(major);
        }

        private static void AddPluginToPluginPackageInfos(IMinerPlugin plugin)
        {
            var version = new MajorMinorVersion(plugin.Version.Major, plugin.Version.Minor);

            string pluginPackageURL = null;
            if (IsMajorVersionLinkSupported(version.major))
            {
                pluginPackageURL = $"https://github.com/nicehash/NHM_MinerPluginsDownloads/releases/download/v{version.major}.x/" + GetPluginPackageName(plugin);
            }
            else
            {
                throw new Exception($"Plugin version '{version.major}' not supported. Make sure you add the download link for this version");
            }
            string minerPackageURL = null;
            if (plugin is IMinerBinsSource binsSource)
            {
                minerPackageURL = binsSource.GetMinerBinsUrlsForPlugin().FirstOrDefault();
            }

            var binaryVersion = "N/A";
            // TODO binary version
            if (plugin is IGetMinerBinaryVersion binVersionGetter)
            {
                binaryVersion = binVersionGetter.GetMinerBinaryVersion();
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
                PluginDescription = $"Miner Binary Version '{binaryVersion}'.\n\n" + pluginMetaInfo.PluginDescription
            };
            PluginPackageInfos.Add(packageInfo);
        }

        // TODO add more options 
        static void Main(string[] args)
        {
            PluginBase.IS_CALLED_FROM_PACKER = true;
            if (args.Length < 1)
            {
                Console.WriteLine("Set miner plugins root path");
                return;
            }

            //var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var exePath = Environment.CurrentDirectory;
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

            // what plugins to bundle
            var bundlePlugins = new List<string>
            {
                "2257f160-7236-11e9-b20c-f9f12eb6d835", // CCMinerTpruvotPlugin
                "70984aa0-7236-11e9-b20c-f9f12eb6d835", // ClaymoreDual14Plugin
                //"92fceb00-7236-11e9-b20c-f9f12eb6d835", // CPUMinerPlugin
                "1b7019d0-7237-11e9-b20c-f9f12eb6d835", // GMinerPlugin
                "435f0820-7237-11e9-b20c-f9f12eb6d835", // LolMinerPlugin
                "59bba2c0-b1ef-11e9-8e4e-bb1e2c6e76b4", // MiniZPlugin
                "6c07f7a0-7237-11e9-b20c-f9f12eb6d835", // NBMinerPlugin
                "f5d4a470-e360-11e9-a914-497feefbdfc8", // PhoenixPlugin
                "abc3e2a0-7237-11e9-b20c-f9f12eb6d835", // TeamRedMinerPlugin
                "d47d9b00-7237-11e9-b20c-f9f12eb6d835", // TRexPlugin
                //"3d4e56b0-7238-11e9-b20c-f9f12eb6d835", // XmrStakPlugin
                "5532d300-7238-11e9-b20c-f9f12eb6d835", // ZEnemyPlugin
                //"4aec5ec0-10f8-11ea-bad3-8dea21141bbb", // XmrStakRxPlugin
                "1046ea50-c261-11e9-8e4e-bb1e2c6e76b4", // XMRig
            };
            var bundlePluginsDlls = new Dictionary<string, string>(); 

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
                    try
                    {
                        CheckPluginMetaData(plugin);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\t\tCheckPluginMetaData ERROR!!!!!!!!! {e.Message}");
                    }

                }

                foreach (var plugin in newPlugins)
                {
                    try
                    {
                        if (bundlePlugins.Contains(plugin.PluginUUID))
                        {
                            bundlePluginsDlls.Add(plugin.PluginUUID, filePath);
                        }

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

                    } catch(Exception e)
                    {
                        Console.WriteLine($"\t\t{e.Message}");
                    }

                }
            }
            try
            {
                var preinstalledDlls = Path.Combine(exePath, "miner_plugins");
                if (!Directory.Exists(preinstalledDlls))
                {
                    Directory.CreateDirectory(preinstalledDlls);
                }
                foreach (var kvp in bundlePluginsDlls)
                {
                    var preinstalledDllPlugin = Path.Combine(exePath, "miner_plugins", kvp.Key);
                    var fileName = Path.GetFileName(kvp.Value);
                    var dllPath = Path.Combine(preinstalledDllPlugin, fileName);
                    if (!Directory.Exists(preinstalledDllPlugin))
                    {
                        Directory.CreateDirectory(preinstalledDllPlugin);
                    }
                    File.Copy(kvp.Value, dllPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\t\t{e.Message}");
            }

            // dump our plugin packages
            InternalConfigs.WriteFileSettings(Path.Combine(pluginPackagesFolder, "update.json"), PluginPackageInfos);

            try
            {
                var deleteFolder = Path.Combine(exePath, "miner_plugins", "BrokenMinerPluginUUID");
                Directory.Delete(deleteFolder, true);
            }
            catch (Exception)
            {
            }
        }
    }
}
