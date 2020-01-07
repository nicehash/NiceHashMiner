using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace LolMinerBeam
{
    public partial class LolMinerBeamPlugin
    {
        internal static List<SAS> SupportedGPUAlgos(bool enabled)
        {
            return new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckarood29) {Enabled = enabled },
                        new SAS(AlgorithmType.GrinCuckatoo31) {Enabled = enabled },
                        new SAS(AlgorithmType.BeamV2) {Enabled = enabled },
                    };
        }
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // fixed fee
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    SupportedGPUAlgos(false) // dsable NVIDIA by default
                },
                {
                    DeviceType.AMD,
                    SupportedGPUAlgos(true)
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.GrinCuckarood29, "GRIN-AD29" },
                { AlgorithmType.GrinCuckatoo31, "GRIN-AT31" },
                { AlgorithmType.BeamV2, "BEAM-II" },
            }
        };
    }
}
