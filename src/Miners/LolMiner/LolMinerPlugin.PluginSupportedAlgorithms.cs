using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace LolMiner
{
    public partial class LolMinerPlugin
    {
        const ulong AMD_8GBMemory = 7UL << 30; // 7GB but really 8GB
        const ulong AMD_6GBMemory = 5UL << 30; // 5GB but really 6GB
        const ulong AMD_3GBMemory = 3UL << 30; // 3GB but really 4GB
        internal static List<SAS> SupportedAMDAlgos()
        {
            return new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckarood29),
                        new SAS(AlgorithmType.GrinCuckatoo31) { NonDefaultRAMLimit = AMD_8GBMemory},
                        new SAS(AlgorithmType.BeamV2),
                        new SAS(AlgorithmType.Cuckaroom) { NonDefaultRAMLimit = AMD_6GBMemory },
                        new SAS(AlgorithmType.GrinCuckatoo32),
                        new SAS(AlgorithmType.ZHash),
                        new SAS(AlgorithmType.BeamV3) { NonDefaultRAMLimit = AMD_3GBMemory }
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
                // don't use NVIDIA OpenCL backend
                //{
                //    DeviceType.NVIDIA,
                //    SupportedNVIDIAOpenCLAlgos(false) // dsable NVIDIA by default
                //},
                {
                    DeviceType.AMD,
                    SupportedAMDAlgos()
                },
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.GrinCuckarood29, "C29D" },
                { AlgorithmType.GrinCuckatoo31, "C31" },
                { AlgorithmType.BeamV2, "BEAM-II" },
                { AlgorithmType.Cuckaroom, "C29M" },
                { AlgorithmType.GrinCuckatoo32, "C32" },
                { AlgorithmType.ZHash, "EQUI144_5" },
                { AlgorithmType.BeamV3, "BEAM-III" }
            }
        };
    }
}
