using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Devices;

namespace NiceHashMiner.Stats.Models
{
    public class DeviceUpdateEventArgs : EventArgs
    {
        public readonly List<ComputeDevice> Devices;

        public DeviceUpdateEventArgs(List<ComputeDevice> devs)
        {
            Devices = devs;
        }
    }
}
