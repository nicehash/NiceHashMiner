using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamRedMiner
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseTeamRedMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is AMDDevice).Cast<AMDDevice>();

            var mappedDevices = new Dictionary<string, int>();
            var cropped = output.Substring(output.IndexOf("Nr CUs"));
            var lines = cropped.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                foreach (var line in lines)
                {
                    if (line.Contains("---") || line.Contains("CUs") || line.Contains("shutdown")) continue;
                    var tmpArray = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var minerID = tmpArray[2];
                    var platform = tmpArray[3];
                    var fullBusID = tmpArray[5];
                    var busID = fullBusID.Split(':').FirstOrDefault();
                    int.TryParse(busID, out var comparePCIeBusID);
                    int.TryParse(minerID, out var ID);
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
