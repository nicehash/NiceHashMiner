using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace GMinerPlugin
{
    public partial class GMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // fee fixed
            DefaultFee = 2.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.ZHash),
                        new SAS(AlgorithmType.GrinCuckatoo31),
                        new SAS(AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares,
                        new SAS(AlgorithmType.GrinCuckarood29),
                        new SAS(AlgorithmType.BeamV2),
                        //new SAS(AlgorithmType.Eaglesong),
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares
                        new SAS(AlgorithmType.BeamV2),
                        new SAS(AlgorithmType.Eaglesong),
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.ZHash, "144_5" },
                { AlgorithmType.GrinCuckatoo31, "grin31" },
                { AlgorithmType.CuckooCycle, "cuckoo29" },
                { AlgorithmType.GrinCuckarood29, "cuckarood29" },
                { AlgorithmType.BeamV2, "beamhashII" },
                { AlgorithmType.Eaglesong, "eaglesong" },
            }
        };
    }
}
