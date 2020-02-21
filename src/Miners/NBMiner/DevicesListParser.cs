using Newtonsoft.Json;
using NHM.Common;
using NHM.Common.Device;
using System.Collections.Generic;
using System.Linq;

namespace NBMiner
{
    internal class Device
    {
        public int device_id { get; set; }
        public object memory { get; set; }
        public string name { get; set; }
        public int pci_bus_id { get; set; }
    }
    internal class NbMinerDevices
    {
        public List<Device> devices { get; set; }
    }
    internal class DevicesListParser
    {
        public static Dictionary<string, int> ParseNBMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var gpus = baseDevices.Where(dev => dev is IGpuDevice).Cast<IGpuDevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            if (gpus.Count() == 0)
            {
                return mappedDevices;
            }

            var parsedOutput = JsonConvert.DeserializeObject<NbMinerDevices>(output);
            foreach (var device in parsedOutput.devices)
            {
                foreach (var gpu in gpus)
                {
                    if (gpu.PCIeBusID == device.pci_bus_id)
                    {
                        Logger.Info("NBM-DLP", $"{gpu.UUID} {gpu.PCIeBusID} - {device.device_id}");
                        mappedDevices.Add(gpu.UUID, device.device_id);
                        break;
                    }
                }
            }
            return mappedDevices;
        }
    }
}
