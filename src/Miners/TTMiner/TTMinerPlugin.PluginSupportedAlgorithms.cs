using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace TTMiner
{
    public partial class TTMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        //new SAS(AlgorithmType.MTP),
                        new SAS(AlgorithmType.Lyra2REv3),
                        new SAS(AlgorithmType.Eaglesong),
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                //{ AlgorithmType.MTP, "mtp" },
                { AlgorithmType.Lyra2REv3, "LYRA2V3" },
                { AlgorithmType.Eaglesong, "EAGLESONG" },
            }
        };
    }
}
