using NHM.Common;
using NHM.MinerPlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1.ExtraLaunchParameters
{
    /// <summary>
    /// This class is used to Parse Extra Launch Parameters
    /// </summary>
    public static class ExtraLaunchParametersParser
    {
        /// <summary>
        /// ParsedMinerOption is used to set parsed value to MinerOption <see cref="MinerOption"/>
        /// </summary>
        private class ParsedMinerOption
        {
            public MinerOption Option { get; set; }
            public string Value { get; set; } = null;
        }

        /// <summary>
        /// ParsedDeviceMinerOptions is used to parse all MinerOptions and parameters for each device
        /// </summary>
        private class ParsedDeviceMinerOptions
        {
            public string DeviceUUID { get; set; }
            public List<string> Parameters { get; set; }
            public List<ParsedMinerOption> ParsedMinerOptions { get; set; }
        }

        /// <summary>
        /// MergedParsedMinerOption is used to merge parameters and values for devices. They are merged in device order.
        /// </summary>
        private class MergedParsedMinerOption
        {
            public MinerOption Option { get; set; }
            public List<string> Values { get; set; } = new List<string>();
            public bool IsDefaults { get; set; }
        }

        /// <summary>
        /// IsOptionDefaultValue checks if parsed value is same as default value of MinerOption
        /// </summary>
        private static bool IsOptionDefaultValue(MinerOption option, string value)
        {
            // Check if value is null or empty
            if (string.IsNullOrEmpty(value)) return true;
            if (string.IsNullOrWhiteSpace(value)) return true;

            // Value is not null or empty, compare it with default value
            if (!value.Equals(option.DefaultValue)) return false;

            return true;
        }

        public static bool CheckIfCanGroup(MiningPair a, MiningPair b, List<MinerOption> options, bool ignoreDefaultValueOptions)
        {
            if (options == null || options.Count == 0) return true;

            var filteredOptionsSingle = options.Where(optionType => optionType.Type == MinerOptionType.OptionWithSingleParameter).ToList();
            var filteredOptionsIsParam = options.Where(optionType => optionType.Type == MinerOptionType.OptionIsParameter).ToList();

            var parsedASingle = Parse(new List<MiningPair> { a }, filteredOptionsSingle, ignoreDefaultValueOptions);
            var parsedBSingle = Parse(new List<MiningPair> { b }, filteredOptionsSingle, ignoreDefaultValueOptions);

            if (parsedASingle != parsedBSingle) return false;

            var parsedAParam = Parse(new List<MiningPair> { a }, filteredOptionsIsParam, ignoreDefaultValueOptions);
            var parsedBParam = Parse(new List<MiningPair> { b }, filteredOptionsIsParam, ignoreDefaultValueOptions);

            return parsedAParam == parsedBParam;
        }

        /// <summary>
        /// Main Parse function which gets List of Mining Pairs <see cref="MiningPair"/>, List of Miner Options <see cref="MinerOption"/> and UseIfDefaults (bool) as arguments
        /// IgnoreDefaultValueOptions argument is set to false if you would like to parse default values for extra launch parameters
        /// It returns parsed string ready for adding to miner command
        /// </summary>
        public static string Parse(List<MiningPair> miningPairs, List<MinerOption> options, bool ignoreDefaultValueOptions = true)
        {
            if (options == null || options.Count == 0) return "";

            // init all devices and options before we check parameters
            // order of devices matters!
            // order of parameters may matter
            var devicesOptions = new List<ParsedDeviceMinerOptions>();
            foreach (var miningPair in miningPairs)
            {
                var device = miningPair.Device;
                var algorithm = miningPair.Algorithm;
                var parameters = algorithm.ExtraLaunchParameters
                    .Replace("=", "= ")
                    .Split(' ')
                    .Where(s => !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s))
                    .ToList();

                var parsedMinerOptions = options
                    .Select(opt => new ParsedMinerOption { Option = opt, Value = null })
                    .ToList();

                var deviceOptions = new ParsedDeviceMinerOptions
                {
                    DeviceUUID = device.UUID,
                    Parameters = parameters,
                    ParsedMinerOptions = parsedMinerOptions
                };
                devicesOptions.Add(deviceOptions);
            }

            // check parameters and values
            foreach (var deviceOptions in devicesOptions)
            {
                var parameters = deviceOptions.Parameters;

                //check for single/multi parameter cases
                for (int paramIndex = 0; paramIndex < parameters.Count; paramIndex++)
                {
                    var param = parameters[paramIndex];
                    string value = (paramIndex + 1) < parameters.Count ? parameters[paramIndex + 1] : "";

                    // find an option with the flag
                    var deviceParsedOption = deviceOptions.ParsedMinerOptions
                        .Where(pOpt => param.Equals(pOpt.Option.ShortName) || param.Equals(pOpt.Option.LongName))
                        .FirstOrDefault();

                    if (deviceParsedOption == null) continue;

                    switch (deviceParsedOption.Option.Type)
                    {
                        case MinerOptionType.OptionIsParameter:
                            deviceParsedOption.Value = param;
                            break;
                        case MinerOptionType.OptionWithSingleParameter:
                        case MinerOptionType.OptionWithMultipleParameters:
                        case MinerOptionType.OptionWithDuplicateMultipleParameters:
                            deviceParsedOption.Value = value;
                            break;
                    }
                }
            }

            // merge parameters and values for devices
            // they are merged in device order
            var mergedParsedMinerOptions = options
                .Select(option => new MergedParsedMinerOption { Option = option })
                .ToArray();

            foreach (var deviceOptions in devicesOptions)
            {
                for (int optIndex = 0; optIndex < mergedParsedMinerOptions.Length; optIndex++)
                {
                    var mergedOption = mergedParsedMinerOptions[optIndex];
                    var parsedOption = deviceOptions.ParsedMinerOptions[optIndex];
                    if (parsedOption.Option != mergedOption.Option)
                    {
                        throw new Exception("Options missmatch");
                    }
                    mergedOption.Values.Add(parsedOption.Value);
                }
            }

            // check if is all defaults
            foreach (var mergedParsedMinerOption in mergedParsedMinerOptions)
            {
                var option = mergedParsedMinerOption.Option;
                var values = mergedParsedMinerOption.Values;
                var isDefault = values.All(value => IsOptionDefaultValue(option, value));
                mergedParsedMinerOption.IsDefaults = isDefault;
            }
            var isAllDefault = mergedParsedMinerOptions.All(mpmopt => mpmopt.IsDefaults);

            // we don't parse if we have everything default and don't force defaults
            if (isAllDefault && ignoreDefaultValueOptions) return "";

            var retVal = "";
            foreach (var mergedParsedMinerOption in mergedParsedMinerOptions)
            {
                var option = mergedParsedMinerOption.Option;
                var values = mergedParsedMinerOption.Values;

                var optionName = string.IsNullOrEmpty(option.ShortName) ? option.LongName : option.ShortName;

                if (mergedParsedMinerOption.IsDefaults && ignoreDefaultValueOptions) continue;
                // if options all default ignore
                switch (option.Type)
                {
                    case MinerOptionType.OptionIsParameter:
                        retVal += $" {optionName}";
                        break;
                    case MinerOptionType.OptionWithSingleParameter:
                        {
                            // get the first non default value
                            var firstNonDefaultValue = values
                                .Where(value => !IsOptionDefaultValue(option, value))
                                .FirstOrDefault();

                            var setValue = option.DefaultValue;
                            if (firstNonDefaultValue != null)
                            {
                                setValue = firstNonDefaultValue;
                            }
                            var mask = " {0} {1}";
                            if (optionName.Contains("="))
                            {
                                mask = " {0}{1}";
                            }
                            retVal += string.Format(mask, optionName, setValue);
                            break;
                        }
                    case MinerOptionType.OptionWithMultipleParameters:
                        {
                            var setValues = values
                                .Select(value => value != null ? value : option.DefaultValue);
                            var mask = " {0} {1}";
                            if (optionName.Contains("="))
                            {
                                mask = " {0}{1}";
                            }
                            retVal += string.Format(mask, optionName, string.Join(option.Delimiter, setValues));
                            break;
                        }
                    case MinerOptionType.OptionWithDuplicateMultipleParameters:
                        {
                            var mask = "{0} {1}";
                            if (optionName.Contains("="))
                            {
                                mask = "{0}{1}";
                            }
                            var setValues = values
                                .Select(value => value != null ? value : option.DefaultValue)
                                .Select(value => string.Format(mask, optionName, value));
                            retVal += " " + string.Join(" ", setValues);
                            break;
                        }
                }
            }
            Logger.Debug("ELPParser", $"Successfully parsed {retVal} elp");
            return retVal;
        }
    }
}
