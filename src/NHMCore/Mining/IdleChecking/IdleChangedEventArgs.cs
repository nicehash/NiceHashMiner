using System;

namespace NHMCore.Mining.IdleChecking
{
    public class IdleChangedEventArgs : EventArgs
    {
        public readonly bool IsIdle;

        public IdleChangedEventArgs(bool idle)
        {
            IsIdle = idle;
        }
    }
}
