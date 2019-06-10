using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.AMD
{
    internal struct AmdBusIDInfo
    {
        public int BusID { get; internal set; }
        public string Uuid { get; internal set; }
        public int Adl1Index { get; internal set; }
        public int Adl2Index { get; internal set; }
    }
}
