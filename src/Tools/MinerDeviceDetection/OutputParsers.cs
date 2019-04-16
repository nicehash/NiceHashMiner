using Newtonsoft.Json;
using NiceHashMinerLegacy.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerDeviceDetection
{
    static class OutputParsers
    {
        public static Dictionary<string, int> ParseGMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var amdGpus = baseDevices.Where(dev => dev is AMDDevice).Cast<AMDDevice>();
            var cudaGpus = baseDevices.Where(dev => dev is CUDADevice).Cast<CUDADevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if(lines.Count() != 0)
            {
                foreach(var line in lines)
                {
                    var tmpArray = line.Split(new[] { "PCI:" }, StringSplitOptions.RemoveEmptyEntries);
                    var fullPciId = tmpArray[1].Split(':');
                    var pciId = fullPciId[1];
                    var id = line.Substring(0, line.IndexOf(':')).Remove(0,3);
                    var lastChar = pciId.ToCharArray();
                    if (char.IsDigit(lastChar.Last()))
                    {
                        foreach (var gpu in cudaGpus)
                        {
                            if (gpu.PCIeBusID == Convert.ToInt32(pciId))
                            {
                                mappedDevices.Add(gpu.UUID, Convert.ToInt32(id));
                                break;
                            }
                        }
                        foreach (var gpu in amdGpus)
                        {
                            if (gpu.PCIeBusID == Convert.ToInt32(pciId))
                            {
                                mappedDevices.Add(gpu.UUID, Convert.ToInt32(id));
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var gpu in cudaGpus)
                        {
                            if (gpu.PCIeBusID == int.Parse(pciId, System.Globalization.NumberStyles.HexNumber))
                            {
                                mappedDevices.Add(gpu.UUID, Convert.ToInt32(id));
                                break;
                            }
                        }
                        foreach (var gpu in amdGpus)
                        {
                            if (gpu.PCIeBusID == int.Parse(pciId, System.Globalization.NumberStyles.HexNumber))
                            {
                                mappedDevices.Add(gpu.UUID, Convert.ToInt32(id));
                                break;
                            }
                        }
                    }
                }
            }
            return mappedDevices;
        }

        public static Dictionary<string, int> ParsePhoenixOutput(string output, List<BaseDevice> baseDevices)
        {
            var amdGpus = baseDevices.Where(dev => dev is AMDDevice).Cast<AMDDevice>();
            var cudaGpus = baseDevices.Where(dev => dev is CUDADevice).Cast<CUDADevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            //delete everything until first GPU
            if (!output.Contains("GPU1"))
            {
                return mappedDevices;
            }
            var filteredOutput = output.Substring(output.IndexOf("GPU1"));
            var lines = filteredOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                int index = 0;
                foreach (var line in lines)
                {
                    var tmpArray = line.Substring(line.IndexOf('('));
                    tmpArray = tmpArray.Remove(tmpArray.IndexOf(')'));
                    var fullPciId = tmpArray.Split(' ');
                    var pciId = fullPciId[1];

                    foreach (var gpu in cudaGpus)
                    {
                        if (gpu.PCIeBusID == Convert.ToInt32(pciId))
                        {
                            mappedDevices.Add(gpu.UUID, index);
                            break;
                        }
                    }
                    foreach (var gpu in amdGpus)
                    {
                        if (gpu.PCIeBusID == Convert.ToInt32(pciId))
                        {
                            mappedDevices.Add(gpu.UUID, index);
                            break;
                        }
                    }
                    index++;
                }
            }
            return mappedDevices;
        }

        public static Dictionary<string, int> ParseTTMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var cudaGpus = baseDevices.Where(dev => dev is CUDADevice).Cast<CUDADevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            if (!output.Contains("CUDA devices:"))
            {
                return mappedDevices;
            }
            var filteredOutput = output.Substring(output.IndexOf("*"));
            var lines = filteredOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() != 0)
            {
                foreach (var line in lines)
                {
                    var pciId = line.Substring(line.IndexOf(':'), 3).Remove(0,1);
                    var index = line.Substring(line.IndexOf('#'), 3).Remove(0,1);
                    foreach (var gpu in cudaGpus)
                    {
                        if (gpu.PCIeBusID == Convert.ToInt32(pciId))
                        {
                            mappedDevices.Add(gpu.UUID, Convert.ToInt32(index));
                            break;
                        }
                    }
                }
            }
            return mappedDevices;
        }

        public class Device
        {
            public int cc_major { get; set; }
            public int cc_minor { get; set; }
            public int cuda_id { get; set; }
            public object memory { get; set; }
            public string name { get; set; }
            public int pci_bus_id { get; set; }
        }

        public class NbMinerDevices
        {
            public List<Device> devices { get; set; }
        }

        public static Dictionary<string, int> ParseNBMinerOutput(string output, List<BaseDevice> baseDevices)
        {
            var cudaGpus = baseDevices.Where(dev => dev is CUDADevice).Cast<CUDADevice>();

            Dictionary<string, int> mappedDevices = new Dictionary<string, int>();
            if (cudaGpus.Count() == 0)
            {
                return mappedDevices;
            }

            var parsedOutput = JsonConvert.DeserializeObject<NbMinerDevices>(output);
            foreach(var device in parsedOutput.devices)
            {
                foreach(var gpu in cudaGpus)
                {
                    if(gpu.PCIeBusID == device.pci_bus_id)
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
