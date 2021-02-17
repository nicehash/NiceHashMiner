using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginLoader;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using static NhmPackager.PackagerFileDirectoryUtils;
using static NhmPackager.PackagerPaths;

namespace NhmPackager
{
    internal static class MinerPluginsPacker
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
                            throw new Exception($"CheckPluginMetaData {plugin.Name}-{plugin.PluginUUID} Invalid name '{name}' for algorithm type '{algo}'");
                        }
                    }
                }
            }
        }

        private static PluginPackageInfoForJson ToPluginToPluginPackageInfos(IMinerPlugin plugin)
        {
            var version = new MajorMinorVersion(plugin.Version.Major, plugin.Version.Minor);

            if (!Checkers.IsMajorVersionSupported(version.major))
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
            if (pluginMetaInfo == null) return null;
            var packagePassword = plugin is IGetBinsPackagePassword p ? p.BinsPackagePassword : null;
            var packageInfo = new PluginPackageInfoForJson
            {
                PluginUUID = plugin.PluginUUID,
                PluginAuthor = "info@nicehash.com",
                PluginName = plugin.Name,
                PluginVersion = version,
                PluginPackageURL = $"https://github.com/nicehash/NHM_MinerPluginsDownloads/releases/download/v{version.major}.x/" + GetPluginPackageName(plugin),
                MinerPackageURL = minerPackageURL,
                SupportedDevicesAlgorithms = TransformToPluginPackageInfoSupportedDevicesAlgorithms(pluginMetaInfo.SupportedDevicesAlgorithms),
                // TODO enhance this with the bins version
                PluginDescription = $"Miner Binary Version '{binaryVersion}'.\n\n" + pluginMetaInfo.PluginDescription,
                PackagePassword = packagePassword
            };
            return packageInfo;
        }

        private static readonly List<string> _pathMustContain = new List<string>
            {
                "obj",
                "Release",
                "netstandard2.0",
            };
        private static bool PathMustContain(string path) => _pathMustContain.All(subDir => path.Contains(subDir));

        private static readonly List<string> _pathMustNOTContain = new List<string>
            {
                "MinerPlugin",
                "bin",
                "BrokenMiner",
                "Ethlargement",
                "Ethminer",
                "ExamplePlugin",
                "FakePlugin",
            };
        private static bool PathMustNOTContain(string path) => _pathMustNOTContain.All(subDir => !path.Contains(subDir));

        public static readonly IReadOnlyList<string> PreInstalledPlugins = new List<string>
            {
                //"e294f620-94eb-11ea-a64d-17be303ea466", // CryptoDredge
                //"e7a58030-94eb-11ea-a64d-17be303ea466", // GMinerPlugin
                "eb75e920-94eb-11ea-a64d-17be303ea466", // LolMiner
                //"eda6abd0-94eb-11ea-a64d-17be303ea466", // MiniZ - BROKEN
                //"f25fee20-94eb-11ea-a64d-17be303ea466", // NanoMiner
                "f683f550-94eb-11ea-a64d-17be303ea466", // NBMiner
                //"fa369d10-94eb-11ea-a64d-17be303ea466", // Phoenix
                //"fd45fff0-94eb-11ea-a64d-17be303ea466", // SRBMiner
                //"01177a50-94ec-11ea-a64d-17be303ea466", // TeamRedMiner
                //"03f80500-94ec-11ea-a64d-17be303ea466", // TRex
                //"074d4a80-94ec-11ea-a64d-17be303ea466", // TTMiner
                //"0a07d6a0-94ec-11ea-a64d-17be303ea466", // WildRig
                "0e0a7320-94ec-11ea-a64d-17be303ea466", // XMRig
                //"1484c660-94ec-11ea-a64d-17be303ea466", // ZEnemy
                "27315fe0-3b03-11eb-b105-8d43d5bd63be", // Excavator
            };
        public static void Execute(string pluginsSearchRoot)
        {
            PluginBase.IS_CALLED_FROM_PACKER = true;

            RecreateDirectoryIfExists(GetPluginsPackagesPath());

            // get all managed plugin dll's 
            var plugins = Directory.GetFiles(pluginsSearchRoot, "*.dll", SearchOption.AllDirectories)
                .Where(PathMustContain)
                .Where(PathMustNOTContain)
                .SelectMany(dllFilePath => MinerPluginHost.LoadPluginsFromDllFile(dllFilePath).Select(plugin => (dllFilePath, plugin)));
            // dump our plugin packages
            var pluginPackageInfos = plugins
                .Select(pair => ToPluginToPluginPackageInfos(pair.plugin))
                .ToList();
            File.WriteAllText(GetPluginsPackagesPath("update.json"), JsonConvert.SerializeObject(pluginPackageInfos, Formatting.Indented));

            Logger.Info("MinerPluginsPacker", "Checking and packaging plugins:");
            foreach (var (dllFilePath, plugin) in plugins)
            {
                Logger.Info("MinerPluginsPacker", $"Checking {plugin.Name} plugin");
                CheckPluginMetaData(plugin);

                var pluginZipFileName = GetPluginPackageName(plugin);
                var dllPackageZip = GetPluginsPackagesPath(pluginZipFileName);
                Logger.Info("MinerPluginsPacker", $"Packaging: {dllPackageZip}");
                var fileName = Path.GetFileName(dllFilePath);
                using (var archive = ZipFile.Open(dllPackageZip, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(dllFilePath, fileName);
                }
            }
            Logger.Info("MinerPluginsPacker", "Packaging pre inastalled plugins:");
            var preInstalledPlugins = plugins.Where(pair => PreInstalledPlugins.Contains(pair.plugin.PluginUUID));
            foreach (var (dllFilePath, plugin) in preInstalledPlugins)
            {
                var preinstalledDllPlugin = GetMinerPluginsPath(plugin.PluginUUID);
                var fileName = Path.GetFileName(dllFilePath);
                var dllPath = Path.Combine(preinstalledDllPlugin, fileName);
                if (!Directory.Exists(preinstalledDllPlugin)) Directory.CreateDirectory(preinstalledDllPlugin);
                File.Copy(dllFilePath, dllPath);
            }
        }
    }
}
