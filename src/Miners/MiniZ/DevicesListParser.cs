using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniZ
{
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseMiniZOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            var outputData = output.Substring(output.IndexOf('#'));  //start of gpu data
            var lines = outputData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                foreach (var line in lines)
                {
                    var dataArray = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (dataArray.Count() != 0)
                    {
                        var id = dataArray[0].Remove(0, 1);
                        var indexID = Convert.ToInt32(id);

                        var fullBusID = dataArray.LastOrDefault();
                        var busID = fullBusID.Split(':')[1];
                        int comparePCIeBusID = int.Parse(busID, System.Globalization.NumberStyles.HexNumber);

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
            }
            return mappedDevices;
        }
    }
}
