using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildRig
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseWildRigOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();
            var mappedDevices = new Dictionary<string, int>();

            const string searchPattern = "BusID";
            var pciLines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(line => line.Contains(searchPattern)).ToList();
            int index = 0;
            foreach (var line in pciLines)
            {
                var pciArray = line.Substring(line.IndexOf(searchPattern) + 10).Split('#');
                var pciId = pciArray[1];
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
