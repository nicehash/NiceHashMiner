using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace TeamRedMiner
{
    public partial class TeamRedMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 2.5,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.Lyra2Z, 3.0 },

            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.CryptoNightR),
                        new SAS(AlgorithmType.Lyra2REv3),
                        new SAS(AlgorithmType.Lyra2Z),
                        new SAS(AlgorithmType.X16R),
                        new SAS(AlgorithmType.GrinCuckatoo31),
                        //new SAS(AlgorithmType.MTP) { Enabled = false },
                        new SAS(AlgorithmType.GrinCuckarood29),
                        new SAS(AlgorithmType.X16Rv2)
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.CryptoNightR, "cnr" },
                { AlgorithmType.Lyra2REv3, "lyra2rev3" },
                { AlgorithmType.Lyra2Z, "lyra2z" },
                { AlgorithmType.X16R, "x16r" },
                { AlgorithmType.GrinCuckatoo31, "cuckatoo31_grin" },
                //{ AlgorithmType.MTP, "mtp" },
                { AlgorithmType.GrinCuckarood29, "cuckarood29_grin" },
                { AlgorithmType.X16Rv2, "x16rv2" },
            }
        };
    }
}
