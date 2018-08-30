using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.IdleChecking
{
    internal class IdleChangedEventArgs : EventArgs
    {
        public readonly bool IsIdle;

        public IdleChangedEventArgs(bool idle)
        {
            IsIdle = idle;
        }
    }
}
