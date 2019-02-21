using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MinerPlugin.Toolkit
{
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
