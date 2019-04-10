using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
