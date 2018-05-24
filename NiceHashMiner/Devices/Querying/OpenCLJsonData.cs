using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices.Querying
{
    public class OpenCLJsonData
    {
        public string PlatformName = "NONE";
        public int PlatformNum = 0;
        public List<OpenCLDevice> Devices = new List<OpenCLDevice>();
    }
}
