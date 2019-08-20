using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRBMiner
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseSRBMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is AMDDevice).Cast<AMDDevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();

            var gpuSections = output.Split(new[] { "DeviceID" }, StringSplitOptions.RemoveEmptyEntries);
            if (gpuSections.Count() != 0)
            {
                foreach(var section in gpuSections)
                {
                    if (!section.Contains("BUSID:")) continue;
                    var lines = section.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Count() != 0)
                    {
                        var busIDString = lines[1];
                        var busIDSub = busIDString.Substring(busIDString.IndexOf("BUSID:") + 7);
                        int.TryParse(busIDSub, out var busID);

                        var indexString = lines[0];
                        indexString = indexString.Replace('[', ' ').Replace(']', ' ');
                        int.TryParse(indexString, out var indexID);

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
