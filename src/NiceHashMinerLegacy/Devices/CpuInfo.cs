using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices
{
    internal struct CpuInfo
    {
        public string VendorID;
        public string Family;
        public string Model;
        public string PhysicalID;
        public string ModelName;

        // maybe this will come in handy
        public static IEnumerable<CpuInfo> GetCpuInfo()
        {
            using (var proc = new ManagementObjectSearcher("select * from Win32_Processor"))
            {
                foreach (var obj in proc.Get())
                {
                    var info = new CpuInfo
                    {
                        Family = obj["Family"].ToString(),
                        VendorID = obj["Manufacturer"].ToString(),
                        ModelName = obj["Name"].ToString(),
                        PhysicalID = obj["ProcessorID"].ToString()
                    };

                    yield return info;
                }
            }
        }
    }
}
