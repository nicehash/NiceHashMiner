using NHM.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using static NhmPackager.PackagerPaths;

namespace NhmPackager
{
    internal static class PluginPackageBundler
    {
        public static async Task ExecuteTask(IEnumerable<string> preinstalledUuids, IEnumerable<int> supportedVersions)
        {
            foreach (var tagVersion in supportedVersions)
            {
                await CachePackage.DownloadAndCachePluginPackages(tagVersion);
            }
            UnzipPackages(preinstalledUuids);
        }

        private static void UnzipPackages(IEnumerable<string> preinstalledUuids)
        {
            var allPackages = Directory.GetFiles(GetCachedPluginsPath())
                .Where(path => path.EndsWith(".zip"));

            var uuids = allPackages
                .Select(GetPackageUUID)
                .Where(uuid => Guid.TryParse(uuid, out var _))
                .Where(uuid => preinstalledUuids.Contains(uuid))
                .Distinct();

            foreach (var uuid in uuids)
            {
                var sameUuidPackage_VersionPathPairs = allPackages
                    .Where(path => path.Contains(uuid))
                    .Select(package => (package, version: GetPackageVersion(package)))
                    .Select(p => (p.package, versionPath: GetMinerPluginsPath(uuid, "dlls", p.version)));

                foreach (var (package, versionPath) in sameUuidPackage_VersionPathPairs)
                {
                    Directory.CreateDirectory(versionPath);
                    ZipFile.ExtractToDirectory(package, versionPath);
                }
            }
        }

        private static string GetPackageVersion(string path)
        {
            var name = Path.GetFileName(path);
            return name.Split('_')[1].Remove(0, 1);
        }

        private static string GetPackageUUID(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return name.Split('_').LastOrDefault();
        }

        private static async Task<string> GetContentFromApi(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "NHM-Plugin-Bundler");
                    var response = await client.GetAsync(url);
                    var responseContent = response.Content;
                    var content = "";
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                    return content;
                }
            }
            catch (Exception e)
            {
                Logger.Error("PluginPackageBundler", $"GetContentFromApi error: {e}");
            }
            return "";
        }

        static class CachePackage
        {
            public static async Task DownloadAndCachePluginPackages(int tagVersion)
            {
                try
                {
                    var content = await GetContentFromApi($"https://api.github.com/repos/nicehash/NHM_MinerPluginsDownloads/releases/tags/v{tagVersion}.x");
                    var release = Newtonsoft.Json.JsonConvert.DeserializeObject<GitHubRelease>(content);
                    content = await GetContentFromApi($"https://api.github.com/repos/nicehash/NHM_MinerPluginsDownloads/releases/{release.id}/assets?per_page=100");
                    var parsedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GitHubAsset>>(content);

                    // create cached plugins
                    var cachedPluginsPath = GetCachedPluginsPath();
                    if (!Directory.Exists(cachedPluginsPath)) Directory.CreateDirectory(cachedPluginsPath);

                    var downloadPackages = parsedResponse
                        .Where(IsDownloadOrUpdate)
                        .Where(asset => asset.name.EndsWith(".zip"));
                    foreach (var package in downloadPackages)
                    {
                        Logger.Info("CachePackage", $"Downloading {package.name}...");
                        var success = await DownloadAndCachePluginPackage(package.browser_download_url, package.name, package.updated_at);
                        Logger.Info("CachePackage", $"Download of {package.name} was {success}.");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("CachePackage", $"DownloadAndCachePluginPackages error: {e}");
                }
            }

            private static bool IsDownloadOrUpdate(GitHubAsset asset)
            {
                return !CheckIfPluginPackageExists(asset.name) || CheckIfPluginPackageNeedsUpdate(asset.name, asset.updated_at);
            }

            private static bool CheckIfPluginPackageExists(string name)
            {
                try
                {
                    var cachedPluginsPath = GetCachedPluginsPath(name);
                    return File.Exists(cachedPluginsPath);
                }
                catch (Exception e)
                {
                    Logger.Error("CachePackage", $"CheckIfPluginPackageExists error: {e}");
                    return false;
                }
            }

            private static bool CheckIfPluginPackageNeedsUpdate(string name, string lastUpdated)
            {
                try
                {
                    var cachedPluginPackageUpdatedInfoPath = GetCachedPluginsPath($"{name}.txt");
                    if (!File.Exists(cachedPluginPackageUpdatedInfoPath)) return true;
                    return File.ReadAllText(cachedPluginPackageUpdatedInfoPath) != lastUpdated;
                }
                catch (Exception e)
                {
                    Logger.Error("CachePackage", $"CheckIfPluginPackageNeedsUpdate error: {e}");
                    return true;
                }
            }

            private static async Task<bool> DownloadAndCachePluginPackage(string url, string name, string lastUpdated)
            {
                try
                {
                    var cachedPluginsPath = GetCachedPluginsPath(name);
                    var cachedPluginsUpdatedPath = GetCachedPluginsPath($"{name}.txt");
                    using (WebClient wc = new WebClient())
                    {
                        await wc.DownloadFileTaskAsync(new Uri(url), cachedPluginsPath);
                        File.WriteAllText(cachedPluginsUpdatedPath, lastUpdated);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Error("CachePackage", $"DownloadAndCachePluginPackage error: {e}");
                    return false;
                }
            }
        }

        [Serializable]
        private class GitHubRelease
        {
            public string url { get; set; }
            public int id { get; set; }
        }

        [Serializable]
        private class GitHubAsset
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
            public string updated_at { get; set; }
        }
    }
}
