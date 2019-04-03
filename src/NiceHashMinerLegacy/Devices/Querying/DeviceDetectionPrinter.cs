using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NiceHashMiner.Devices.Querying
{
    public static class DeviceDetectionPrinter
    {
        public static async Task<T>GetDeviceDetectionResultAsync<T>(string args, int milliseconds = (30 * 1000)) where T : class
        {
            var run = new Process()
            {
                StartInfo =
                {
                    FileName = "DeviceDetectionPrinter.exe",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true,
            };

            var resultLock = new object();
            bool resultFound = false;
            string lineReadData = "";

            T result = null;
            var tcs = new TaskCompletionSource<T>();
            
            var lineRead = new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) =>
            {
                lock (resultLock)
                {
                    if (resultFound) return;
                    if (string.IsNullOrEmpty(e.Data)) return;
                    if (string.IsNullOrWhiteSpace(e.Data)) return;

                    try
                    {
                        lineReadData += e.Data;
                        var isResult = JsonConvert.DeserializeObject<T>(lineReadData, Globals.JsonSettings);
                        if (isResult != null)
                        {
                            resultFound = true;
                            tcs.TrySetResult(isResult);
                            run.Close();
                        }
                    }
                    catch
                    { }
                }
            });
            try
            {
                run.OutputDataReceived += lineRead;
                if (!run.Start())
                {
                    throw new InvalidOperationException("Could not start process: " + run);
                }
                var ct = new CancellationTokenSource(milliseconds);
                ct.Token.Register(() => {
                    try {
                        tcs.TrySetCanceled();
                    }
                    catch{}
                });
                run.BeginOutputReadLine();
                // TODO this here is hacky
                var waitForExit = Task.Run(() =>
                {
                    if (milliseconds < 0)
                    {
                        run.WaitForExit();
                    }
                    else
                    {
                        run.WaitForExit(milliseconds);
                    }
                });
                result = await tcs.Task;
                if (result == null)
                {
                    Helpers.ConsolePrint("DeviceDetectionPrinter", $"result is NULL lineReadData='{lineReadData}'");
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("DeviceDetectionPrinter", $"threw Exception: {ex.Message}");
            }
            finally
            {
                run.OutputDataReceived -= lineRead;
                run.Dispose();
            }

            return result;
        }
    }
}
