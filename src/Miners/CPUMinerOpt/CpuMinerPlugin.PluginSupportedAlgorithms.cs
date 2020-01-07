using MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace CpuMinerOpt
{
    public partial class CPUMinerPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.CPU,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.Lyra2Z) { Enabled = false },
                        new SAS(AlgorithmType.Lyra2REv3) { Enabled = false },
                        new SAS(AlgorithmType.X16R) { Enabled = false },
                        new SAS(AlgorithmType.X16Rv2)
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.Lyra2Z, "lyra2z" },
                { AlgorithmType.Lyra2REv3, "lyra2rev3" },
                { AlgorithmType.X16R, "x16r" },
                { AlgorithmType.X16Rv2, "x16rv2" },
            }
        };
    }
}
