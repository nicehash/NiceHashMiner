using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
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
        /// <param name="filePath">Represents file path to json file</param>
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
                return null;
            }
            return null;
        }

        /// <summary>
        /// WriteFileSettings is used to serialize and write settings to file. It returns true if saving was successfull and false otherwise.
        /// </summary>
        /// <typeparam name="T">Represents type of object that will be serialized</typeparam>
        /// <param name="filePath">Represents file path to json file</param>
        /// <param name="settingsValue">Represent object that will be serialized</param>
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
                return false;
            }
        }

        /// <summary>
        /// WriteFileSettings is used to save text settings to file. It returns true if saving was successfull and false otherwise.
        /// </summary>
        /// <param name="filePath">Represents file path to json file</param> 
        /// <param name="settingsText">Represents text that will be saved to file as it is</param>
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
                return false;
            }
        }

        /// <summary>
        /// InitMinerSystemEnvironmentVariablesSettings checks if there is MinerSystemEnvironmentVariables json file and returns file settings from it
        /// If there is no file, the new one is generated with default settings
        /// </summary>
        /// <param name="pluginRoot">Represents root path of plugin</param>
        /// <param name="defaultSettings">Represents default settings to write if there are no custom settings available</param>
        public static MinerSystemEnvironmentVariables InitMinerSystemEnvironmentVariablesSettings(string pluginRoot, MinerSystemEnvironmentVariables defaultSettings)
        {
            return InitInternalSetting(pluginRoot, defaultSettings, "MinerSystemEnvironmentVariables.json");
        }

        /// <summary>
        /// InitInternalsHelper checks if there is MinerOptionsPackage json file and returns MinerOptionsPackage <see cref="MinerOptionsPackage"/> from it
        /// If file doesn't exist or UseUserSettings equals false, the new file is generated with settings from parameter <paramref name="minerOptionsPackage"/> 
        /// </summary>
        /// <param name="pluginRoot">Represents root path of plugin</param>
        /// <param name="minerOptionsPackage">Represents MinerOptionsPackage that will be written to file if the file doesn't exist and UseUserSettings equals false</param>
        public static MinerOptionsPackage InitInternalsHelper(string pluginRoot, MinerOptionsPackage minerOptionsPackage)
        {
            return InitInternalSetting(pluginRoot, minerOptionsPackage, "MinerOptionsPackage.json");
        }

        /// <summary>
        /// InitMinerReservedPorts checks if there is MinerReservedPorts json file and returns MinerReservedPorts <see cref="MinerReservedPorts"/> from it
        /// If file doesn't exist or UseUserSettings equals false, the new file is generated with settings from parameter <paramref name="minerReservedPorts"/> 
        /// </summary>
        /// <param name="pluginRoot">Represents root path of plugin</param>
        /// <param name="minerReservedPorts">Represents MinerReservedPorts that will be written to file if the file doesn't exist and UseUserSettings equals false</param>
        /// <returns></returns>
        public static MinerReservedPorts InitMinerReservedPorts(string pluginRoot, MinerReservedPorts minerReservedPorts)
        {
            return InitInternalSetting(pluginRoot, minerReservedPorts, "MinerReservedPorts.json");
        }

        // TODO document
        public static MinerApiMaxTimeoutSetting InitMinerApiMaxTimeoutSetting(string pluginRoot, MinerApiMaxTimeoutSetting minerApiMaxTimeoutSetting)
        {
            return InitInternalSetting(pluginRoot, minerApiMaxTimeoutSetting, "MinerApiMaxTimeoutSetting.json");
        }

        public static T InitInternalSetting<T>(string pluginRoot, T setting, string settingName) where T : class, IInternalSetting
        {
            var pluginRootIntenrals = Path.Combine(pluginRoot, "internals");
            var settingPath = Path.Combine(pluginRootIntenrals, settingName);
            var settingPackage = ReadFileSettings<T>(settingPath);
            if (settingPackage != null && settingPackage.UseUserSettings)
            {
                return settingPackage;
            }
            else
            {
                WriteFileSettings(settingPath, setting);
                return null;
            }
        }
    }
}
