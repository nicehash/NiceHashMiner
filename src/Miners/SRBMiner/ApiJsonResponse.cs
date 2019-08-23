using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRBMiner
{
    [Serializable]
    internal class Device
    {
        public string device { get; set; }
        public int device_id { get; set; }
        public string model { get; set; }
        public int bus_id { get; set; }
        public int hashrate { get; set; }
    }

    [Serializable]
    internal class ApiJsonResponse
    {
        public int hashrate_total_now { get; set; }
        public List<Device> devices { get; set; }
    }
}
