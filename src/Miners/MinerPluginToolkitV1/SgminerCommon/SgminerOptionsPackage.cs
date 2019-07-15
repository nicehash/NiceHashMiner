using MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace MinerPluginToolkitV1.SgminerCommon
{
    public static class SgminerOptionsPackage
    {
        // TODO remove redundant/duplicated long/short names after ELP parser is fixed
        public static readonly MinerOptionsPackage DefaultMinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                // Single Param
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "KeccakUnroll",
                    ShortName = "--keccak-unroll",
                    LongName = "--keccak-unroll",
                    DefaultValue = "0"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "HamsiExpandBig",
                    ShortName = "--hamsi-expand-big",
                    LongName = "--hamsi-expand-big",
                    DefaultValue = "4"
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "Nfactor",
                    ShortName = "--nfactor",
                    LongName = "--nfactor",
                    DefaultValue = "10"
                },
                // Multi Params
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "Intensity",
                    ShortName = "-I",
                    LongName = "--intensity",
                    DefaultValue = "d",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "Xintensity",
                    ShortName = "-X",
                    LongName = "--xintensity",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "Rawintensity",
                    ShortName = "--rawintensity",
                    LongName = "--rawintensity",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ThreadConcurrency",
                    ShortName = "--thread-concurrency",
                    LongName = "--thread-concurrency",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "Worksize",
                    ShortName = "-w",
                    LongName = "--worksize",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "GpuThreads",
                    ShortName = "-g",
                    LongName = "--gpu-threads",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "LookupGap",
                    ShortName = "--lookup-gap",
                    LongName= "--lookup-gap",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                // Only parameter
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "RemoveDisabled",
                    ShortName = "--remove-disabled",
                    DefaultValue = "--remove-disabled",
                },
            },
            TemperatureOptions = new List<MinerOption>
            {
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "GpuFan",
                    ShortName = "--gpu-fan",
                    LongName = "--gpu-fan",
                    DefaultValue = "30-60",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "TempCutoff",
                    ShortName = "--temp-cutoff",
                    LongName = "--temp-cutoff",
                    DefaultValue = "95",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "TempOverheat",
                    ShortName = "--temp-overheat",
                    LongName = "--temp-overheat",
                    DefaultValue = "85",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "TempTarget",
                    ShortName = "--temp-target",
                    LongName = "--temp-target",
                    DefaultValue = "75",
                    Delimiter = ","
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "AutoFan",
                    ShortName = "--auto-fan",
                    LongName = "--auto-fan",
                },
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "AutoGpu",
                    ShortName = "--auto-gpu",
                    LongName = "--auto-gpu",
                },
            }
        };
    }
}
