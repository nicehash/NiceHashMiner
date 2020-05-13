using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.MinerPluginToolkitV1.ClaymoreCommon
{
    public static class DevicesListParser
    {
        public static Dictionary<string, int> ParseClaymoreDualOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();
            var mappedDevices = new Dictionary<string, int>();
            const string searchPattern = "pci bus ";
            var pciLines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(line => line.Contains(searchPattern)).ToList();
            int index = 0;
            foreach (var line in pciLines)
            {
                var pciArray = line.Substring(line.IndexOf(searchPattern) + searchPattern.Length).Split(':');
                var pciId = pciArray[0];
                var comparePCIeBusID = Convert.ToInt32(pciId);
                foreach (var gpu in gpus)
                {
                    if (gpu.PCIeBusID == comparePCIeBusID)
                    {
                        mappedDevices.Add(gpu.UUID, index);
                        break;
                    }
                }
                index++;
            }
            return mappedDevices;
        }
    }
}
