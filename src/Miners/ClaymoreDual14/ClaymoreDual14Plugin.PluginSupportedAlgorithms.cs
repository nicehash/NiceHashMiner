using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace ClaymoreDual14
{
    public partial class ClaymoreDual14Plugin
    {
        internal static List<SAS> SupportedGPUAlgos()
        {
            return new List<SAS>
                    {
                        new SAS( AlgorithmType.DaggerHashimoto),
                        // duals disabled by default
#pragma warning disable 0618
                        new SAS(AlgorithmType.DaggerHashimoto, AlgorithmType.Decred) {Enabled = false },
                        new SAS(AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s) {Enabled = false },
                        new SAS(AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak) {Enabled = false },
#pragma warning restore 0618
                    };
        }
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    SupportedGPUAlgos()
                },
                {
                    DeviceType.AMD,
                    SupportedGPUAlgos()
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.DaggerHashimoto, "eth" },
                #pragma warning disable 0618
                { AlgorithmType.Decred, "dcr" },
                { AlgorithmType.Blake2s, "b2s" },
                { AlgorithmType.Keccak, "kc" },
                #pragma warning restore 0618
            }
        };
    }
}
