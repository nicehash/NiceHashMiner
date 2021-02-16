using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NhmPackager
{
    class PluginPackageBundler
    {
        public static async Task ExecuteTask(List<string> preinstalledUuids, List<int> supportedVersions)
        {
            var tagVersion = 15;

            await CachePackage.DownloadAndCachePluginPackages(tagVersion);
            UnzipPackages();
        }

        private static void UnzipPackages()
        {
            var cachedPluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "cached_plugins");
            if (!Directory.Exists(cachedPluginsPath)) return;

            var bundledPluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "miner_plugins");
            if (Directory.Exists(bundledPluginsPath)) Directory.Delete(bundledPluginsPath, true);

            var allPackages = Directory.GetFiles(cachedPluginsPath).Where(path => path.EndsWith(".zip"));
            var uuids = allPackages
                .Select(GetPackageUUID)
                .Where(uuid => Guid.TryParse(uuid, out var _))
                .Distinct();
            foreach (var uuid in uuids)
            {
                var packages = allPackages.Where(path => path.Contains(uuid));
                var bundledPackageDllsPath = Path.Combine(Directory.GetCurrentDirectory(), "miner_plugins", uuid, "dlls");
                var packageVersionPaths = packages.Select(package => (package, Path.Combine(bundledPackageDllsPath, GetPackageVersion(package))));

                foreach (var (package, versionPath) in packageVersionPaths)
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
                Console.WriteLine(e.Message);
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

                    //create cached plugins
                    var cachedPluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "cached_plugins");
                    if (!Directory.Exists(cachedPluginsPath)) Directory.CreateDirectory(cachedPluginsPath);

                    var downloadPackages = parsedResponse
                        .Where(IsDownloadOrUpdate)
                        .Where(asset => asset.name.EndsWith(".zip"));
                    foreach (var package in downloadPackages)
                    {
                        Console.WriteLine($"Downloading {package.name}");
                        var success = await DownloadAndCachePluginPackage(package.browser_download_url, package.name, package.updated_at);
                        Console.WriteLine($"Download of {package.name} was {success}.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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
                    var cachedPluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "cached_plugins", name);
                    return File.Exists(cachedPluginsPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("CheckIfPluginPackageExists throw: ", e.Message);
                    return false;
                }
            }

            private static bool CheckIfPluginPackageNeedsUpdate(string name, string lastUpdated)
            {
                try
                {
                    var cachedPluginsUpdatedPath = Path.Combine(Directory.GetCurrentDirectory(), "cached_plugins", $"{name}.txt");
                    if (!File.Exists(cachedPluginsUpdatedPath)) return true;
                    return File.ReadAllText(cachedPluginsUpdatedPath) != lastUpdated;
                }
                catch (Exception e)
                {
                    Console.WriteLine("CheckIfPluginPackageNeedsUpdate throw: ", e.Message);
                    return true;
                }
            }

            private static async Task<bool> DownloadAndCachePluginPackage(string url, string name, string lastUpdated)
            {
                try
                {
                    var cachedPluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "cached_plugins", name);
                    var cachedPluginsUpdatedPath = Path.Combine(Directory.GetCurrentDirectory(), "cached_plugins", $"{name}.txt");
                    using (WebClient wc = new WebClient())
                    {
                        await wc.DownloadFileTaskAsync(new Uri(url), cachedPluginsPath);
                        File.WriteAllText(cachedPluginsUpdatedPath, lastUpdated);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("DownloadAndCachePluginPackage throw: ", e.Message);
                    return false;
                }
            }
        }
    }

    [Serializable]
    public class GitHubRelease
    {
        public string url { get; set; }
        public int id { get; set; }
    }

    [Serializable]
    public class GitHubAsset
    {
        public string name { get; set; }
        public string browser_download_url { get; set; }
        public string updated_at { get; set; }
    }
}
