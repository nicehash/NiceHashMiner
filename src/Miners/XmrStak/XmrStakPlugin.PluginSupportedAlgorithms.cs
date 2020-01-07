using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace XmrStak
{
    public partial class XmrStakPlugin
    {
        internal static List<SAS> SupportedAlgos(bool enabled = true)
        {
            return new List<SAS>
            {
                new SAS(AlgorithmType.CryptoNightR) {Enabled = enabled },
            };
        }

        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 0.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.CPU,
                    SupportedAlgos()
                },
                {
                    DeviceType.NVIDIA,
                    SupportedAlgos()
                },
                {
                    DeviceType.AMD,
                    SupportedAlgos(false)
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.CryptoNightR, "cryptonight_r" },
            }
        };
    }
}
