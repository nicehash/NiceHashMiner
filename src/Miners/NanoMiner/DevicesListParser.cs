using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NanoMiner
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseNanoMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            if (gpus.Count() == 0)
            {
                return mappedDevices;
            }

            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                foreach (var line in lines)
                {
                    if (!line.Contains("GPU")) continue;
                    var tmpArray = line.Split(new[] { "PCI" }, StringSplitOptions.RemoveEmptyEntries);
                    var fullPciId = tmpArray[1].Split(':');
                    var pciId = fullPciId[0];
                    var comparePCIeBusID = int.Parse(pciId, System.Globalization.NumberStyles.HexNumber);
                    var id = tmpArray[0].Remove(0, 3);
                    var indexID = Convert.ToInt32(id);
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
