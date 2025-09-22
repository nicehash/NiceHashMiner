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
                        new SAS(AlgorithmType.DaggerHashimoto) { Enabled = true},
                        new SAS(AlgorithmType.EtcHash) { Enabled = true},
                        new SAS(AlgorithmType.Autolykos) { Enabled = true},
                        new SAS(AlgorithmType.KAWPOW) { Enabled = true, NonDefaultRAMLimit = (4UL << 30) },
                        new SAS(AlgorithmType.NeoScrypt) { Enabled = true},
                        new SAS(AlgorithmType.ZelHash) { Enabled = true},
                        new SAS(AlgorithmType.Alephium) { Enabled = true },
                        new SAS(AlgorithmType.FishHash) { Enabled = true},
                        new SAS(AlgorithmType.XelisHashV2) { Enabled = true}
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.DaggerHashimoto) { Enabled = true },
                        new SAS(AlgorithmType.EtcHash) { Enabled = true },
                        new SAS(AlgorithmType.Autolykos) { Enabled = true },
                        new SAS(AlgorithmType.KAWPOW) { Enabled = true, NonDefaultRAMLimit =  (4UL << 30) },
                        new SAS(AlgorithmType.NeoScrypt) { Enabled = true },
                        new SAS(AlgorithmType.ZelHash) { Enabled = true },
                        new SAS(AlgorithmType.Alephium) { Enabled = true },
                        new SAS(AlgorithmType.FishHash) { Enabled = true },
                        new SAS(AlgorithmType.XelisHashV2) { Enabled = true }
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
                { AlgorithmType.ZelHash, "zelhash" },
                { AlgorithmType.Alephium, "alephium" },
                { AlgorithmType.FishHash, "fishhash" },
                { AlgorithmType.XelisHashV2, "xelishash" }
            }
        };
    }
}
