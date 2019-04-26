using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoMiner
{
    [Serializable]
    public class JsonApiResponse
    {
        public List<Dictionary<string, Dictionary<string, object>>> Algorithms { get; set; }
        public List<Dictionary<string, DeviceData>> Devices { get; set; }
    }

    [Serializable]
    public class DeviceData
    {
        public string Name { get; set; }
        public string Platform { get; set; }
        public string Pci { get; set; }
        public int Temperature { get; set; }
        public double Power { get; set; }
    }
}
