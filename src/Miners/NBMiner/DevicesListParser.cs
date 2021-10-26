using Newtonsoft.Json;
using NHM.Common.Device;
using System.Collections.Generic;
using System.Linq;

namespace NBMiner
{
    internal class DevicesListParser
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

        public static Dictionary<string, int> ParseNBMinerOutput(string minerListDevicesJSON, IEnumerable<BaseDevice> nhmDevices)
        {
            var minerDevices = JsonConvert.DeserializeObject<NbMinerDevices>(minerListDevicesJSON)?.devices ?? Enumerable.Empty<Device>();
            var mappedDevices = nhmDevices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .Select(gpu => (gpu, minerDevice: minerDevices?.FirstOrDefault(dev => gpu.PCIeBusID == dev.pci_bus_id) ?? null))
                .Where(p => p.minerDevice != null)
                .ToDictionary(p => p.gpu.UUID, p => p.minerDevice.device_id);
            return mappedDevices;
        }
    }
}
