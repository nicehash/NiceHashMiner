using NiceHashMinerLegacy.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace XmrStak
{
    internal static class CommandLineHelpers
    {
        public static string DisableDevCmd(ICollection<DeviceType> usedDevs)
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
    }
}
