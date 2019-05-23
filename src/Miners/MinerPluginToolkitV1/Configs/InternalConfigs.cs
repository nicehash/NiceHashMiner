using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MinerPluginToolkitV1.Configs
{
    /// <summary>
    /// InternalConfigs is the main class for configs residing in internals folder
    /// </summary>
    public static class InternalConfigs
    {
        // All internal configs are JSON based

        /// <summary>
        /// settings for json serialization
        /// </summary>
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
        };

        /// <summary>
        /// ReadFileSettings method is used to deserialize and read settings from file. It returns deserialized object.
        /// <typeparam name="T"></typeparam> represents type of deserialized object
        /// <param name="filePath"></param> represents file path to json file
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
            catch (Exception e)
            {
                Logger.Error("InternalConfigs", $"Error occured while reading file settings from {filePath}: {e.Message}");
                //Helpers.ConsolePrint(_tag, $"ReadFile {FilePath}: exception {e}");
                return null;
            }
            return null;
        }

        /// <summary>
        /// WriteFileSettings is used to serialize and write settings to file. It returns true if saving was successfull and false otherwise.
        /// </summary>
        /// <typeparam name="T"></typeparam> represents type of object that will be serialized
        /// <param name="filePath"></param> represents file path to json file
        /// <param name="settingsValue"></param> represent object that will be serialized
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
            catch (Exception e)
            {
                Logger.Error("InternalConfigs", $"Error occured while writing file settings to {filePath}: {e.Message}");
                //Helpers.ConsolePrint(_tag, $"ReadFile {FilePath}: exception {e}");
                return false;
            }
        }

        /// <summary>
        /// WriteFileSettings is used to save text settings to file. It returns true if saving was successfull and false otherwise.
        /// </summary>
        /// <param name="filePath"></param> represents file path to json file
        /// <param name="settingsText"></param> represents text that will be saved to file as it is
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
            catch (Exception e)
            {
                Logger.Info("InternalConfigs", $"Error occured while writing file settings to {filePath}: {e.Message}");
                //Helpers.ConsolePrint(_tag, $"ReadFile {FilePath}: exception {e}");
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

        public static MinerReservedPorts InitMinerReservedPorts(string pluginRoot, MinerReservedPorts minerReservedPorts)
        {
            var pluginRootIntenrals = Path.Combine(pluginRoot, "internals");
            var minerOptionsPackagePath = Path.Combine(pluginRootIntenrals, "MinerReservedPorts.json");
            var fileMinerOptionsPackage = ReadFileSettings<MinerReservedPorts>(minerOptionsPackagePath);
            if (fileMinerOptionsPackage != null && fileMinerOptionsPackage.UseUserSettings)
            {
                return fileMinerOptionsPackage;
            }
            else
            {
                WriteFileSettings(minerOptionsPackagePath, minerReservedPorts);
                return null;
            }
        }
    }
}
