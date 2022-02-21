using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using NHM.Common;
using System.Threading;
using System.Diagnostics;

namespace NHMCore.Utils
{
    public static class OutsideProcessMonitor
    {
        private static bool _isDisposed = false;
        private static int CheckIntervalInSeconds = 5;
        private static ManagementEventWatcher StartProcessWatch { get; set; } = null;
        private static Task CheckProcessTask { get; set; } = null;



        public static void Init()
        {
            if (Helpers.IsElevated)
            {

            }
            else
            {
                CheckProcessTask = Task.Run(async () =>
                {
                    await ProcessMonitorLoop();
                });
            }
        }

        private static void Deinit()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                CheckProcessTask.Dispose();
            }
        }

        static void InitProcessWatch()
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(ProcessWatchEventArrived);
            startWatch.Start();
        }

        static void ProcessWatchEventArrived(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Process started: {0}", e.NewEvent.Properties["ProcessName"].Value);
            //if (this is the process I'm interested in)
            //{
            //    startWatch.Stop();
            //}
        }

        private static async Task ProcessMonitorLoop()
        {
            while (!_isDisposed)
            {
                await MonitorIteration();
                Task.Delay(CheckIntervalInSeconds * 1000).Wait();
            }
        }

        private static async Task MonitorIteration()
        {
            var processes = Process.GetProcesses();
            bool hasInstallerOrUpdaterRunning () 
            {
                var processListInstallUpdate = processes
                    .Where(proc => proc.ProcessName.ToLower().Contains("nhm_windows"))
                    .Where(proc => proc.MainWindowTitle.ToLower().Contains("setup"));
                return processListInstallUpdate.Any();
            }

            bool hasUninstallerRunning()
            {
                var processListUninstall = processes
                    .Where(proc => proc.ProcessName.ToLower().Contains("un_a"))
                    .Where(proc => proc.MainWindowTitle.ToLower().Contains("uninstall"));
                return processListUninstall.Any();
            }

            if (hasInstallerOrUpdaterRunning() || hasUninstallerRunning())
            {
                try
                {
                    Logger.Info("OutsideProcessMonitor", "Installer/uninstaller is running, attempting NHM shutdown");
                    await ApplicationStateManager.BeforeExit();
                    ApplicationStateManager.ExecuteApplicationExit();
                    Logger.Info("OutsideProcessMonitor", "NHM closed successfully due to installer/uninstaller.");
                }
                catch (Exception ex)
                {
                    Logger.Error("OutsideProcessMonitor", ex.Message);
                }
                finally
                {
                    Deinit();
                }
            }
        }
    }
}
