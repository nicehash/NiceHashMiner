using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace SRBMiner
{
    public partial class SRBMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 0.85,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.Handshake, 2.0 },
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        //new SAS(AlgorithmType.Eaglesong),
                        new SAS(AlgorithmType.Handshake)
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                //{ AlgorithmType.Eaglesong, "eaglesong" },
                { AlgorithmType.Handshake, "bl2bsha3" }
            }
        };
    }
}
