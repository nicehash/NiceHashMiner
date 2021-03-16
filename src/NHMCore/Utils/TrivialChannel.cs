using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    internal class TrivialChannel<T> where T : class
    {
        private readonly ConcurrentQueue<T> _commandQueue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(0);

        public TrivialChannel(int initialCount = 0) 
        {
            _sem = new SemaphoreSlim(initialCount);
        }

        public void Enqueue(T c)
        {
            _commandQueue.Enqueue(c);
            _sem.Release();
        }

        public async Task<(T t, bool hasTimedout, string exceptionString)> ReadAsync(TimeSpan timeout, CancellationToken stop)
        {
            T c = null;
            bool hasTimedout = false;
            string exceptionString = null;
            try
            {
                hasTimedout = await _sem.WaitAsync(timeout, stop);
                _commandQueue.TryDequeue(out c);
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
            catch (Exception e)
            {
                exceptionString = $"TrivialChannel.ReadAsync {e.Message}";
            }
            return (c, hasTimedout, exceptionString);
        }
    }
}
