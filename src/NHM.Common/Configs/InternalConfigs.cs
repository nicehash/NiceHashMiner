using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;

namespace NHM.Common.Configs
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
            Culture = CultureInfo.InvariantCulture,
            ObjectCreationHandling = ObjectCreationHandling.Replace
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
                var ret = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), _jsonSettings);
                return ret;
            }
            catch (Exception e)
            {
                Logger.Error("InternalConfigs", $"Error occured while reading file settings from {filePath}: {e.Message}");
                return null;
            }
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
        /// InitInternalSetting initialize and returns a given setting of type T where T must implement IInternalSetting <see cref="IInternalSetting"/>.
        /// If file doesn't exist or UseUserSettings equals false, the new file is generated with settings from parameter <paramref name="setting"/> to file <paramref name="settingFileName"/>.
        /// If file exist and UseUserSettings equals true, it will read and parse settings from the file <paramref name="settingFileName"/>.
        /// </summary>
        /// <param name="pluginRoot">Represents root path of plugin</param>
        /// <param name="setting">Represents setting of type T that will be written to file if the file doesn't exist and UseUserSettings equals false</param>
        /// <param name="settingFileName">Represents file name user for reading and writing the <paramref name="setting"/></param>
        /// <returns></returns>
        public static T InitInternalSetting<T>(string pluginRoot, T setting, string settingFileName) where T : class, IInternalSetting
        {
            var pluginRootIntenrals = Path.Combine(pluginRoot, "internals");
            var settingPath = Path.Combine(pluginRootIntenrals, settingFileName);
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

        private static bool UseFileSettings<T>(T fileSettings) where T : class
        {
            if (fileSettings != null && fileSettings is IInternalSetting internals) return internals.UseUserSettings;
            return fileSettings != null;
        }

        public static (T settings, bool fromFile) GetDefaultOrFileSettings<T>(string settingFilePath, T defaultSettings, bool writeDefaultSettingsToFile = true) where T : class
        {
            var fileSettings = ReadFileSettings<T>(settingFilePath);
            if (UseFileSettings(fileSettings)) return (fileSettings, true);
            if (writeDefaultSettingsToFile) WriteFileSettings(settingFilePath, defaultSettings);
            return (defaultSettings, false);
        }
    }
}
