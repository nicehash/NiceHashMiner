using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBMiner
{
    internal class Device
    {
        public int cc_major { get; set; }
        public int cc_minor { get; set; }
        public int cuda_id { get; set; }
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
            var cudaGpus = baseDevices.Where(dev => dev is CUDADevice).Cast<CUDADevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            if (cudaGpus.Count() == 0)
            {
                return mappedDevices;
            }

            var parsedOutput = JsonConvert.DeserializeObject<NbMinerDevices>(output);
            foreach (var device in parsedOutput.devices)
            {
                foreach (var gpu in cudaGpus)
                {
                    if (gpu.PCIeBusID == device.pci_bus_id)
                    {
                        mappedDevices.Add(gpu.UUID, device.cuda_id);
                        break;
                    }
                }
            }
            return mappedDevices;
        }
    }
}
