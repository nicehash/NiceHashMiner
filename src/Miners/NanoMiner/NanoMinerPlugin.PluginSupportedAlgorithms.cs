using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace NanoMiner
{
    public partial class NanoMinerPlugin
    {
        internal static List<SAS> SupportedGPUAlgos()
        {
            return new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckarood29),
                        new SAS(AlgorithmType.RandomXmonero) { Enabled = false },
                    };
        }

        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 2.0,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.CryptoNightR, 1.0 }, // dev fee migh be wrong
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    SupportedGPUAlgos()
                },
                {
                    DeviceType.AMD,
                    SupportedGPUAlgos()
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.GrinCuckarood29, "Cuckarood29" },
                { AlgorithmType.RandomXmonero, "RandomX" },
            }
        };
    }
}
