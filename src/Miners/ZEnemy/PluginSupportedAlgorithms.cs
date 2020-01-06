using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace ZEnemy
{
    internal static class PluginSupportedAlgorithms
    {
        internal static PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings = new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 1.0,
            NVIDIA_Algorithms = new List<SAS>
            {
                new SAS(AlgorithmType.X16R),
                new SAS(AlgorithmType.X16Rv2),
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.X16R, "x16r" },
                { AlgorithmType.X16Rv2, "x16rv2" },
            }
        };
    }
}
