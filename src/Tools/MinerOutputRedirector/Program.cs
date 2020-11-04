using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MinerOutputRedirector
{
    class Program
    {
        static List<string> DeviceUUIDs = new List<string>();
        static string LogsPath = "";
        static void Main(string[] args)
        {
            if(args.Count() != 5)
            {
                Console.WriteLine("USAGE: MinerOutputRedirector.exe logsPath pathToMiner workingDir minerArguments deviceUUIDs");
                return;
            }

            try
            {
                LogsPath = args[0];
                DeviceUUIDs = args[4].Split('|').ToList();
                var minerProcess = new Process();
                minerProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = args[1],
                    WorkingDirectory = args[2],
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = args[3]
                };
                //minerProcess.EnableRaisingEvents = true;

                minerProcess.OutputDataReceived += DataReceivedEventHandler;
                minerProcess.ErrorDataReceived += DataReceivedEventHandler;

                minerProcess.Start();
                minerProcess.BeginOutputReadLine();
                minerProcess.BeginErrorReadLine();
                minerProcess.WaitForExit();
            }
            catch(Exception ex)
            {
                Console.WriteLine("MinerOutputRedirector error: ", ex.Message);
            }


            //minerProcess.Close();
        }

        private static void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            try
            {
                // Prepend line numbers to each line of the output.
                if (!string.IsNullOrEmpty(e.Data))
                {
                    // Write the redirected output to this application's window.
                    Console.WriteLine(e.Data);
                    foreach (var devUUID in DeviceUUIDs)
                    {
                        var deviceLogPath = Path.Combine(LogsPath, $"device_{devUUID}_log.txt");
                        File.AppendAllText(deviceLogPath, e.Data + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DataReceivedEventHandler failed: {ex.Message}");
            }
        }
    }
}
