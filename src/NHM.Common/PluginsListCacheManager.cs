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
        readonly static string dateFilePath = Paths.AppRootPath("cachedplugindate.json");
        readonly static string pluginFilePath = Paths.AppRootPath("cachedplugins.json");
        private static readonly string format = "d MMM yyyy HH:mm 'GMT'";
        private static readonly string _TAG = "PluginsListCacheManager";

        private static DateTime LastVersion
        {
            get
            {
                return GetCachedDateTime();
            }
            set
            {
                WriteNewCachedDateTime(value);
            }
        }

        public static DateTime GetCachedDateTime()
        {
            if (!File.Exists(dateFilePath)) return DateTime.MinValue;
            try
            {
                string fileContent = File.ReadAllText(dateFilePath);
                DateTime deserializedDateTime = JsonSerializer.Deserialize<DateTime>(fileContent);
                return deserializedDateTime;
            }
            catch (Exception ex)
            {
                Logger.Error(_TAG, ex.Message);
                return DateTime.MinValue; 
            }
        }

        public static void WriteNewCachedDateTime(DateTime newCachedDateTime)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(newCachedDateTime);
                File.WriteAllText(dateFilePath, jsonString);
            }
            catch (Exception ex)
            {
                Logger.Error(_TAG, ex.Message);
            }
        }

        public static async Task<bool> CheckIfShouldUpdateAndUpdateLatestDate(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "PluginsListCacheManager");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK) return true;
                    if (response.Content.Headers.NonValidated.Contains("Last-Modified"))
                    {
                        var lastModifiedStr = response.Content.Headers.NonValidated.GetValueOrDefault("Last-Modified");
                        if (DateTime.TryParseExact(lastModifiedStr.ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
                        {
                            if (LastVersion == result) return false;
                            LastVersion = result;
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

        public static bool WritePluginCache(string plugins)
        {
            try
            {
                File.WriteAllText(pluginFilePath, plugins);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(_TAG, $"Error: {ex.Message}");
            }
            return false;
        }

        public static string ReadPluginCache()
        {
            try
            {
                if (!File.Exists(pluginFilePath)) return string.Empty;
                return File.ReadAllText(pluginFilePath);
            }
            catch (Exception ex)
            {
                Logger.Error(_TAG, $"Error: {ex.Message}");
            }
            return string.Empty;
        }
    }
}
