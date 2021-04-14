using NHM.Common.Configs;
using NHM.Common.Enums;

namespace NHM.Common
{
    public static class BuildOptions
    {
        public class BuildOptionSettings
        {
            public BuildTag BUILD_TAG { get; set; } = BuildTag.PRODUCTION;
            public bool IS_PLUGINS_TEST_SOURCE { get; set; } = false;
            public bool CUSTOM_ENDPOINTS_ENABLED { get; set; } = false;
            // core settings
            public bool FORCE_MINING { get; set; } = false;
            public bool FORCE_PROFITABLE { get; set; } = false;
            public bool SHOW_TDP_SETTINGS { get; set; } = false;
        }

        private static bool _initCalled = false;
        public static void Init()
        {
            if (_initCalled) return;
            _initCalled = true;
            var (buildSettings, buildSettingsFromFile) = InternalConfigs.GetDefaultOrFileSettings(Paths.RootPath("build_settings.json"), new BuildOptionSettings { }, true);
            Logger.Info("BuildOptions", $"Init from file '{buildSettingsFromFile}'");
            BUILD_TAG = buildSettings.BUILD_TAG;
            IS_PLUGINS_TEST_SOURCE = buildSettings.IS_PLUGINS_TEST_SOURCE;
            CUSTOM_ENDPOINTS_ENABLED = buildSettings.CUSTOM_ENDPOINTS_ENABLED;
            FORCE_MINING = buildSettings.FORCE_MINING;
            FORCE_PROFITABLE = buildSettings.FORCE_PROFITABLE;
            SHOW_TDP_SETTINGS = buildSettings.SHOW_TDP_SETTINGS;
            StratumServiceHelpers.InitStratumServiceHelpers();
        }

        public static BuildTag BUILD_TAG { get; private set; } = BuildTag.PRODUCTION;

        public static bool IS_PLUGINS_TEST_SOURCE { get; private set; } = false;

        public static bool CUSTOM_ENDPOINTS_ENABLED { get; private set; } = false;
        // core settings
        public static bool FORCE_MINING { get; private set; } = false;
        public static bool FORCE_PROFITABLE { get; private set; } = false;
        public static bool SHOW_TDP_SETTINGS { get; private set; } = false;
    }
}
