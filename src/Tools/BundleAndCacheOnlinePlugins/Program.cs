using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BundleAndCacheOnlinePlugins
{
    class Program
    {
        static void Main(string[] args)
        {
            var tagVersion = 15;
            //var pluginPackages = $"https://github.com/nicehash/NHM_MinerPluginsDownloads/releases/download/v{majorVersion}.x/";
            //https://api.github.com/repos/nicehash/NHM_MinerPluginsDownloads/releases/34511354/assets
            DownloadAllPluginDlls(tagVersion).Wait();
        }

        private static async Task DownloadAllPluginDlls(int tagVersion)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "NHM-Plugin-Bundler");
                    client.DefaultRequestHeaders.Add("Authorization", "token c79b8ba665c6fbee7a40e14a309f279070aa035e");
                    var response = await client.GetAsync($"https://api.github.com/repos/nicehash/NHM_MinerPluginsDownloads/releases/tags/v{tagVersion}.x");
                    var responseContent = response.Content;
                    var content = "";
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        content = await reader.ReadToEndAsync();
                    }

                    var release = Newtonsoft.Json.JsonConvert.DeserializeObject<GitHubRelease>(content);

                    //assets
                    response = await client.GetAsync($"https://api.github.com/repos/nicehash/NHM_MinerPluginsDownloads/releases/{release.id}/assets");
                    responseContent = response.Content;
                    content = "";
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        content = await reader.ReadToEndAsync();
                    }

                    var parsedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GitHubAsset>>(content);

                    var bundlePlugins = new List<string>
                    {
                        "eb75e920-94eb-11ea-a64d-17be303ea466", // LolMiner
                        "f683f550-94eb-11ea-a64d-17be303ea466", // NBMiner
                        "0e0a7320-94ec-11ea-a64d-17be303ea466", // XMRig
                        "27315fe0-3b03-11eb-b105-8d43d5bd63be", // Excavator
                    };

                    var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "miner_plugins");
                    if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);

                    foreach (var plugin in bundlePlugins)
                    { 
                        //create dir with uuid
                        var pluginUUID = plugin;
                        if (!Directory.Exists(Path.Combine(pluginsPath, pluginUUID))) Directory.CreateDirectory(Path.Combine(pluginsPath, pluginUUID));


                        var parsedPlugins = parsedResponse.Where(resp => resp.name.Contains(plugin));
                        foreach(var parsedPlugin in parsedPlugins)
                        {
                            //create dir with versions
                            var version = parsedPlugin.name.Split('_');


                        }
                    }

                    //if (_isSourceDifferent == false)
                    //{
                    //    Program.CachedContents.TryGetValue(_apiPathPart, out var cachedBody);
                    //    if (cachedBody == null) Program.CachedContents.Add(_apiPathPart, body); // in case of first run
                    //    if (Program.CachedContents[_apiPathPart] != body)
                    //    {
                    //        _isSourceDifferent = true;
                    //        _isContentSame = false;
                    //        Program.CachedContents[_apiPathPart] = body;
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task<string> GetContentFromApi()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "NHM-Plugin-Bundler");
                    client.DefaultRequestHeaders.Add("Authorization", "token c79b8ba665c6fbee7a40e14a309f279070aa035e");
                    var response = await client.GetAsync($"https://api.github.com/repos/nicehash/NHM_MinerPluginsDownloads/releases/34511354/assets");
                    var responseContent = response.Content;
                    var content = "";
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                    return content;
                }
            }
            catch (Exception ex)
            {

            }
            return "";
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
    }
}
