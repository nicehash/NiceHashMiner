using NHM.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Excavator
{
    internal static class ExcavatorTaskHelpers
    {
        public static Task TryDelay(TimeSpan delay, CancellationToken token)
        {
            return TryDelay((int)delay.TotalMilliseconds, token);
        }

        public static Task TryDelay(int millisecondsDelay, CancellationToken token)
        {
            try
            {
                return Task.Delay(millisecondsDelay, token);
            }
            catch (Exception e)
            {
                // catch all
                Logger.Debug("ExcavatorTaskHelpers", $"catch block {e.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
