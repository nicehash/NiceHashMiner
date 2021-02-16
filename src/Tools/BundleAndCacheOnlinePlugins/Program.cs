using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace BundleAndCacheOnlinePlugins
{
    class Program
    {
        private const string ApiKey_DoNotCommitToGit = "token 43602aa0a9293a60f2a081a5dbe12c654e6a5297";
        private static List<string> bundlePlugins = new List<string>
                    {
                        "eb75e920-94eb-11ea-a64d-17be303ea466", // LolMiner
                        "f683f550-94eb-11ea-a64d-17be303ea466", // NBMiner
                        "0e0a7320-94ec-11ea-a64d-17be303ea466", // XMRig
                        "27315fe0-3b03-11eb-b105-8d43d5bd63be", // Excavator
                    };

        static async Task Main(string[] args)
        {
            var tagVersion = 15;

            await CachePackage.DownloadAndCachePluginPackages(tagVersion);
            UnzipPackages();
            //if (args.Length == 0)
            //{
            //DownloadAllPluginDlls(tagVersion).Wait();
            //}
            //else
            //{
            //    switch (args[0])
            //    {
            //        case "cache":
            //            DownloadAndCachePluginDlls(tagVersion).Wait();
            //            break;
            //        case "bundle":
            //            DownloadAndBundlePluginDlls(tagVersion).Wait();
            //            break;
            //    }
            //}
        }

        private static async Task DownloadAllPluginDlls(int tagVersion)
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
                // crete miner_plugins
                var bundledPluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "miner_plugins");
                if (!Directory.Exists(bundledPluginsPath)) Directory.CreateDirectory(bundledPluginsPath);

                foreach (var plugin in bundlePlugins)
                {
                    Console.WriteLine(plugin);
                    //create dir with uuid
                    var pluginUUID = plugin;

                    var cachedPluginDir = Path.Combine(cachedPluginsPath, pluginUUID);
                    if (!Directory.Exists(cachedPluginDir)) Directory.CreateDirectory(cachedPluginDir);
                    var cachedDllsDir = Path.Combine(cachedPluginDir, "dlls");
                    if (!Directory.Exists(cachedDllsDir)) Directory.CreateDirectory(cachedDllsDir);

                    var bundlePluginDir = Path.Combine(bundledPluginsPath, pluginUUID);
                    if (!Directory.Exists(bundlePluginDir)) Directory.CreateDirectory(bundlePluginDir);
                    var bundleDllsDir = Path.Combine(bundlePluginDir, "dlls");
                    if (!Directory.Exists(bundleDllsDir)) Directory.CreateDirectory(bundleDllsDir);

                    var parsedPlugins = parsedResponse.Where(resp => resp.name.Contains(plugin));
                    Console.WriteLine(parsedPlugins.Count());
                    using (WebClient wc = new WebClient())
                        foreach (var parsedPlugin in parsedPlugins)
                        {
                            //create dir with versions
                            var version = parsedPlugin.name.Split('_')[1].Remove(0, 1);
                            var cachedVersionDir = Path.Combine(cachedDllsDir, version);
                            if (!Directory.Exists(cachedVersionDir)) Directory.CreateDirectory(cachedVersionDir);
                            // this overwrites old file
                            await wc.DownloadFileTaskAsync(new Uri(parsedPlugin.browser_download_url), Path.Combine(cachedVersionDir, parsedPlugin.name));

                            var bundleVersionDir = Path.Combine(bundleDllsDir, version);
                            // we delete directory since extract doesn't overwrite
                            if (Directory.Exists(bundleVersionDir)) Directory.Delete(bundleVersionDir, true); 
                            Directory.CreateDirectory(bundleVersionDir);
                            // copy from cached so we don't need to download again
                            File.Copy(Path.Combine(cachedVersionDir, parsedPlugin.name), Path.Combine(bundleVersionDir, parsedPlugin.name));
                            // this does not overwrite old files
                            ZipFile.ExtractToDirectory(Path.Combine(bundleVersionDir, parsedPlugin.name), bundleVersionDir);

                            //delete zipFile
                            File.Delete(Path.Combine(bundleVersionDir, parsedPlugin.name));
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
            foreach(var uuid in uuids)
            {
                var packages = allPackages.Where(path => path.Contains(uuid));
                var bundledPackageDllsPath = Path.Combine(Directory.GetCurrentDirectory(), "miner_plugins", uuid, "dlls");
                var packageVersionPaths = packages.Select(package => (package, Path.Combine(bundledPackageDllsPath, GetPackageVersion(package))));

                foreach(var (package, versionPath) in packageVersionPaths)
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


        private static async Task DownloadAndBundlePluginDlls(int tagVersion)
        {
            try
            {
                var content = await GetContentFromApi($"https://api.github.com/repos/nicehash/NHM_MinerPluginsDownloads/releases/tags/v{tagVersion}.x");
                var release = Newtonsoft.Json.JsonConvert.DeserializeObject<GitHubRelease>(content);
                content = await GetContentFromApi($"https://api.github.com/repos/nicehash/NHM_MinerPluginsDownloads/releases/{release.id}/assets?per_page=100");
                var parsedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GitHubAsset>>(content);

                // crete miner_plugins
                var bundledPluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "miner_plugins");
                if (!Directory.Exists(bundledPluginsPath)) Directory.CreateDirectory(bundledPluginsPath);

                foreach (var plugin in bundlePlugins)
                {
                    //create dir with uuid
                    var pluginUUID = plugin;

                    var bundlePluginDir = Path.Combine(bundledPluginsPath, pluginUUID);
                    if (!Directory.Exists(bundlePluginDir)) Directory.CreateDirectory(bundlePluginDir);
                    var bundleDllsDir = Path.Combine(bundlePluginDir, "dlls");
                    if (!Directory.Exists(bundleDllsDir)) Directory.CreateDirectory(bundleDllsDir);

                    var parsedPlugins = parsedResponse.Where(resp => resp.name.Contains(plugin));

                    using (WebClient wc = new WebClient())
                        foreach (var parsedPlugin in parsedPlugins)
                        {
                            //create dir with versions
                            var version = parsedPlugin.name.Split('_')[1].Remove(0, 1);

                            var bundleVersionDir = Path.Combine(bundleDllsDir, version);
                            // we delete directory since extract doesn't overwrite
                            if (Directory.Exists(bundleVersionDir)) Directory.Delete(bundleVersionDir, true);
                            Directory.CreateDirectory(bundleVersionDir);
                            await wc.DownloadFileTaskAsync(new Uri(parsedPlugin.browser_download_url), Path.Combine(bundleVersionDir, parsedPlugin.name));
                            // this does not overwrite old files
                            ZipFile.ExtractToDirectory(Path.Combine(bundleVersionDir, parsedPlugin.name), bundleVersionDir);

                            //delete zipFile
                            File.Delete(Path.Combine(bundleVersionDir, parsedPlugin.name));
                            Console.WriteLine($"{parsedPlugin.name} done");
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task<string> GetContentFromApi(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "NHM-Plugin-Bundler");
                    client.DefaultRequestHeaders.Add("Authorization", ApiKey_DoNotCommitToGit);
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
