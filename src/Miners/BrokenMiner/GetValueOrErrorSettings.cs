using MinerPluginToolkitV1.Configs;
using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMiner
{
    internal static class GetValueOrErrorSettings
    {
        static GetValueOrErrorSettings()
        {
            var settingsPath = Paths.MinerPluginsPath("BrokenPlugin", "settings.json");
            var globalBenchmarkExceptions = InternalConfigs.ReadFileSettings<Dictionary<string, bool>>(settingsPath);
            if (globalBenchmarkExceptions != null)
            {
                _settings = globalBenchmarkExceptions;
            }
            else
            {
                InternalConfigs.WriteFileSettings(settingsPath, _settings);
            }
        }

        private static Dictionary<string, bool> _settings = new Dictionary<string, bool>
        {
            //plugin
            { "Version", false },
            { "Name", false},
            { "Author", false }, //doesn't break anything
            { "PluginUUID", false },
            { "CanGroup", false}, //NO
            { "CheckBinaryPackageMissingFiles", false}, // vse je ok
            { "CreateMiner", false},
            { "GetApiMaxTimeout", false},
            { "GetSupportedAlgorithms", false},
            { "InitInternals", false},
            { "ShouldReBenchmarkAlgorithmOnDevice", false},

            //miner
            { "GetMinerStatsDataAsync", false},
            { "InitMiningLocationAndUsername", false},
            { "InitMiningPairs", false},
            { "StartBenchmark", false},
            { "StartMining", false},
            { "StopMining", false}
            //{ "KEY", false }
        };

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
