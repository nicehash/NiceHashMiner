using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginLoader;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static NhmPackager.PackagerFileDirectoryUtils;
using static NhmPackager.PackagerPaths;

namespace NhmPackager
{
    internal static class MinerPluginsPacker
    {
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
            var versionStr = $"v{plugin.Version.Major}.{plugin.Version.Minor}";
            // TODO workaround to check if it is built with the Toolkit
            if (plugin is IInitInternals) versionStr = $"{versionStr}_mptoolkitV1";
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
                            //throw new Exception($"CheckPluginMetaData {plugin.Name}-{plugin.PluginUUID} Invalid name '{name}' for algorithm type '{algo}'");
                        }
                    }
                }
            }
        }

        private static PluginPackageInfo ToPluginToPluginPackageInfos(IMinerPlugin plugin)
        {
            var version = plugin.Version;
            
            if (!Checkers.IsMajorVersionSupported(version.Major))
            {
                throw new Exception($"Plugin version '{version.Major}' not supported. Make sure you add the download link for this version");
            }
            string minerPackageURL = null;
            if (plugin is IMinerBinsSource binsSource)
            {
                minerPackageURL = binsSource.GetMinerBinsUrlsForPlugin().FirstOrDefault();
            }
            Logger.Info("MinerPluginsPacker", $"Calculating hash for {plugin.Name}-{plugin.PluginUUID} - url {minerPackageURL}");
            string binaryHash = minerPackageURL != null ? FileHelpers.GetURLFileSHA256Checksum(minerPackageURL) : null;

            var pluginZipFileName = GetPluginPackageName(plugin);
            var dllPackageZip = GetPluginsPackagesPath(pluginZipFileName);
            string pluginPackageHash = FileHelpers.GetFileSHA256Checksum(dllPackageZip);
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
            var packageInfo = new PluginPackageInfo
            {
                PluginUUID = plugin.PluginUUID,
                BinaryPackageHash = binaryHash,
                PluginPackageHash = pluginPackageHash,
                PluginAuthor = "info@nicehash.com",
                PluginName = plugin.Name,
                PluginVersion = version,
                PluginPackageURL = $"https://github.com/nicehash/NHM_MinerPluginsDownloads/releases/download/v{version.Major}.x/" + GetPluginPackageName(plugin),
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
                "PluginsToSign"
            };
        private static bool PathMustContain(string path) => _pathMustContain.All(subDir => path.Contains(subDir));

        private static readonly string[] _pathMustNOTContain = new string[]
            {
                "MinerPlugin",
                "bin",
                "BrokenMiner",
                "Ethlargement",
                "Ethminer",
                "ExamplePlugin",
                "FakePlugin",
                "CryptoDredge",
                "ZEnemy",
                "WildRig",
                "TTMiner",
                "TRex",
                "TeamRedMiner",
                "SRBMiner",
                "Phoenix",
                "MiniZ",
                "GMiner",
            };
        private static bool PathMustNOTContain(string path) => _pathMustNOTContain.All(subDir => !path.Contains(subDir));

        public static readonly IReadOnlyList<string> PreInstalledPlugins = new string[]
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

            Logger.Info("MinerPluginsPacker", "Checking and packaging plugins:");
            foreach (var (dllFilePath, plugin) in plugins)
            {
                Logger.Info("MinerPluginsPacker", $"Checking {plugin.Name} plugin");
                CheckPluginMetaData(plugin);

                var pluginZipFileName = GetPluginPackageName(plugin);
                var dllPackageZip = GetPluginsPackagesPath(pluginZipFileName);
                Logger.Info("MinerPluginsPacker", $"Packaging: {dllPackageZip}");
                var fileName = Path.GetFileName(dllFilePath);
                using var archive = ZipFile.Open(dllPackageZip, ZipArchiveMode.Create);
                archive.CreateEntryFromFile(dllFilePath, fileName);
            }
            Logger.Info("MinerPluginsPacker", "Packaging pre inastalled plugins:");
            var preInstalledPlugins = plugins.Where(pair => PreInstalledPlugins.Contains(pair.plugin.PluginUUID));
            foreach (var (dllFilePath, plugin) in preInstalledPlugins)
            {
                var preinstalledDllPlugin = GetMinerPluginsPath(plugin.PluginUUID);
                var fileName = Path.GetFileName(dllFilePath);
                var dllPath = Path.Combine(preinstalledDllPlugin, fileName);
                Paths.EnsureDirectoryPath(preinstalledDllPlugin);
                File.Copy(dllFilePath, dllPath);
            }

            // dump our plugin packages
            var pluginPackageInfos = plugins
                .Select(pair => ToPluginToPluginPackageInfos(pair.plugin))
                .ToList();
            File.WriteAllText(GetPluginsPackagesPath("update.json"), JsonConvert.SerializeObject(pluginPackageInfos, Formatting.Indented));
            var onlinePlugins = GetOnlineMinerPlugins();
            if (onlinePlugins != null)
            {
                var pairs = onlinePlugins
                    .Select(online => (online, local: pluginPackageInfos.FirstOrDefault(l => online.PluginUUID == l.PluginUUID)))
                    .ToArray();
                var different = pairs
                    .Where(IsDifferentPluginPackageInfoForJson)
                    .ToArray();
                File.WriteAllText(GetPluginsPackagesPath("update_diff.json"), JsonConvert.SerializeObject(different, Formatting.Indented));
            }
        }

        private static bool IsDifferentPluginPackageInfoForJson((PluginPackageInfo online, PluginPackageInfo local) pair)
        {
            var (o, l) = pair;
            if (o == null || l == null) return false;
            var allSame = o.PluginUUID == l.PluginUUID
                //&& o.PluginPackageHash == l.PluginPackageHash  // TODO this is always different for some reason
                && o.BinaryPackageHash == l.BinaryPackageHash
                && o.PluginName == l.PluginName
                && o.PluginVersion.Major == l.PluginVersion.Major
                && o.PluginVersion.Minor == l.PluginVersion.Minor
                && o.PluginPackageURL == l.PluginPackageURL
                && o.MinerPackageURL == l.MinerPackageURL
                // SupportedDevicesAlgorithms CHECK below
                && o.PluginAuthor == l.PluginAuthor
                && o.PluginDescription == l.PluginDescription
                && o.PackagePassword == l.PackagePassword
                ;
            if (!allSame) return true;

            // SupportedDevicesAlgorithms check
            var mergedDeviceKeys = new HashSet<string>(o.SupportedDevicesAlgorithms.Keys);
            mergedDeviceKeys.UnionWith(l.SupportedDevicesAlgorithms.Keys);
            foreach (var deviceType in mergedDeviceKeys)
            {
                if (!o.SupportedDevicesAlgorithms.ContainsKey(deviceType)) return true;
                if (!l.SupportedDevicesAlgorithms.ContainsKey(deviceType)) return true;

                var o_Algos = o.SupportedDevicesAlgorithms[deviceType];
                var l_Algos = l.SupportedDevicesAlgorithms[deviceType];
                var o_AlgosExceptCount = o_Algos.Except(l_Algos).Count();
                var l_AlgosExceptCount = l_Algos.Except(o_Algos).Count();
                var sameZeroDiffCount = o_AlgosExceptCount == l_AlgosExceptCount && l_AlgosExceptCount == 0;
                if (!sameZeroDiffCount) return true; 
            }

            return false;
        }

        private static List<PluginPackageInfo> GetOnlineMinerPluginsDev() => GetOnlineMinerPlugins("https://miner-plugins-test-dev.nicehash.com/api/plugins");

        private static List<PluginPackageInfo> GetOnlineMinerPlugins(string url = "https://miner-plugins.nicehash.com/api/plugins")
        {
            List<PluginPackageInfo> getPlugins(int version)
            {
                using var client = new NoKeepAliveHttpClient();
                string s = client.GetStringAsync($"{url}?v={version}").Result;
                return JsonConvert.DeserializeObject<List<PluginPackageInfo>>(s, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Culture = CultureInfo.InvariantCulture
                });
            }
            try
            {
                var random = new Random();
                var onlinePluginsAllVersions = new List<PluginPackageInfo>();
                foreach (var version in Enumerable.Range(15, Checkers.GetLatestSupportedVersion))
                {
                    onlinePluginsAllVersions.AddRange(getPlugins(version));
                    Task.Delay(TimeSpan.FromMilliseconds(100 + random.Next(0, 200))).Wait();
                }
                return onlinePluginsAllVersions.OrderBy(p => p.PluginUUID)
                                        .ThenByDescending(p => p.PluginVersion)
                                        .ToList();
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsPacker", $"Error occured while getting online miner plugins: {e.Message}");
            }
            return null;
        }

        public static void CheckOnlinePlugins()
        {
            bool isHashOk(string fileUrl, string hash) => FileHelpers.GetURLFileSHA256Checksum(fileUrl) == hash;
            string pluginStr(PluginPackageInfo p) => $"{p.PluginUUID} {p.PluginVersion.Major}.{p.PluginVersion.Minor} {p.PluginName} ";
            void checkAndIterate(List<PluginPackageInfo> plugins)
            {
                foreach (var p in plugins)
                {
                    var pluginOk = isHashOk(p.PluginPackageURL, p.PluginPackageHash);
                    var minerOk = isHashOk(p.MinerPackageURL, p.BinaryPackageHash);
                    var msg = (pluginOk, minerOk) switch
                    {
                        (true, true) => $"OK",
                        _ => $"ERROR (pluginOk, minerOk)={(pluginOk, minerOk)}",
                    };
                    Logger.Info("MinerPluginsPacker", $"{pluginStr(p)} {msg}");
                }
            }
            Logger.Info("MinerPluginsPacker", "Checking production hashes START");
            checkAndIterate(GetOnlineMinerPlugins());
            Logger.Info("MinerPluginsPacker", "Checking production hashes DONE\n\n");

            Logger.Info("MinerPluginsPacker", "Checking DEV hashes START");
            checkAndIterate(GetOnlineMinerPluginsDev());
            Logger.Info("MinerPluginsPacker", "Checking DEV hashes DONE\n\n");
        }

    }
}
