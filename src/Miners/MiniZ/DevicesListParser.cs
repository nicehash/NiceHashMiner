using NHM.Common;
using NHM.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniZ
{
    internal class DevicesListParser
    {
        private static int GetDeviceID(string deviceIDInfo)
        {
            if (deviceIDInfo == null) return -1;
            try
            {
                var idStr = deviceIDInfo.Split(' ').Where(entry => entry.Contains("#")).FirstOrDefault().Trim('#');
                return int.Parse(idStr);
            }
            catch (Exception e)
            {
                Logger.Error("MiniZ.DevicesListParser.GetDeviceID", $"{e}");
            }
            return -1;
        }

        private static int GetBusID(string deviceBusID)
        {
            if (deviceBusID == null || !deviceBusID.ToLower().Contains("busid")) return -1;
            try
            {
                var busID = deviceBusID.Split(':')[2];
                int comparePCIeBusID = int.Parse(busID, System.Globalization.NumberStyles.HexNumber);
                return comparePCIeBusID;
            }
            catch (Exception e)
            {
                Logger.Error("MiniZ.DevicesListParser.GetBusID", $"{e}");
            }
            return -1;
        }

        public static Dictionary<string, int> ParseMiniZOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();

            var mappedDevices = new Dictionary<string, int>();
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(line => line.Contains("BusID"));
            foreach (var line in lines)
            {
                // order is ID and GPU | SM ver | SM count | RAM | BusID
                var dataArray = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var minerID = GetDeviceID(dataArray.FirstOrDefault());
                var busID = GetBusID(dataArray.LastOrDefault());
                if (minerID == -1 || busID == -1)
                {
                    Logger.Error("MiniZ.DevicesListParser.ParseMiniZOutput", $"MinerID({minerID}) or BusID({busID}) is negative {line}");
                    continue;
                }
                var devWithBusID = gpus.Where(gpu => gpu.PCIeBusID == busID).FirstOrDefault();
                if (devWithBusID == null)
                {
                    Logger.Error("MiniZ.DevicesListParser.ParseMiniZOutput", $"Cannot find GPU with PCI bus {busID}");
                    continue;
                }
                mappedDevices.Add(devWithBusID.UUID, minerID);
            }
            return mappedDevices;
        }
    }
}
