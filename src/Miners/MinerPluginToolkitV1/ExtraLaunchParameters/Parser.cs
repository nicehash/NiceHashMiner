using System;
using System.Collections.Generic;
using System.Linq;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Algorithm;

namespace MinerPluginToolkitV1.ExtraLaunchParameters
{
    public static class Parser
    {

        private class ParsedMinerOption
        {
            public MinerOption Option { get; set; }
            public string Value { get; set; } = null;
        }

        private class ParsedDeviceMinerOptions
        {
            public string DeviceUUID { get; set; }
            public List<string> Parameters { get; set; }
            public List<ParsedMinerOption> ParsedMinerOptions { get; set; }
        }

        private class MergedParsedMinerOption
        {
            public MinerOption Option { get; set; }
            public List<string> Values { get; set; } = new List<string>();
            public bool IsDefaults { get; set; }
        }

        private static bool IsOptionDefaultValue(MinerOption option, string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            if (string.IsNullOrWhiteSpace(value)) return true;
            if (value.Equals(option.DefaultValue)) return true;
            return false;
        }

        public static string Parse(List<(BaseDevice device, Algorithm algorithm)> miningPairs, List<MinerOption> options, bool useIfDefaults = false)
        {
            if (options == null || options.Count == 0) return "";
            
            // init all devices and options before we check parameters
            // order of devices matters!
            // order of parameters may matter
            var devicesOptions = new List<ParsedDeviceMinerOptions>();
            foreach (var (device, algorithm) in miningPairs)
            {
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
                for (int paramIndex = 0; paramIndex < parameters.Count - 1; paramIndex++)
                {
                    var param = parameters[paramIndex];
                    var value = parameters[paramIndex + 1];

                    // find an option with the flag
                    var deviceParsedOption = deviceOptions.ParsedMinerOptions
                        .Where(pOpt => param.Equals(pOpt.Option.ShortName) || param.Equals(pOpt.Option.LongName))
                        .FirstOrDefault();

                    if (deviceParsedOption == null) continue;

                    switch (deviceParsedOption.Option.Type)
                    {
                        case MinerOptionType.OptionWithSingleParameter:
                        case MinerOptionType.OptionWithMultipleParameters:
                        case MinerOptionType.OptionWithDuplicateMultipleParameters:
                            deviceParsedOption.Value = value;
                            break;

                        case MinerOptionType.OptionIsParameter:
                            deviceParsedOption.Value = param;
                            break;
                    }
                }
            }

            // merge parameters and values for devices
            // they are merged in device order
            var mergedParsedMinerOptions = options
                .Select(option => new MergedParsedMinerOption{ Option = option })
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
            if (isAllDefault && useIfDefaults == false) return "";

            var retVal = "";
            foreach (var mergedParsedMinerOption in mergedParsedMinerOptions)
            {
                var option = mergedParsedMinerOption.Option;
                var values = mergedParsedMinerOption.Values;

                if (mergedParsedMinerOption.IsDefaults && !useIfDefaults) continue;
                // if options all default ignore
                switch (option.Type)
                {
                    case MinerOptionType.OptionIsParameter:
                        retVal += $" {option.ShortName}";
                        break;
                    case MinerOptionType.OptionWithSingleParameter:
                        {
                            // get the first non default value
                            var firstNonDefaultValue = values
                                .Where(value => IsOptionDefaultValue(option, value))
                                .FirstOrDefault();

                            var setValue = option.DefaultValue;
                            if (firstNonDefaultValue != null)
                            {
                                setValue = firstNonDefaultValue;
                            }
                            var mask = " {0} {1}";
                            if (option.ShortName.Contains("="))
                            {
                                mask = " {0}{1}";
                            }
                            retVal += string.Format(mask, option.ShortName, setValue);
                            break;
                        }
                    case MinerOptionType.OptionWithMultipleParameters:
                        {
                            var setValues = values
                                .Select(value => value != null ? value : option.DefaultValue);
                            var mask = " {0} {1}";
                            if (option.ShortName.Contains("="))
                            {
                                mask = " {0}{1}";
                            }
                            retVal += string.Format(mask, option.ShortName, string.Join(option.Delimiter, setValues));
                            break;
                        }
                    case MinerOptionType.OptionWithDuplicateMultipleParameters:
                        {
                            var mask = "{0} {1}";
                            if (option.ShortName.Contains("="))
                            {
                                mask = "{0}{1}";
                            }
                            var setValues = values
                                .Select(value => value != null ? value : option.DefaultValue)
                                .Select(value => string.Format(mask, option.ShortName, value));
                            retVal += " " + string.Join(" ", setValues);
                            break;
                        }
                }
            }
            return retVal;
        }
    }
}
