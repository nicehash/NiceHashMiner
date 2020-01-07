using MinerPluginToolkitV1.Configs;
using NHM.Common.Algorithm;
using NHM.Common.Enums;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.Interfaces
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
