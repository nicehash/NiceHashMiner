using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace LolMiner
{
    public partial class LolMinerPlugin
    {
        const ulong AMD_8GBMemory = 7UL << 30; // 7GB but really 8GB
        internal static List<SAS> SupportedAMDAlgos()
        {
            return new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckarood29),
                        new SAS(AlgorithmType.GrinCuckatoo31),
                        new SAS(AlgorithmType.BeamV2),
                        new SAS(AlgorithmType.Cuckaroom) { NonDefaultRAMLimit = AMD_8GBMemory },
                        new SAS(AlgorithmType.GrinCuckatoo32),
                    };
        }

        // NVIDIA OpenCL backend is not really that stable
        internal static List<SAS> SupportedNVIDIAOpenCLAlgos(bool enabled = false)
        {
            return new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckarood29) {Enabled = enabled },
                        new SAS(AlgorithmType.GrinCuckatoo31) {Enabled = enabled },
                        new SAS(AlgorithmType.BeamV2) {Enabled = enabled },
                    };
        }

        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // fixed fee
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    SupportedNVIDIAOpenCLAlgos(false) // dsable NVIDIA by default
                },
                {
                    DeviceType.AMD,
                    SupportedAMDAlgos()
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.GrinCuckarood29, "GRIN-AD29" },
                { AlgorithmType.GrinCuckatoo31, "GRIN-AT31" },
                { AlgorithmType.BeamV2, "BEAM-II" },
                { AlgorithmType.Cuckaroom, "FLOO-C29M" },
                { AlgorithmType.GrinCuckatoo32, "GRIN-C32" },
            }
        };
    }
}
