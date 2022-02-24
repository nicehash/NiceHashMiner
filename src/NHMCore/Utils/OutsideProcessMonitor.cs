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
        private static string Tag = "OutsideProcessMonitor";
        private static ManagementEventWatcher ProcessInitWatch { get; set; } = null;
        private static MonitorState RunState { get; set; } = MonitorState.None;
        private enum MonitorState
        {
            None,
            ElevatedMode,
            FallbackMode,
        }
        public static async Task Init(CancellationToken stop)
        {
            if(IsNHMShutdownNeeded()) await GracefulShutdown(); // initial check if installer running before NHM
            //if (Helpers.IsElevated && InitAndStartProcessWatcher())
            //{
            //    Logger.Info(Tag, "Running with elevated rights");
            //    RunState = MonitorState.ElevatedMode;
            //}
            //else
            {
                Logger.Info(Tag, "Running with normal rights");
                _ = Task.Run(async () =>
                {
                    await ProcessMonitorLoop(stop);
                });
                RunState = MonitorState.FallbackMode;
            }
            stop.Register(Deinit);
        }
        private static void Deinit()//deinit is called from inside of checker, therefore it waits for itself...
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if (RunState == MonitorState.ElevatedMode && ProcessInitWatch != null)
                {
                    ProcessInitWatch.Stop();
                    //ProcessInitWatch.Dispose();
                }
            }
        }
        private static bool InitAndStartProcessWatcher()
        {
            try
            {
                ProcessInitWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                ProcessInitWatch.EventArrived += async (s, e) =>
                {
                    Logger.Info(Tag, "Received event: " + e.NewEvent.Properties["ProcessName"].Value);
                    await CheckNewEventAndShutdownIfNeeded();
                };
                ProcessInitWatch.Start();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, $"Failed to init ManagementEventWatcher {ex.Message}");
            }
            return false;
        }
        private static async Task ProcessMonitorLoop(CancellationToken stop)
        {
            try
            {
                while (!IsNHMShutdownNeeded())
                {
                    await Task.Delay(CheckIntervalInSeconds * 1000, stop);
                }
                await GracefulShutdown();
            }
            catch {}
        }
        private static async Task CheckNewEventAndShutdownIfNeeded()
        {
            if (IsNHMShutdownNeeded())
            {
                await GracefulShutdown();
            }
        }
        private static async Task GracefulShutdown()
        {
            try
            {
                Logger.Info(Tag, "Installer/uninstaller is running, attempting NHM shutdown");
                await ApplicationStateManager.BeforeExit();
                ApplicationStateManager.ExecuteApplicationExit();
                Logger.Info(Tag, "NHM closed successfully due to installer/uninstaller.");
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, ex.Message);
            }
        }
        private static bool IsNHMShutdownNeeded_2()
        {
            try
            {
                var processes = Process.GetProcesses();
                return HasInstallerOrUpdaterRunning(processes) || HasUninstallerRunning(processes);
            }
            catch (Exception ex) 
            {
                Logger.Error(Tag, $"IsNHMShutdownNeeded {ex.Message}");
            }
            return false;
        }

        private static bool IsNHMShutdownNeeded()
        {
            //Mutex.TryOpenExisting(string name, MutexRights rights, out Mutex result)
            var ok = Mutex.TryOpenExisting(APP_GUID.GUID, System.Security.AccessControl.MutexRights.ReadPermissions, out Mutex res);
            Logger.Info(Tag, $"TryOpenExisting(mutex) result: {ok}");
            return ok;
            //check if mutex is locked
            //if yes then shutdown
        }

        private static bool HasInstallerOrUpdaterRunning(params Process[] processes)
        {
            var processListInstallUpdate = processes
                .Where(proc => proc.ProcessName.ToLower().Contains("nhm_windows"))
                .Where(proc => proc.MainWindowTitle.ToLower().Contains("setup"));
            Logger.Info(Tag, $"HasInstallerOrUpdaterRunning found number of processes: {processListInstallUpdate.Count()}");
            return processListInstallUpdate.Any();
        }
        private static bool HasUninstallerRunning(params Process[] processes)
        {
            var processListUninstall = processes
                .Where(proc => proc.ProcessName.ToLower().Contains("un_a"))
                .Where(proc => proc.MainWindowTitle.ToLower().Contains("uninstall"));
            Logger.Info(Tag, $"HasUninstallerRunning found number of processes: {processListUninstall.Count()}");
            return processListUninstall.Any();
        }
    }
}
