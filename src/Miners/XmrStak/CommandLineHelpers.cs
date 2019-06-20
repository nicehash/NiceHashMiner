using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XmrStak
{
    internal static class CommandLineHelpers
    {
        public static string DisableDevCmd(IEnumerable<DeviceType> usedDevs)
        {
            var devTypes = new List<DeviceType>
            {
                DeviceType.AMD,
                DeviceType.CPU,
                DeviceType.NVIDIA
            };
            return devTypes
                .FindAll(d => !usedDevs.Contains(d))
                .Aggregate("", (current, dev) => current + $"--no{dev} ");
        }

        private static string deviceIDs => "0123456789abcdefghijkmnopqrstuvwxyz";
        private static char GetDeviceID(int index)
        {
            if (index < deviceIDs.Length) return deviceIDs[index];
            return '-'; // invalid 
        }

        private static DeviceType[] _deviceTypesOrder = { DeviceType.CPU, DeviceType.NVIDIA, DeviceType.AMD };
        public static IEnumerable<Tuple<string, string>> GetConfigCmd(string configPrefix, IEnumerable<Tuple<DeviceType, int>> deviceTypes)
        {
            foreach (var type in _deviceTypesOrder)
            {
                var devs = deviceTypes.Where(pair => pair.Item1 == type);
                if (devs.Count() > 0)
                {
                    var typeStr = type.ToString();
                    var flag = $"--{typeStr.ToLower()}";
                    var orderedIDs = devs.Select(pair => pair.Item2).OrderBy(num => num).Select(num => GetDeviceID(num));
                    var confFile = $"{configPrefix}_{typeStr}_{string.Concat(orderedIDs)}.txt";
                    yield return Tuple.Create(flag, confFile);
                }
            }
        }

        public static IEnumerable<string> GetGeneralAndPoolsConf(string configPrefix)
        {
            yield return $"--config {configPrefix}_config.txt";
            yield return $"--poolconf {configPrefix}_poolconf.txt";
        }
    }
}
