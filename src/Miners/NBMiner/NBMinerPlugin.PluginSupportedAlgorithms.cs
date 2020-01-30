using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace NBMiner
{
    public partial class NBMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // TODO fees are not just 2%
            DefaultFee = 2.0,
            //AlgorithmFees = new Dictionary<AlgorithmType, double>
            //{
            //    { AlgorithmType.DaggerHashimoto, 0.65 },
            //    { AlgorithmType.DaggerHashimoto + AlgorithmType.Eaglesong, 3.0 },
            //},
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckatoo31),
                        new SAS(AlgorithmType.CuckooCycle),
                        new SAS(AlgorithmType.GrinCuckarood29),
                        new SAS(AlgorithmType.Eaglesong),
                        // new SAS(AlgorithmType.DaggerHashimoto), // needs different protocol settings
                        new SAS(AlgorithmType.Eaglesong, AlgorithmType.DaggerHashimoto) { NonDefaultRAMLimit = 4UL << 30  },
                    }
                },
                // TODO
                //{
                //    DeviceType.AMD,
                //    new List<SAS>
                //    {
                //    }
                //}
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.GrinCuckatoo31, "cuckatoo" },
                { AlgorithmType.CuckooCycle, "cuckoo_ae" },
                { AlgorithmType.GrinCuckarood29, "cuckarood" },
                { AlgorithmType.Eaglesong, "eaglesong" },
                { AlgorithmType.DaggerHashimoto, "ethash" },
            }
        };
    }
}
