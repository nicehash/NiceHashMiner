using NHM.MinerPlugin;
using NHM.MinerPluginLoader;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Interfaces;
using Newtonsoft.Json;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

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

        private static void AddPluginToPluginPackageInfos(IMinerPlugin plugin)
        {
            var version = new MajorMinorVersion(plugin.Version.Major, plugin.Version.Minor);

            string pluginPackageURL = null;
            if (Checkers.IsMajorVersionSupported(version.major))
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

            var pathMustContain = new List<string>
            {
                "obj",
                "Release",
                "netstandard2.0",
            };
            var pathMustNOTContain = new List<string>
            {
                "MinerPlugin",
                "bin",
                "BrokenMiner",
                "Ethlargement",
                "Ethminer",
                "ExamplePlugin",
                "FakePlugin",
            };

            // get all managed plugin dll's 
            var dllFiles = Directory.GetFiles(pluginsSearchRoot, "*.dll", SearchOption.AllDirectories)
                .Where(filePath => pathMustContain.All(subDir => filePath.Contains(subDir)) && pathMustNOTContain.All(subDir => !filePath.Contains(subDir))).ToList();

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
                //"95b390a0-94eb-11ea-a64d-17be303ea466", // CCMinerTpruvot
                "c9abdb10-94eb-11ea-a64d-17be303ea466", // ClaymoreDual
                //"e294f620-94eb-11ea-a64d-17be303ea466", // CryptoDredge
                "e7a58030-94eb-11ea-a64d-17be303ea466", // GMinerPlugin
                "eb75e920-94eb-11ea-a64d-17be303ea466", // LolMiner
                //"eda6abd0-94eb-11ea-a64d-17be303ea466", // MiniZ - BROKEN
                //"f25fee20-94eb-11ea-a64d-17be303ea466", // NanoMiner
                "f683f550-94eb-11ea-a64d-17be303ea466", // NBMiner
                "fa369d10-94eb-11ea-a64d-17be303ea466", // Phoenix
                //"fd45fff0-94eb-11ea-a64d-17be303ea466", // SRBMiner
                //"01177a50-94ec-11ea-a64d-17be303ea466", // TeamRedMiner
                //"03f80500-94ec-11ea-a64d-17be303ea466", // TRex
                //"074d4a80-94ec-11ea-a64d-17be303ea466", // TTMiner
                //"0a07d6a0-94ec-11ea-a64d-17be303ea466", // WildRig
                "0e0a7320-94ec-11ea-a64d-17be303ea466", // XMRig
                //"116b0340-94ec-11ea-a64d-17be303ea466", // XmrStakRx - BROKEN
                //"1484c660-94ec-11ea-a64d-17be303ea466", // ZEnemy
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
            File.WriteAllText(Path.Combine(pluginPackagesFolder, "update.json"), JsonConvert.SerializeObject(PluginPackageInfos, Formatting.Indented));

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
