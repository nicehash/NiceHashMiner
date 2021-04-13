using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace NBMiner
{
    public partial class NBMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // TODO fees are not just 2%
            DefaultFee = 2.0,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.DaggerHashimoto, 1.0 },
                //{ AlgorithmType.Cuckaroo29BFC, 3.0 },
                { AlgorithmType.Octopus, 3.0 },
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckatoo31),
                        new SAS(AlgorithmType.CuckooCycle),
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.GrinCuckatoo32),
                        new SAS(AlgorithmType.BeamV3),
                        new SAS(AlgorithmType.Octopus) {NonDefaultRAMLimit = 5UL << 30},
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.DaggerHashimoto),
                        //new SAS(AlgorithmType.Octopus) {NonDefaultRAMLimit = 5UL << 30},
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.GrinCuckatoo31, "cuckatoo" },
                { AlgorithmType.CuckooCycle, "cuckoo_ae" },
                //{ AlgorithmType.GrinCuckarood29, "cuckarood" },
                { AlgorithmType.DaggerHashimoto, "ethash" },
                { AlgorithmType.KAWPOW, "kawpow" },
                //{ AlgorithmType.Cuckaroo29BFC, "bfc" },
                { AlgorithmType.GrinCuckatoo32, "cuckatoo32" },
                { AlgorithmType.BeamV3, "beamv3" },
                { AlgorithmType.Octopus, "octopus" }
            }
        };
    }
}
