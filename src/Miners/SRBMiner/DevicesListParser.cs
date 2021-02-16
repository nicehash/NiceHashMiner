using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SRBMiner
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseSRBMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is AMDDevice).Cast<AMDDevice>();

            var mappedDevices = new Dictionary<string, int>();

            var gpuSections = output.Split(new[] { "GPU" }, StringSplitOptions.RemoveEmptyEntries);
            if (gpuSections.Count() != 0)
            {
                foreach (var section in gpuSections)
                {
                    if (!section.Contains("BUS:")) continue;
                    var lines = section.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Count() != 0)
                    {
                        var infoString = lines[0];
                        var busIDSub = infoString.Substring(infoString.IndexOf("BUS:") + 4);
                        busIDSub = busIDSub.Replace(']', ' ');
                        int.TryParse(busIDSub, out var busID);

                        var indexSub = infoString.Substring(0, 2).Trim();
                        int.TryParse(indexSub, out var indexID);

                        foreach (var gpu in gpus)
                        {
                            if (gpu.PCIeBusID == busID)
                            {
                                mappedDevices.Add(gpu.UUID, indexID);
                                break;
                            }
                        }
                    }

                }
            }
            return mappedDevices;
        }
    }
}
