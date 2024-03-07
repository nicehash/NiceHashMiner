using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NHM.Common
{
    public static class PluginsListCacheManager
    {
        //private static DateTime lastVersion = DateTime.MinValue;
        readonly static string dateFilePath = Paths.AppRootPath("cachedplugindate");
        readonly static string pluginFilePath = Paths.AppRootPath("cachedplugins");
        private static readonly string format = "d MMM yyyy HH:mm 'GMT'";
        private static readonly string _TAG = "PluginsListCacheManager";

        private static DateTime GetLastVersion(string version)
        {
            return GetCachedDateTime(version);
        }
        private static void SetLastVersion(DateTime dt, string version)
        {
            WriteNewCachedDateTime(dt, version);
        }

        public static DateTime GetCachedDateTime(string version)
        {
            string filePath = $"{dateFilePath}_v{version}.json";
            if (!File.Exists(filePath)) return DateTime.MinValue;
            try
            {
                string fileContent = File.ReadAllText(filePath);
                DateTime deserializedDateTime = JsonSerializer.Deserialize<DateTime>(fileContent);
                return deserializedDateTime;
            }
            catch (Exception ex)
            {
                Logger.Error(_TAG, ex.Message);
                return DateTime.MinValue; 
            }
        }

        public static void WriteNewCachedDateTime(DateTime newCachedDateTime, string version)
        {
            string filePath = $"{dateFilePath}_v{version}.json";
            try
            {
                string jsonString = JsonSerializer.Serialize(newCachedDateTime);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Logger.Error(_TAG, ex.Message);
            }
        }

        public static async Task<bool> CheckIfShouldUpdateAndUpdateLatestDate(string url, int version)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "PluginsListCacheManager");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, $"{url}?v={version}");
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK) return true;
                    if (response.Content.Headers.NonValidated.Contains("Last-Modified"))
                    {
                        var lastModifiedStr = response.Content.Headers.NonValidated.GetValueOrDefault("Last-Modified");
                        if (DateTime.TryParseExact(lastModifiedStr.ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
                        {
                            var lastVer = GetLastVersion(version.ToString());
                            if (lastVer == result)
                            {
                                Logger.Debug("PluginsListCacheManager", $"Plugin update loop: versions {version} - no update needed - {result}");
                                return false;
                            }
                            Logger.Debug("PluginsListCacheManager", $"Plugin update loop: versions {version} - WILL UPDATE - {lastVer}/{result}");
                            SetLastVersion(result, version.ToString());
                            return true;
                        }
                    }             
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(_TAG, $"Error: {ex.Message}");
            }
            return true;
        }

        public static bool WritePluginCache(int version, string plugins)
        {
            string filePath = $"{pluginFilePath}_v{version}.json";
            try
            {
                File.WriteAllText(filePath, plugins);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(_TAG, $"Error: {ex.Message}");
            }
            return false;
        }

        public static string ReadPluginCache(int version)
        {
            string filePath = $"{pluginFilePath}_v{version}.json";
            try
            {
                if (!File.Exists(filePath)) return string.Empty;
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Logger.Error(_TAG, $"Error: {ex.Message}");
            }
            return string.Empty;
        }
    }
}
