using System;
using System.Collections.Generic;
using NHMCore.Mining;

namespace NHMCore.Stats.Models
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
