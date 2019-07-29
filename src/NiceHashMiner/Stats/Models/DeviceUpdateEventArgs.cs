using System;
using System.Collections.Generic;
using NiceHashMiner.Mining;

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
