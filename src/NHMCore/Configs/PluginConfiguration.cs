using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Configs
{
    public record PluginConfiguration
    {
        public string PluginName { get; init; }
        public string PluginUUID { get; init; }
        public Dictionary<string, List<string>> SupportedDevicesAlgorithms { get; init; }
        public List<(string FullName, string Uuid, DeviceType deviceType)> Devices { get; init; }
        public List<List<string>> MinerSpecificCommands { get; init; }
    }
}
