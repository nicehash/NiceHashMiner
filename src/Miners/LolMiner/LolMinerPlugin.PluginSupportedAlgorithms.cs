using NHM.Common.Enums;
using NHM.MinerPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace LolMiner
{
    public partial class LolMinerPlugin
    {
        const ulong AMD_8GBMemory = 7UL << 30; // 7GB but really 8GB
        const ulong AMD_6GBMemory = 5UL << 30; // 5GB but really 6GB
        const ulong AMD_3GBMemory = 3UL << 30; // 3GB but really 4GB
        // NVIDIA OpenCL backend is not really that stable
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // fixed fee
            DefaultFee = 1.0,
            AlgorithmFeesV2 = new Dictionary<string, double>
            {
                { $"{AlgorithmType.DaggerHashimoto}", 0.7 },
                { $"{AlgorithmType.EtcHash}", 0.7 }
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                // don't use NVIDIA OpenCL backend
                //{
                //    DeviceType.NVIDIA,
                //    SupportedNVIDIAOpenCLAlgos(false) // dsable NVIDIA by default
                //},
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckatoo32),
                        new SAS(AlgorithmType.CuckooCycle),
                        new SAS(AlgorithmType.ZHash),
                        new SAS(AlgorithmType.BeamV3),
                        new SAS(AlgorithmType.DaggerHashimoto) { Enabled = false },
                        new SAS(AlgorithmType.EtcHash){NonDefaultRAMLimit =  (4UL << 29) + (5UL << 28) + (1UL << 26), Enabled = false },
                        new SAS(AlgorithmType.ZelHash),
                        new SAS(AlgorithmType.KHeavyHash),
                        new SAS(AlgorithmType.Autolykos) { Enabled = false },
                        new SAS(AlgorithmType.NexaPow),
                        new SAS(AlgorithmType.IronFish) { Enabled = false },
                        new SAS(AlgorithmType.KarlsenHash) { Enabled = false },
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.GrinCuckatoo32),
                        new SAS(AlgorithmType.CuckooCycle),
                        new SAS(AlgorithmType.ZHash),
                        new SAS(AlgorithmType.BeamV3) { NonDefaultRAMLimit = AMD_3GBMemory },
                        new SAS(AlgorithmType.DaggerHashimoto) { Enabled = false },
                        new SAS(AlgorithmType.EtcHash) {NonDefaultRAMLimit =  (4UL << 29) + (5UL << 28) + (1UL << 26), Enabled = false },
                        new SAS(AlgorithmType.ZelHash),
                        new SAS(AlgorithmType.KHeavyHash),
                        new SAS(AlgorithmType.Autolykos) { Enabled = false },
                        new SAS(AlgorithmType.NexaPow),
                        new SAS(AlgorithmType.IronFish) { Enabled = false },
                        new SAS(AlgorithmType.KarlsenHash) { Enabled = false },
                    }
                },
                {
                    DeviceType.INTEL,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KHeavyHash),
                        new SAS(AlgorithmType.BeamV3),
                        new SAS(AlgorithmType.Autolykos),
                        new SAS(AlgorithmType.ZelHash),
                        new SAS(AlgorithmType.ZHash)
                    }
                }
            }
        };
    }
}
