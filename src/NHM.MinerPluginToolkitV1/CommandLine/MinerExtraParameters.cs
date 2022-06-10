using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1.CommandLine
{
    //["--flagName"] // option is parameter

    //["--flagName", "value"] // option single parameter

    //["--zombie-tune", "value", ","] // option with multiple parameters - FOR DEVICES


    using Parameter = List<string>;
    //using MinerParameters = List<List<string>>;
    //using AlgorithmParameters = List<List<string>>;
    using DeviceParameters = List<List<string>>;
    using DevicesParametersList = List<List<List<string>>>;
    public static class MinerExtraParameters
    {
        // this is a re-write of the NHM.MinerPluginToolkitV1.ExtraLaunchParameters.ExtraLaunchParametersParser
        // that ofers a dynamic ELP options
        // basically we have
        // #1 options per miners that replace 'OptionIsParameter' and 'OptionWithSingleParameter'
        // #2 options per devices that replace 'OptionWithMultipleParameters'
        // This parser does not have support for default missing values and that is perfectally fine
        // we should add a counterpart that has a mandatory miner command line​

        //public interface DeviceParameters : IReadOnlyList<string[]> { }
        //public interface DevicesParametersList : IReadOnlyList<IReadOnlyList<IReadOnlyList<string>>> { }
        internal static bool IsOptionIsParameter(Parameter parameter) => parameter.Count == 1;
        internal static bool IsOptionWithSingleParameter(Parameter parameter) => parameter.Count == 2;
        internal static bool IsOptionWithMultipleParameters(Parameter parameter) => parameter.Count == 3;
        public static DeviceParameters ToDeviceOptionsAreParameters(DeviceParameters parameters)
        {
            return parameters.Where(IsOptionIsParameter).ToList();
        }
        public static DeviceParameters ToDeviceOptionsWithSingleParameter(DeviceParameters parameters)
        {
            return parameters.Where(IsOptionWithSingleParameter).ToList();
        }
        public static DeviceParameters ToDeviceOptionsWithMultipleParameters(DeviceParameters parameters)
        {
            return parameters.Where(IsOptionWithMultipleParameters).ToList();
        }

        private static string DevicesStringForFlag(string flag, IEnumerable<DeviceParameters> parameters)
        {
            var flagParams = parameters
                .Select(p => p.FirstOrDefault(o => o[0] == flag));
            var delimiter = flagParams.FirstOrDefault()[2];
            var values = string.Join(delimiter, flagParams.Select(o => o[1]).ToArray());
            var mask = flag.EndsWith("=") ? "{0}{1}" : "{0} {1}";
            return string.Format(mask, flag, values);
        }

        private static bool CheckIfCanGroup(List<string> aP, DeviceParameters b_Parameters)
        {
            var bParams = b_Parameters.Where(bP => bP[0] == aP[0]);

            var canGroup = bParams.Count() == 1;

            return canGroup;
        }

        private static bool CheckIfCanGroupSingle(List<string> aP, DeviceParameters b_Parameters)
        {
            var bParams = b_Parameters.Where(bP => bP[0] == aP[0]);

            var canGroup = bParams.Count() == 1 && 
                bParams.All(bP => bP[1] == aP[1]);

            return canGroup;
        }

        private static bool CheckIfCanGroupMultiple(List<string> aP, DeviceParameters b_Parameters)
        {
            var bParams = b_Parameters.Where(bP => bP[0] == aP[0] );

            var canGroup = bParams.Count() == 1 && 
                bParams.All(bP => bP[2] == aP[2]);

            return canGroup;
        }

        public static bool CheckIfCanGroup(DeviceParameters a, DeviceParameters b)
        {
            if (a == null || b == null) return false;

            var a_Parameters = ToDeviceOptionsAreParameters(a);
            var b_Parameters = ToDeviceOptionsAreParameters(b);
            var a_SingleParameters = ToDeviceOptionsWithSingleParameter(a);
            var b_SingleParameters = ToDeviceOptionsWithSingleParameter(b);
            var a_MultipleParameters = ToDeviceOptionsWithMultipleParameters(a);
            var b_MultipleParameters = ToDeviceOptionsWithMultipleParameters(b);

            var aCanGroupWithB = a_Parameters.All(aP => CheckIfCanGroup(aP, b_Parameters));
            var bCanGroupWithA = b_Parameters.All(bP => CheckIfCanGroup(bP, a_Parameters));
            var aCanGroupWithBSingle = a_SingleParameters.All(aP => CheckIfCanGroupSingle(aP, b_SingleParameters));
            var bCanGroupWithASingle = b_SingleParameters.All(bP => CheckIfCanGroupSingle(bP, a_SingleParameters));
            var aCanGroupWithBMultiple = a_MultipleParameters.All(aP => CheckIfCanGroupMultiple(aP, b_MultipleParameters));
            var bCanGroupWithAMultiple = b_MultipleParameters.All(bP => CheckIfCanGroupMultiple(bP, a_MultipleParameters));

            return aCanGroupWithB && bCanGroupWithA && a_Parameters.Count == b_Parameters.Count &&
                aCanGroupWithBSingle && bCanGroupWithASingle && a_SingleParameters.Count == b_SingleParameters.Count && 
                aCanGroupWithBMultiple && bCanGroupWithAMultiple && a_MultipleParameters.Count == b_MultipleParameters.Count;
        }
        private static bool CheckIfCanGroup(DevicesParametersList devicesParameters)
        {
            foreach (var a in devicesParameters)
            {
                foreach (var b in devicesParameters)
                {
                    if (a == b) continue;
                    if (!CheckIfCanGroup(a, b)) return false;
                }
            }
            return true;
        }

        public static string Parse(DeviceParameters minerParameters, DeviceParameters algorithmParameters, DevicesParametersList devicesParameters)
        {
            if (devicesParameters == null || devicesParameters.Count == 0) return "";
            if (!CheckIfCanGroup(devicesParameters)) return "";

            var options = devicesParameters.Select(ToDeviceOptionsAreParameters).SelectMany(x => x).GroupBy(p => p[0]).Select(grp => grp.First()).SelectMany(x => x);
            var singleOptions = devicesParameters.Select(ToDeviceOptionsWithSingleParameter).SelectMany(x => x).GroupBy(p => p[0]).Select(grp => grp.First()).SelectMany(x => x);
            var multipleOptions = devicesParameters
                .Select(ToDeviceOptionsWithMultipleParameters);

            var miner = minerParameters.SelectMany(x => x).ToList();
            var algo = algorithmParameters.SelectMany(x => x).ToList();
            var ss = multipleOptions.SelectMany(d => d.Select(o => o[0]));
            var allFlags = new HashSet<string>(ss);
            var deviceFlagValues = allFlags.Select(flag => DevicesStringForFlag(flag, multipleOptions));

            var elp = string.Join(" ", miner) + " " + string.Join(" ", algo) + " " + string.Join(" ", options) + " " + string.Join(" ", singleOptions) + " " + string.Join(" ", deviceFlagValues);
            return elp;
        }
    }
}
