using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GMinerPlugin
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseGMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();

            var mappedDevices = new Dictionary<string, int>();
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                foreach (var line in lines)
                {
                    var tmpArray = line.Split(new[] { "PCI:" }, StringSplitOptions.RemoveEmptyEntries);
                    var fullPciId = tmpArray[1].Split(':');
                    var pciId = fullPciId[1];
                    var id = line.Substring(0, line.IndexOf(':')).Remove(0, 3);
                    var indexID = Convert.ToInt32(id);
                    var lastChar = pciId.ToCharArray();
                    int comparePCIeBusID = int.Parse(pciId, System.Globalization.NumberStyles.HexNumber);
                    foreach (var gpu in gpus)
                    {
                        if (gpu.PCIeBusID == comparePCIeBusID)
                        {
                            mappedDevices.Add(gpu.UUID, indexID);
                            break;
                        }
                    }
                }
            }
            return mappedDevices;
        }
    }
}
