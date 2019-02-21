using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMinerLegacy.Common.Device
{
    public class CPUDevice : BaseDevice
    {
        public CPUDevice(BaseDevice bd, int threads, bool supportsHyperThreading, ulong affinityMask) : base(bd)
        {
            Threads = threads;
            SupportsHyperThreading = supportsHyperThreading;
            AffinityMask = affinityMask;
        }

        public int Threads { get; }
        public bool SupportsHyperThreading { get; }
        public ulong AffinityMask { get; protected set; } // TODO check if this makes any sense
    }
}
