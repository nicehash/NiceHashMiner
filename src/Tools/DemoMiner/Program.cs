using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoMiner
{
    class Program
    {
        private static CancellationTokenSource _cancellationTokenSource = null;
        private static int _beforeExitWaitSeconds = 5;
        static void Main(string[] args)
        {
            Console.WriteLine("args :");

            foreach (var arg in args)
            {
                Console.Write(arg);
            }
            _cancellationTokenSource = new CancellationTokenSource();
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                Console.WriteLine("Exit called");
                _cancellationTokenSource?.Cancel();
            };
            MainLoop(_cancellationTokenSource.Token).Wait();

            Thread.Sleep(1000);
            _cancellationTokenSource.Dispose();
        }

        private static async Task MainLoop(CancellationToken stop)
        {
            Console.WriteLine("Statring main loop");
            while (!stop.IsCancellationRequested)
            {
                await Task.Delay(500, stop);
            }
            Console.WriteLine("Exiting main loop");

            Console.WriteLine($"Before exit cleanup {_beforeExitWaitSeconds}s");
            await Task.Delay(_beforeExitWaitSeconds * 1000);
            Console.WriteLine($"Cleanup finished");
        }
    }
}
