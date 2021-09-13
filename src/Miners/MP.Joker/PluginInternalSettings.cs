using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace MP.Joker
{
    internal static class PluginInternalSettings
    {
        internal static MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables{};

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            UseUserSettings = true,
            GeneralOptions = new List<MinerOption>
            {
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "OptionIsParameter",
                    ShortName = "--OptionIsParameter"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithDuplicateMultipleParameters,
                    ID = "OptionWithDuplicateMultipleParameters",
                    ShortName = "--OptionWithDuplicateMultipleParameters"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "OptionWithMultipleParameters",
                    ShortName = "--OptionWithMultipleParameters",
                    DefaultValue = "0",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "OptionWithSingleParameter",
                    ShortName = "--OptionWithSingleParameter",
                    DefaultValue = "1"
                },
            }
        };
    }
}
