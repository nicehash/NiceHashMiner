using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParsePhoenixOutput(string output, IEnumerable<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();
            var mappedDevices = new Dictionary<string, int>();
            var lines = output.Split(new[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            
            int index = -1;
            foreach (var line in lines)
            {
                if (!line.Contains("pcie")) continue;
                index++;

                var tmpArray = line.Substring(line.IndexOf('('));
                tmpArray = tmpArray.Remove(tmpArray.IndexOf(')'));
                var fullPciId = tmpArray.Split(' ');
                var pciId = fullPciId[1];
                var comparePCIeBusID = Convert.ToInt32(pciId);
                var gpuWithPCIeBusID = gpus.Where(gpu => gpu.PCIeBusID == comparePCIeBusID).FirstOrDefault();
                if (gpuWithPCIeBusID != null)
                {
                    mappedDevices.Add(gpuWithPCIeBusID.UUID, index);
                }
            }
            
            return mappedDevices;
        }
    }
}
