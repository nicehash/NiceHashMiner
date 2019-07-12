using System;

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
