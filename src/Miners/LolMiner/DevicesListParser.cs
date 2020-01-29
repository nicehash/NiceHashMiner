using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LolMiner
{
    internal class DevicesListParser
    {
        internal enum LineSearchState
        {
            SEARCH_DEVICE = 0,
            SEARCH_ADDRESS
        }

        const string Device = "Device";
        const string Address = "Address:";

        public static Dictionary<string, int> ParseLolMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();

            var mappedDevices = new Dictionary<string, int>();
            var miner_PCI_ID_MinerID = new Dictionary<int, int>();
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                var devMinerID = -1;
                var nextSearch = LineSearchState.SEARCH_DEVICE;
                foreach (var line in lines)
                {
                    if (nextSearch == LineSearchState.SEARCH_DEVICE && line.Contains(Device))
                    {
                        var trimmedAddress = line.Replace(Device, "").Trim().Split(':');
                        if (Int32.TryParse(trimmedAddress.FirstOrDefault(), out devMinerID))
                        {
                            nextSearch = LineSearchState.SEARCH_ADDRESS;
                        }
                    }
                    else if (nextSearch == LineSearchState.SEARCH_ADDRESS && line.Contains(Address))
                    {
                        var trimmedAddress = line.Replace(Address, "").Trim().Split(':');
                        var devPCI_ID = -1;
                        if (Int32.TryParse(trimmedAddress.FirstOrDefault(), out devPCI_ID))
                        {
                            miner_PCI_ID_MinerID[devPCI_ID] = devMinerID;
                            nextSearch = LineSearchState.SEARCH_DEVICE;
                        }
                    }
                }
                // map
                foreach (var kvp in miner_PCI_ID_MinerID)
                {
                    var pci = kvp.Key;
                    var id = kvp.Value;
                    var gpuUuid = gpus.Where(gpu => gpu.PCIeBusID == pci).FirstOrDefault();
                    if (gpuUuid == null) continue;
                    mappedDevices[gpuUuid.UUID] = id;
                }
            }
            return mappedDevices;
        }
    }
}
