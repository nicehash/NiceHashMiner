using NiceHashMinerLegacy.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1
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
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (var getDevicesHandle = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
                using (var ct = new CancellationTokenSource(timeoutMilliseconds))
                {
                    getDevicesHandle.Start();
                    Action<string> getDevicesHandleStop = (string stopFrom) => {
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
                            // TODO log
                        }
                    };
                    ct.Token.Register(() => getDevicesHandleStop("from cancel token"));
                    output = await getDevicesHandle.StandardOutput.ReadToEndAsync();
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
    }
}
