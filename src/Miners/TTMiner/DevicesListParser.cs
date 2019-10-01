using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TTMiner
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseTTMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is CUDADevice).Cast<CUDADevice>();

            var mappedDevices = new Dictionary<string, int>();

            var gpuData = output.Substring(output.IndexOf("#"));
            var lines = gpuData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                foreach (var line in lines)
                {
                    var cudaID = line.Substring(line.IndexOf('#') + 1, 2);
                    int.TryParse(cudaID, out var ID);
                    var pciID = line.Substring(line.IndexOf(':') + 1, 2);
                    int.TryParse(pciID, out var comparePCIeBusID);

                    foreach (var gpu in gpus)
                    {
                        if (gpu.PCIeBusID == comparePCIeBusID)
                        {
                            mappedDevices.Add(gpu.UUID, ID);
                            break;
                        }
                    }
                }
            }
            return mappedDevices;
        }
    }
}
