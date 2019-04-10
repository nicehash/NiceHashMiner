using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MinerPluginToolkitV1
{
    // TODO this class is completely redundant
    public class MiningProcess
    {
        public MiningProcess(Process handle)
        {
            Handle = handle;
        }
        // TODO make ProcessLibrary swappable
        public Process Handle { get; }
    }
}
