using NHM.Common.Enums;
using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1
{
    public class PluginMetaInfo
    {
        public string PluginDescription { get; set; }
        public Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithms { get; set; }
    }
}
