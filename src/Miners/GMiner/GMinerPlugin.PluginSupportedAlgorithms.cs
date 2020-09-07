using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace GMinerPlugin
{
    public partial class GMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // fee fixed
            DefaultFee = 2.0,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.Cuckaroo29BFC, 3.0 },
                { AlgorithmType.CuckaRooz29, 3.0 },
            },
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
                        new SAS(AlgorithmType.GrinCuckatoo32),
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.Cuckaroo29BFC),
                        new SAS(AlgorithmType.BeamV3),
                        new SAS(AlgorithmType.CuckaRooz29),
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares
                        new SAS(AlgorithmType.Cuckaroo29BFC),
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.ZHash, "144_5" },
                { AlgorithmType.GrinCuckatoo31, "grin31" },
                { AlgorithmType.CuckooCycle, "cuckoo29" },
                { AlgorithmType.GrinCuckarood29, "cuckarood29" },
                { AlgorithmType.GrinCuckatoo32, "cuckatoo32" },
                { AlgorithmType.KAWPOW, "kawpow" },
                { AlgorithmType.Cuckaroo29BFC, "bfc" },
                { AlgorithmType.BeamV3, "beamhashIII" },
                { AlgorithmType.CuckaRooz29, "cuckarooz29" },
            }
        };
    }
}
