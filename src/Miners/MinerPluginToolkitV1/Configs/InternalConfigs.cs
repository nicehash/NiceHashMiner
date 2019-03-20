using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MinerPluginToolkitV1.Configs
{
    public static class InternalConfigs
    {
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
        };

        // all internal configs are JSON based 
        public static T ReadFileSettings<T>(string filePath) where T : class
        {
            if (File.Exists(filePath) == false) return null;

            try
            {
                var dirPath = Path.GetDirectoryName(filePath);
                if (Directory.Exists(dirPath) == false)
                {
                    Directory.CreateDirectory(dirPath);
                }
                if (File.Exists(filePath))
                {
                    var ret = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), _jsonSettings);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                //Helpers.ConsolePrint(_tag, $"ReadFile {FilePath}: exception {ex}");
                return null;
            }
            return null;
        }

        // all internal configs are JSON based 
        public static bool WriteFileSettings<T>(string filePath, T settingsValue) where T : class
        {
            if (settingsValue == null) return false;

            try
            {
                var dirPath = Path.GetDirectoryName(filePath);
                if (Directory.Exists(dirPath) == false)
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.WriteAllText(filePath, JsonConvert.SerializeObject(settingsValue, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                //Helpers.ConsolePrint(_tag, $"ReadFile {FilePath}: exception {ex}");
                return false;
            }
            return false;
        }
    }
}
