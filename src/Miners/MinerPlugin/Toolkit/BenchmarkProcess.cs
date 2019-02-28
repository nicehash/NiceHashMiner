using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinerPlugin.Toolkit
{
    // This one just reads from std out and err
    public class BenchmarkProcess : IDisposable
    {
        public BenchmarkProcess(Process p)
        {
            Handle = p;
        }

        public BenchmarkProcess(string binPath, string workingDir, string commandLine, Dictionary<string, string> environmentVariables = null)
        {
            Handle = MinerToolkit.CreateBenchmarkProcess(binPath, workingDir, commandLine, environmentVariables);
        }

        public delegate (double, bool) CheckDataFun(string data);
        public CheckDataFun CheckData { get; set; }

        public Process Handle { get; }
        public double Speed { get; private set; } = default(double);
        public bool Success { get; private set; } = default(bool);

        private bool exitCalled = false;

        private void BenchmarkOutputErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (Success || string.IsNullOrEmpty(e.Data) || exitCalled)
            {
                return;
            }

            (Speed, Success) = CheckData(e.Data);
            if (Success)
            {
                TryExit();
            }
        }

        public void TryExit()
        {
            try
            {
                var isRunning = Process.GetProcessById(Handle.Id) != null;
                if (isRunning)
                {
                    try
                    {
                        Handle.OutputDataReceived -= BenchmarkOutputErrorDataReceived;
                        Handle.ErrorDataReceived -= BenchmarkOutputErrorDataReceived;
                        Handle.Kill();
                    }
                    catch
                    { } // TODO gotta catch them all!!!
                }
            }
            catch { }
        }

        public Task<(double, bool)> Execute(CancellationToken stop)
        {
            var tcs = new TaskCompletionSource<(double, bool)>();
            // TODO throw if handle is not initialized
            //if (Handle == null)
            //{
            //    return Task.;
            //}

            //tcs.SetCanceled()

            Handle.Exited += (s, ea) => {
                exitCalled = true;
                tcs.SetResult((Speed, Success));
            };
            Handle.OutputDataReceived += BenchmarkOutputErrorDataReceived;
            Handle.ErrorDataReceived += BenchmarkOutputErrorDataReceived;

            // TODO BeforeStart

            if (!Handle.Start())
            {
                // TODO maybe not throw, maybe just return failed result instead
                throw new InvalidOperationException("Could not start process: " + Handle);
            }

            // TODO AfterStart

            Handle.BeginOutputReadLine();
            Handle.BeginErrorReadLine();
            stop.Register(() => {
                TryExit();
            });

            return tcs.Task;
        }

        public void Dispose()
        {
            Handle?.Dispose();
        }
    }
}
