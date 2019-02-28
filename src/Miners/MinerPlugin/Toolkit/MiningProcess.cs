using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MinerPlugin.Toolkit
{
    public class MiningProcess : IDisposable
    {
        public MiningProcess(Process handle)
        {
            Handle = handle;
        }
        // TODO make ProcessLibrary swappable
        public Process Handle { get; }

        public void Dispose()
        {
            // Process handles should be disposed, the OS has a limited number available
            Handle?.Dispose();
        }
    }
}
