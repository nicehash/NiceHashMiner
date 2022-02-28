using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginLoader;
using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
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


            public PluginPackageInfo ToPluginPackageInfo()
            {
                return new PluginPackageInfo
                {
                    PluginUUID = this.PluginUUID,
                    PluginVersion = new Version(this.PluginVersion.major, this.PluginVersion.minor, 0, 0),
                    BinaryPackageHash = this.BinaryPackageHash,
                    PackagePassword = this.PackagePassword,
                    PluginAuthor = this.PluginAuthor,
                    MinerPackageURL = this.MinerPackageURL,
                    PluginDescription = this.PluginDescription,
                    PluginName = this.PluginName,
                    PluginPackageHash = this.PluginPackageHash,
                    PluginPackageURL = this.PluginPackageURL,
                    SupportedDevicesAlgorithms = this.SupportedDevicesAlgorithms
                };
            }
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
            string binaryHash = null;
            if (minerPackageURL != null)
            {
                var filepath = GetTemporaryWorkFolder($"{plugin.PluginUUID}.tmp");
                Logger.Info("MinerPluginsPacker", $"Calculating hash for {plugin.Name}-{plugin.PluginUUID}");
                using (var myWebClient = new WebClient()) myWebClient.DownloadFile(minerPackageURL, filepath);
                using (var sha256Hash = SHA256.Create())
                using (var stream = File.OpenRead(filepath))
                {
                    var hash = sha256Hash.ComputeHash(stream);
                    binaryHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
                File.Delete(filepath);
            }
            string pluginPackageHash = null;
            var pluginZipFileName = GetPluginPackageName(plugin);
            var dllPackageZip = GetPluginsPackagesPath(pluginZipFileName);
            using (var sha256Hash = SHA256.Create())
            using (var stream = File.OpenRead(dllPackageZip))
            {
                var hash = sha256Hash.ComputeHash(stream);
                pluginPackageHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
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
                BinaryPackageHash = binaryHash,
                PluginPackageHash = pluginPackageHash,
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
            List<string> temporaryPath = new List<string>();
            //get all plugin projects paths
            var pluginProjectPaths = Directory.GetFiles(pluginsSearchRoot, "*.csproj", SearchOption.AllDirectories)
                .Where(PathMustNOTContain)
                .Select(projectPath => Directory.GetParent(projectPath).ToString());
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
                temporaryPath.Add(dllPackageZip);
                using (var archive = ZipFile.Open(dllPackageZip, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(dllFilePath, fileName);
                }
                var pluginProjectPath = pluginProjectPaths.FirstOrDefault(path => dllFilePath.Contains(path));

                var dateTimeStr = GetLastCommitDateTime(GetGitCommitHash(pluginProjectPath));
                DateTimeOffset.TryParseExact(dateTimeStr, "ddd MMM d HH:mm:ss yyyy K",
                                 CultureInfo.InvariantCulture,
                                 DateTimeStyles.None, out var dateTime);

                using (ZipArchive archive = ZipFile.Open(dllPackageZip, ZipArchiveMode.Update))
                {
                    archive.Entries.FirstOrDefault().LastWriteTime = dateTime;
                }

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
                    .Select(p => new { local = p.local.ToPluginPackageInfo(), p.online })
                    .ToArray();
                File.WriteAllText(GetPluginsPackagesPath("update_diff.json"), JsonConvert.SerializeObject(different, Formatting.Indented));
            }
        }

        private static bool IsDifferentPluginPackageInfoForJson((PluginPackageInfo online, PluginPackageInfoForJson local) pair)
        {
            var (o, l) = pair;
            if (o == null || l == null) return false;
            var allSame = o.PluginUUID == l.PluginUUID
                //&& o.PluginPackageHash == l.PluginPackageHash  // TODO this is always different for some reason
                && o.BinaryPackageHash == l.BinaryPackageHash
                && o.PluginName == l.PluginName
                && o.PluginVersion.Major == l.PluginVersion.major
                && o.PluginVersion.Minor == l.PluginVersion.minor
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

        private class NoKeepAlivesWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    ((HttpWebRequest)request).KeepAlive = false;
                }

                return request;
            }
        }


        private static List<PluginPackageInfo> GetOnlineMinerPlugins()
        {
            try
            {
                using (var client = new NoKeepAlivesWebClient())
                {
                    string s = client.DownloadString("https://miner-plugins.nicehash.com/api/plugins");
                    return JsonConvert.DeserializeObject<List<PluginPackageInfo>>(s, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        Culture = CultureInfo.InvariantCulture
                    });
                }
            }
            catch (Exception e)
            {
                Logger.Error("MinerPluginsPacker", $"Error occured while getting online miner plugins: {e.Message}");
            }
            return null;
        }        

        public static string GetGitCommitHash(string pluginProjectPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"rev-list --all {pluginProjectPath}\\**",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (var getGitHash = new Process { StartInfo = startInfo })
            {
                var ok = getGitHash.Start();
                if (!getGitHash.StandardOutput.EndOfStream)
                    return getGitHash.StandardOutput.ReadLine();
            }
            return null;
        }

        public static string GetLastCommitDateTime(string commitHash)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"show {commitHash}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (var getGitCommitDateTime = new Process { StartInfo = startInfo })
            {
                getGitCommitDateTime.Start();

                while (!getGitCommitDateTime.StandardOutput.EndOfStream)
                {
                    var line = getGitCommitDateTime.StandardOutput.ReadLine();
                    if (line.StartsWith("Date:")) return line.Replace("Date:", "").Trim();
                }
            }
            return null;
        }
    }
}
