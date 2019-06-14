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
            { "PluginUUID", false },
            { "Version", false },
            //{ "KEY", false }
        };

        public static T GetValueOrError<T>(string interfacePropertyOrMethod, T value)
        {
            bool shouldThrow = _settings.ContainsKey(interfacePropertyOrMethod) && _settings[interfacePropertyOrMethod];
            if (shouldThrow) throw new Exception($"Throwing on purpose {interfacePropertyOrMethod}. Lets see what breaks?!");
            return value;
        }
    }
}
