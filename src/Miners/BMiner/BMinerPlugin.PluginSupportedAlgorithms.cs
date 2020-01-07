using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace BMiner
{
    public partial class BMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 2.0,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.DaggerHashimoto, 0.65 },
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto) {Enabled = false },
                        new SAS(AlgorithmType.ZHash) {Enabled = false },
                        new SAS(AlgorithmType.GrinCuckaroo29),
                        new SAS(AlgorithmType.GrinCuckatoo31),
                        new SAS(AlgorithmType.GrinCuckarood29),
                    }
                },
                //{
                //    DeviceType.AMD,
                //    new List<SAS>
                //    {
                //        //new SAS(AlgorithmType.Beam) {Enabled = false },
                //    }
                //}
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.DaggerHashimoto, "ethstratum" },
                { AlgorithmType.ZHash, "zhash" },
                { AlgorithmType.GrinCuckaroo29, "cuckaroo29" },
                { AlgorithmType.GrinCuckatoo31, "cuckatoo31" },
                { AlgorithmType.GrinCuckarood29, "cuckaroo29d" },
            }
        };
    }
}
