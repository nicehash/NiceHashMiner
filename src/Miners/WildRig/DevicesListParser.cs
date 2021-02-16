using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WildRig
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseWildRigOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();
            var mappedDevices = new Dictionary<string, int>();

            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var pciArray = line.Split('#');
                var wildIndex = Convert.ToInt32(pciArray[1].Substring(0, 2).Replace(':', ' '));
                var pciID = line.Substring(line.IndexOf("busID: ") + 7).Split(',').FirstOrDefault();
                if (string.IsNullOrEmpty(pciID)) continue;
                var comparePCIeBusID = Convert.ToInt32(pciID);
                foreach (var gpu in gpus)
                {
                    if (gpu.PCIeBusID == comparePCIeBusID)
                    {
                        mappedDevices.Add(gpu.UUID, wildIndex);
                        break;
                    }
                }
            }
            return mappedDevices;
        }
    }
}
