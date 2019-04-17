using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParsePhoenixOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            //delete everything until first GPU
            if (!output.Contains("GPU1"))
            {
                return mappedDevices;
            }
            var filteredOutput = output.Substring(output.IndexOf("GPU1"));
            var lines = filteredOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                int index = 0;
                foreach (var line in lines)
                {
                    var tmpArray = line.Substring(line.IndexOf('('));
                    tmpArray = tmpArray.Remove(tmpArray.IndexOf(')'));
                    var fullPciId = tmpArray.Split(' ');
                    var pciId = fullPciId[1];
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
            }
            return mappedDevices;
        }
    }
}
