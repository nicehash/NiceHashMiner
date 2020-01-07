using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace CryptoDredge
{
    public partial class CryptoDredgePlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 1.0,
            //AlgorithmFees = new Dictionary<AlgorithmType, double>
            //{
            //    { AlgorithmType.MTP, 2.0 },
            //},
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.Lyra2REv3),
                        new SAS(AlgorithmType.X16R),
                        new SAS(AlgorithmType.X16Rv2),
                        //new SAS(AlgorithmType.MTP) { Enabled = false }
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.Lyra2REv3, "lyra2v3" },
                { AlgorithmType.X16R, "x16r" },
                { AlgorithmType.X16Rv2, "x16rv2" },
                //{ AlgorithmType.MTP, "mtp" },
            }
        };
    }
}
