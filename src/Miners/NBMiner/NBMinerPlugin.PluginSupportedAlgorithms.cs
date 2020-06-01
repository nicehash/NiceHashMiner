using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
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
                //{ AlgorithmType.DaggerHashimoto + AlgorithmType.Eaglesong, 3.0 },
                { AlgorithmType.Cuckaroo29BFC, 3.0 },
            },
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
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.Eaglesong, AlgorithmType.DaggerHashimoto) { NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.Handshake),
                        new SAS(AlgorithmType.Handshake, AlgorithmType.DaggerHashimoto) { NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.KAWPOW),
                        //new SAS(AlgorithmType.Cuckaroo29BFC),
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.Eaglesong, AlgorithmType.DaggerHashimoto) { NonDefaultRAMLimit = 4UL << 30  },
                        new SAS(AlgorithmType.Handshake),
                        new SAS(AlgorithmType.Handshake, AlgorithmType.DaggerHashimoto) { NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.KAWPOW),
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.GrinCuckatoo31, "cuckatoo" },
                { AlgorithmType.CuckooCycle, "cuckoo_ae" },
                { AlgorithmType.GrinCuckarood29, "cuckarood" },
                { AlgorithmType.Eaglesong, "eaglesong" },
                { AlgorithmType.DaggerHashimoto, "ethash" },
                { AlgorithmType.Handshake, "hns" },
                { AlgorithmType.KAWPOW, "kawpow" },
                { AlgorithmType.Cuckaroo29BFC, "bfc" },
            }
        };
    }
}
