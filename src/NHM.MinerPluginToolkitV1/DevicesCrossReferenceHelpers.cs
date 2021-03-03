using NHM.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NHM.MinerPluginToolkitV1
{
    /// <summary>
    /// DevicesCrossReferenceHelpers class is used in combinaton with IDevicesCrossReference to get mapped device Ids
    /// </summary>
    public class DevicesCrossReferenceHelpers
    {
        /// <summary>
        /// MinerOutput creates new process and gets its StandardOutput
        /// </summary>
        public static async Task<string> MinerOutput(string path, string arguments, int timeoutMilliseconds = 30 * 1000)
        {
            string output = "";
            try
            {
                string workingDirectory = Path.GetDirectoryName(path);
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = workingDirectory,
                };

                using (var getDevicesHandle = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
                using (var ct = new CancellationTokenSource(timeoutMilliseconds))
                {
                    getDevicesHandle.Start();
                    Action<string> getDevicesHandleStop = (string stopFrom) =>
                    {
                        try
                        {
                            var isRunning = !getDevicesHandle?.HasExited ?? false;
                            if (!isRunning) return;
                            getDevicesHandle.CloseMainWindow();
                            var hasExited = getDevicesHandle?.WaitForExit(1000) ?? false;
                            if (!hasExited) getDevicesHandle.Kill();
                        }
                        catch (Exception e)
                        {
                            Logger.Error("DeviceCrossReference", $"Unable to get DevicesHandle: {e.Message}");
                        }
                    };
                    ct.Token.Register(() => getDevicesHandleStop("from cancel token"));
                    output = await getDevicesHandle.StandardOutput.ReadToEndAsync();
                    if (output == "")
                    {
                        output = await getDevicesHandle.StandardError.ReadToEndAsync();
                    }
                    getDevicesHandleStop("after read to end");
                }
            }
            catch (Exception e)
            {
                Logger.Error("DevicesCrossReferenceHelpers", $"Error occured while getting miner output: {e.Message}");
                return "";
            }
            return output;
        }

#warning DO NOT DELETE THIS function old plugins rely on IT!
        /// <summary>
        /// MinerOutput creates new process and gets its StandardOutput
        /// </summary>
        public static async Task<string> MinerOutput(string path, string arguments, IEnumerable<string> breakLines, int timeoutMilliseconds = 30 * 1000)
        {
            var output = new StringBuilder();
            try
            {
                string workingDirectory = Path.GetDirectoryName(path);
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WorkingDirectory = workingDirectory,
                };

                using (var getDevicesHandle = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
                using (var ct = new CancellationTokenSource(timeoutMilliseconds))
                {
                    getDevicesHandle.Start();
                    Action<string> getDevicesHandleStop = (string stopFrom) =>
                    {
                        try
                        {
                            var isRunning = !getDevicesHandle?.HasExited ?? false;
                            if (!isRunning) return;
                            getDevicesHandle.CloseMainWindow();
                            var hasExited = getDevicesHandle?.WaitForExit(1000) ?? false;
                            if (!hasExited) getDevicesHandle.Kill();
                        }
                        catch (Exception e)
                        {
                            Logger.Error("DeviceCrossReference", $"Unable to get DevicesHandle: {e.Message}");
                        }
                    };
                    ct.Token.Register(() => getDevicesHandleStop("from cancel token"));

                    var line = "";
                    while (!getDevicesHandle.HasExited)
                    {
                        line = await getDevicesHandle.StandardOutput.ReadLineAsync();
                        if (line != null)
                        {
                            //Console.WriteLine(line);
                            if (breakLines.Any(breakLine => line.Contains(breakLine)))
                            {
                                getDevicesHandleStop("from read line loop");
                                break;
                            }
                            output.AppendLine(line);
                        }
                    }

                    getDevicesHandleStop("after read to end");
                }
            }
            catch (Exception e)
            {
                Logger.Error("DevicesCrossReferenceHelpers", $"Error occured while getting miner output: {e.Message}");
                return output.ToString();
            }
            return output.ToString();
        }
    }
}
