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
        }

        public static bool WriteFileSettings(string filePath, string settingsText)
        {
            if (settingsText == null) return false;

            try
            {
                var dirPath = Path.GetDirectoryName(filePath);
                if (Directory.Exists(dirPath) == false)
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.WriteAllText(filePath, settingsText);
                return true;
            }
            catch (Exception ex)
            {
                //Helpers.ConsolePrint(_tag, $"ReadFile {FilePath}: exception {ex}");
                return false;
            }
        }

        // this checks if there are user settings and returns that if not then it saves the given defaultSettings to a file
        public static MinerSystemEnvironmentVariables InitMinerSystemEnvironmentVariablesSettings(string pluginRoot, MinerSystemEnvironmentVariables defaultSettings)
        {
            var pluginRootIntenrals = Path.Combine(pluginRoot, "internals");
            var settingsPath = Path.Combine(pluginRootIntenrals, "MinerSystemEnvironmentVariables.json");
            if (File.Exists(settingsPath))
            {
                var fileSettings = ReadFileSettings<MinerSystemEnvironmentVariables>(settingsPath);
                if (fileSettings != null && fileSettings.UseUserSettings)
                {
                    // use file settings
                    return fileSettings;
                }
            }

            // if we get here then create/override the settings file
            WriteFileSettings(settingsPath, defaultSettings);
            return null;
        }

        public static MinerOptionsPackage InitInternalsHelper(string pluginRoot, MinerOptionsPackage minerOptionsPackage)
        {
            var pluginRootIntenrals = Path.Combine(pluginRoot, "internals");
            var minerOptionsPackagePath = Path.Combine(pluginRootIntenrals, "MinerOptionsPackage.json");
            var fileMinerOptionsPackage = ReadFileSettings<MinerOptionsPackage>(minerOptionsPackagePath);
            if (fileMinerOptionsPackage != null && fileMinerOptionsPackage.UseUserSettings) {
                return fileMinerOptionsPackage;
            }
            else
            { 
                 WriteFileSettings(minerOptionsPackagePath, minerOptionsPackage);
                 return null;
            }
        }
    }
}
