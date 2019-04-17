using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1
{
    public class DevicesCrossReferenceHelpers
    {
        public static async Task<string> MinerOutput(string path, string arguments, int timeoutMilliseconds = 30 * 1000)
        {
            string output = "";
            try
            {
                var getDevicesHandle = new Process
                {
                    StartInfo =
                    {
                                FileName = path,
                                Arguments = arguments,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardOutput = true
                    },
                    EnableRaisingEvents = true,
                };
                getDevicesHandle.Start();
                using (var ct = new CancellationTokenSource(timeoutMilliseconds))
                {
                    ct.Token.Register(() =>
                    {
                        try
                        {
                            getDevicesHandle.Kill();
                        }
                        catch { }
                    });
                    output = await getDevicesHandle.StandardOutput.ReadToEndAsync();
                    getDevicesHandle.WaitForExit();
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return output;
        }
    }
}
