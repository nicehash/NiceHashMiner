using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace Excavator
{
    public partial class ExcavatorPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 0.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.EtcHash),
                        new SAS(AlgorithmType.Autolykos) { Enabled = false },
                        new SAS(AlgorithmType.KAWPOW) { Enabled = false, NonDefaultRAMLimit = (4UL << 30) },
                        new SAS(AlgorithmType.NeoScrypt),
                        new SAS(AlgorithmType.KHeavyHash),
                        new SAS(AlgorithmType.IronFish)
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.EtcHash),
                        new SAS(AlgorithmType.Autolykos) { Enabled = false },
                        new SAS(AlgorithmType.KAWPOW) { Enabled = false, NonDefaultRAMLimit =  (4UL << 30) },
                        new SAS(AlgorithmType.NeoScrypt),
                        new SAS(AlgorithmType.KHeavyHash),
                        new SAS(AlgorithmType.IronFish)
                    }
                },
                {
                    DeviceType.CPU,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.RandomXmonero)
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.DaggerHashimoto, "daggerhashimoto" },
                { AlgorithmType.EtcHash, "etchash" },
                { AlgorithmType.Autolykos, "autolykos" },
                { AlgorithmType.KAWPOW, "kawpow" },
                { AlgorithmType.NeoScrypt, "neoscrypt" },
                { AlgorithmType.RandomXmonero, "randomx" },
                { AlgorithmType.KHeavyHash, "kheavyhash" },
                { AlgorithmType.IronFish, "ironfish" }
            }
        };
    }
}
