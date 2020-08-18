using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace ClaymoreDual14
{
    public partial class ClaymoreDual14Plugin
    {
        internal static List<SAS> SupportedGPUAlgos()
        {
            return new List<SAS>
                    {
                        new SAS( AlgorithmType.DaggerHashimoto) {NonDefaultRAMLimit = 5UL << 30}
                    };
        }
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    SupportedGPUAlgos()
                },
                {
                    DeviceType.AMD,
                    SupportedGPUAlgos()
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.DaggerHashimoto, "eth" }
            }
        };
    }
}
