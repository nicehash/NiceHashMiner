using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace FakePlugin
{
    public partial class FakePlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // fee fixed
            DefaultFee = 2.0,
            AlgorithmFeesV2 = new Dictionary<string, double>
            {
                { $"{AlgorithmType.KAWPOW}", 3.0 },
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.ZHash),
                        new SAS(AlgorithmType.CuckooCycle) {Enabled = false },
                        new SAS(AlgorithmType.BeamV3),
                        new SAS(AlgorithmType.KAWPOW),
                        new SAS(AlgorithmType.DaggerHashimoto, AlgorithmType.ZHash),
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.CuckooCycle) {Enabled = false },
                        new SAS(AlgorithmType.BeamV3),
                        new SAS(AlgorithmType.KAWPOW),
                    }
                },
                {
                    DeviceType.CPU,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.RandomXmonero),
                        new SAS(AlgorithmType.KAWPOW),
                    }
                },
            }
        };
    }
}
