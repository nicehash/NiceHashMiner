using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.CommandLine
{
    //["--flagName"] // option is parameter

    //["--flagName", "value"] // option single parameter

    //["--zombie-tune", "value", ","] // option with multiple parameters - FOR DEVICES


    using Parameter = List<string>;
    using Parameters = List<List<string>>;
    using DevicesParametersList = List<List<List<string>>>;

    public static class MinerExtraParameters
    {
        public static ElpSettings ReadJson(string path)
        {
            var elp = JsonConvert.DeserializeObject<ElpSettings>(File.ReadAllText(path));
            return elp;
        }

        public enum ParameterType
        {
            OptionIsParameter = 1,
            OptionWithSingleParameter = 2,
            OptionWithMultipleParameters = 3
        }

        internal static ParameterType GetParameterType(Parameter parameter) => (ParameterType)parameter.Count;
        internal static bool IsParameterOfType(Parameter parameter, ParameterType type) => parameter.Count == (int)type;
        public static (string flag, string value, string delimiter) ParseParameter(Parameter parameter)
        {
            return GetParameterType(parameter) switch
            {
                ParameterType.OptionIsParameter => (parameter[0], null, null),
                ParameterType.OptionWithSingleParameter => (parameter[0], parameter[1], null),
                ParameterType.OptionWithMultipleParameters => (parameter[0], parameter[1], parameter[2]),
                _ => (null, null, null),
            };
        }

        internal static bool IsValidParameter(Parameter parameter, ParameterType type)
        {
            if (!IsParameterOfType(parameter, type)) return false;

            var (_, value, _) = ParseParameter(parameter);
            var valid = type switch
            {
                ParameterType.OptionWithSingleParameter or ParameterType.OptionWithMultipleParameters => !string.IsNullOrEmpty(value),
                _ => true,
            };

            return valid;
        }

        internal static Parameters FilterValidParametersOfType(Parameters parameters, ParameterType type)
        {
            return parameters.Where(p => IsValidParameter(p, type)).ToList();
        }

        internal static bool CheckIfCanGroup(Parameter aP, Parameter bP, ParameterType type)
        {
            if (!IsValidParameter(aP, type) || !IsValidParameter(bP, type)) return false;

            var (aFlag, aValue, aDelimiter) = ParseParameter(aP);
            var (bFlag, bValue, bDelimiter) = ParseParameter(bP);
            
            if (aFlag != bFlag) return false;

            return type switch
            {
                ParameterType.OptionIsParameter => true,
                ParameterType.OptionWithSingleParameter => aValue == bValue,
                ParameterType.OptionWithMultipleParameters => aDelimiter == bDelimiter,
                _ => false,
            };
        }

        private static bool CheckIfCanGroup(Parameter aP, Parameters b_Parameters, ParameterType type)
        {
            return b_Parameters.Count(bP => CheckIfCanGroup(aP, bP, type)) == 1;
        }

        private static bool CheckIfCanGroup(Parameters a, Parameters b, ParameterType type)
        {
            return a.All(aP => CheckIfCanGroup(aP, b, type)) && b.All(bP => CheckIfCanGroup(bP, a, type));
        }

        private static bool CheckIfCanGroup(Parameters a, Parameters b)
        {
            if (a == null || b == null) return false;

            foreach (ParameterType type in Enum.GetValues(typeof(ParameterType)))
            {
                var a_types = FilterValidParametersOfType(a, type);
                var b_types = FilterValidParametersOfType(b, type);
                if(!CheckIfCanGroup(a_types, b_types, type) && a_types.Count > 0 && b_types.Count > 0) return false;
            }

            return true;
        }
        public static bool CheckIfCanGroup(DevicesParametersList devicesParameters)
        {
            foreach (var a in devicesParameters)
            {
                foreach (var b in devicesParameters)
                {
                    if (a == b) continue;
                    if(a.Count == 0 || b.Count == 0) return false;
                    if (!CheckIfCanGroup(a, b)) return false;
                }
            }
            return true;
        }
        private static string DevicesStringForFlag(string flag, IEnumerable<Parameters> parameters)
        {
            var flagParams = parameters
                .Where(p => p.Count > 0)
                .Select(p => p.FirstOrDefault(o => o[0] == flag));
            var delimiter = flagParams.FirstOrDefault()[2];
            var values = string.Join(delimiter, flagParams.Select(o => o[1]).ToArray());
            var mask = flag.EndsWith("=") ? "{0}{1}" : "{0} {1}";
            return string.Format(mask, flag, values);
        }

        public static string Parse(Parameters minerParameters, Parameters algorithmParameters, DevicesParametersList devicesParameters)
        {
            //if (devicesParameters == null || devicesParameters.Count == 0 || minerParameters.Count == 0 || algorithmParameters.Count == 0) return "";
            if (devicesParameters == null) return "";
            if (!CheckIfCanGroup(devicesParameters)) return "";

            var options = FilterValidParametersOfType(devicesParameters.First(), ParameterType.OptionIsParameter).SelectMany(x => x).ToList();
            var singleOptions = FilterValidParametersOfType(devicesParameters.First(), ParameterType.OptionWithSingleParameter).SelectMany(x => x).ToList();
            var multipleOptions = devicesParameters
                .Select(p => FilterValidParametersOfType(p, ParameterType.OptionWithMultipleParameters));
            var ss = multipleOptions.SelectMany(p => p.Select(x => x[0]));
            var allFlags = new HashSet<string>(ss);
            var deviceFlagValues = allFlags.Select(flag => DevicesStringForFlag(flag, multipleOptions));
            var check = devicesParameters.SelectMany(x => x).ToList();

            options.AddRange(singleOptions);
            options.AddRange(deviceFlagValues);
            var algo = algorithmParameters.Where(p => check.All(o => o[0] != p[0]));
            var miner = minerParameters.Where(p => check.All(o => o[0] != p[0]) && algo.All(a=> a[0] != p[0]));
            var elp = "";
            if (miner.Any()) elp += string.Join(" ", miner.SelectMany(x => x));
            if (algo.Any()) elp += " " + string.Join(" ", algo.SelectMany(x => x));
            if (options.Any()) elp += " " + string.Join(" ", options);

            return elp.Trim();
        }

        public static string ParseAlgoPreview(Parameters minerParameters, Parameters algorithmParameters)
        {
            Parameters uniqueSingle = new();
            var uniqueSingles = minerParameters.Where(p => p.Count == 1).Concat(algorithmParameters.Where(p => p.Count == 1));
            foreach (var sng in uniqueSingles)
            {
                if (!uniqueSingle.Select(x => x[0]).Any(i => i == sng[0])) uniqueSingle.Add(new() { sng[0] });
            }
            Parameters uniqueDouble = new();
            var doublesAlgo = algorithmParameters.Where(p => p.Count == 2);
            var doublesMiner = minerParameters.Where(p => p.Count == 2);
            foreach(var dbl in doublesAlgo) uniqueDouble.Add(new() { dbl[0], dbl[1] });
            foreach(var dbl in doublesMiner)
            {
                if (!uniqueDouble.Select(x => x[0]).Any(i => i == dbl[0])) uniqueDouble.Add(new() { dbl[0], dbl[1] });
            }
            return $"{string.Join(' ', uniqueSingle.SelectMany(x => x))} {string.Join(' ', uniqueDouble.SelectMany(x => x))}";
        }
    }
}
