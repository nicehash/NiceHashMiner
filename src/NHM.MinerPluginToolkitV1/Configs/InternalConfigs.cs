using NHM.MinerPluginToolkitV1.Interfaces;
using System;


namespace NHM.MinerPluginToolkitV1.Configs
{
    [Obsolete("Use NHM.Common.Configs.InternalConfigs", true)]
    public static class InternalConfigs
    {
        [Obsolete("Use NHM.Common.Configs.InternalConfigs.ReadFileSettings", false)]
        public static T ReadFileSettings<T>(string filePath) where T : class => NHM.Common.Configs.InternalConfigs.ReadFileSettings<T>(filePath);

        [Obsolete("Use NHM.Common.Configs.InternalConfigs.WriteFileSettings", false)]
        public static bool WriteFileSettings<T>(string filePath, T settingsValue) where T : class => NHM.Common.Configs.InternalConfigs.WriteFileSettings(filePath, settingsValue);

        [Obsolete("Use NHM.Common.Configs.InternalConfigs.WriteFileSettings", false)]
        public static bool WriteFileSettings(string filePath, string settingsText) => NHM.Common.Configs.InternalConfigs.WriteFileSettings(filePath, settingsText);

        [Obsolete("Use NHM.Common.Configs.InternalConfigs.InitInternalSetting", false)]
        public static T InitInternalSetting<T>(string pluginRoot, T setting, string settingFileName) where T : class, IInternalSetting => NHM.Common.Configs.InternalConfigs.InitInternalSetting(pluginRoot, setting, settingFileName);
    }
}
