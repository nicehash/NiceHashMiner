using MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XmrStak.Configs
{
    static class ConfigHelpers
    {

        //public static void PrepareConfigFiles(string path, )
        //{

        //}

        //public static 

        private static Task CreateConfigFileTask(string binPath, string cwdPath, string commandLine, Dictionary<string, string> environmentVariables, CancellationToken stop)
        {
            var genSettingsHandle = new BenchmarkProcess(binPath, cwdPath, commandLine, environmentVariables);
            var timeoutTime = TimeSpan.FromSeconds(30);
            var delayTime = TimeSpan.FromSeconds(5);
            return MinerToolkit.WaitBenchmarkResult(genSettingsHandle, timeoutTime, delayTime, stop);
        }

        public static async Task<bool> CreateConfigFile(string configName, string binPath, string cwdPath, string commandLine, Dictionary<string, string> environmentVariables = null)
        {
            var configFilePath = Path.Combine(cwdPath, configName);
            try
            {
                using (var stopProcess = new CancellationTokenSource())
                {
                    var configCreateTask = CreateConfigFileTask(binPath, cwdPath, commandLine, environmentVariables, stopProcess.Token);
                    var start = DateTime.Now;
                    while (DateTime.Now.Subtract(start).Seconds < 34)
                    {
                        if (configCreateTask.IsCompleted) break;
                        if (File.Exists(configFilePath))
                        {
                            stopProcess.Cancel();
                            return true;
                        }
                        await Task.Delay(300);
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine($"CreateConfigFile {e.Message}");
            }
            return File.Exists(configFilePath);
        }
    }
}
