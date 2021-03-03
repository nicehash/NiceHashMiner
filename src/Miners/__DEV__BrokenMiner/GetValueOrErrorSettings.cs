using NHM.Common;
using NHM.Common.Configs;
using System;
using System.Collections.Generic;

namespace BrokenMiner
{
    internal static class GetValueOrErrorSettings
    {
        static GetValueOrErrorSettings()
        {
            var defaultSettings = new Dictionary<string, bool>
            {
                //plugin
                { "Version", false }, //breaks at MinerPluginsManager.cs:127
                { "Name", false}, //breaks at MinerPluginsManager.cs:127
                { "Author", false }, //doesn't break anything
                { "PluginUUID", false }, //breaks at MinerPluginsManager.cs:90
                { "CanGroup", false}, //doesn't break anything
                { "CheckBinaryPackageMissingFiles", false}, // broken miner doesn't get downloaded
                { "CreateMiner", false}, // NHML crashes if the mining wants to be started
                { "GetApiMaxTimeout", false}, // doesn't break anything
                { "GetSupportedAlgorithms", false}, //breaks at MinerPluginsManager.cs:116
                { "InitInternals", false}, //breaks at MinerPluginsManager.cs:138
                { "ShouldReBenchmarkAlgorithmOnDevice", false}, // doesn't break anything

                //miner
                { "GetMinerStatsDataAsync", false}, // doesn't break anything
                { "InitMiningLocationAndUsername", false}, // doesn't break anything
                { "InitMiningPairs", false}, // doesn't break anything
                { "StartBenchmark", false}, // doesn't break anything
                { "StartMining", false}, // doesn't break anything
                { "StopMining", false} // doesn't break anything
                //{ "KEY", false }
            };
            var fromFile = false;
            (_settings, fromFile) = InternalConfigs.GetDefaultOrFileSettings(Paths.MinerPluginsPath("BrokenMinerPluginUUID", "settings.json"), defaultSettings);
        }

        private static readonly Dictionary<string, bool> _settings;

        public static T GetValueOrError<T>(string interfacePropertyOrMethod, T value)
        {
            bool shouldThrow = _settings.ContainsKey(interfacePropertyOrMethod) && _settings[interfacePropertyOrMethod];
            if (shouldThrow) throw new Exception($"Throwing on purpose {interfacePropertyOrMethod}. Lets see what breaks?!");
            return value;
        }

        public static void SetError(string interfacePropertyOrMethod)
        {
            bool shouldThrow = _settings.ContainsKey(interfacePropertyOrMethod) && _settings[interfacePropertyOrMethod];
            if (shouldThrow) throw new Exception($"Throwing on purpose {interfacePropertyOrMethod}. Lets see what breaks?!");
        }
    }
}
