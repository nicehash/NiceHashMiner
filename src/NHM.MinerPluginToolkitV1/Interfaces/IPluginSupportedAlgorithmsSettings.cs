using NHM.Common.Algorithm;
using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;

namespace NHM.MinerPluginToolkitV1.Interfaces
{
    public interface IPluginSupportedAlgorithmsSettings
    {
        PluginSupportedAlgorithmsSettings PluginSupportedAlgorithmsSettings { get; }

        bool UnsafeLimits();

        Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithmsDict();

        List<Algorithm> GetSupportedAlgorithmsForDeviceType(DeviceType deviceType);

        string AlgorithmName(params AlgorithmType[] algorithmTypes);
        double DevFee(params AlgorithmType[] algorithmTypes);
    }
}
