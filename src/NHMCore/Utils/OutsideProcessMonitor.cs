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
        private static Task CheckProcessTask { get; set; } = null;
        private static MonitorState RunState { get; set; } = MonitorState.None;
        private enum MonitorState
        {
            None,
            ElevatedMode,
            FallbackMode,
        }
        public static async Task Init(CancellationToken stop)
        {
            await ExecuteGracefulShutdownIfNeeded(); // initial check if installer running before NHM
            if (Helpers.IsElevated && InitAndStartProcessWatcher())
            {
                RunState = MonitorState.ElevatedMode;
            }
            else
            {
                Logger.Info(Tag, "Running with normal rights");
                CheckProcessTask = Task.Run(async () =>
                {
                    await ProcessMonitorLoop(stop);
                });
                RunState = MonitorState.FallbackMode;
            }
            stop.Register(Deinit);
        }
        private static void Deinit()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if (RunState == MonitorState.ElevatedMode && ProcessInitWatch != null)
                {
                    ProcessInitWatch.Stop();
                    ProcessInitWatch.Dispose();
                }
                if (CheckProcessTask != null)
                {
                    CheckProcessTask.Wait();
                    CheckProcessTask.Dispose();
                }
            }
        }
        private static bool InitAndStartProcessWatcher()
        {
            try
            {
                ProcessInitWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                ProcessInitWatch.EventArrived += async (s, e) => await ExecuteGracefulShutdownIfNeeded();
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
                while (!stop.IsCancellationRequested)
                {
                    await ExecuteGracefulShutdownIfNeeded();
                    await Task.Delay(CheckIntervalInSeconds * 1000, stop);
                }
            }
            catch {}
        }
        private static async Task ExecuteGracefulShutdownIfNeeded()
        {
            try
            {
                if (IsNHMShutdownNeeded()) await GracefulShutdown();
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, $"ExecuteGracefulShutdownIfNeeded {ex.Message}");
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
            finally
            {
                Deinit();
            }
        }
        private static bool IsNHMShutdownNeeded()
        {
            var processes = Process.GetProcesses();
            return HasInstallerOrUpdaterRunning(processes) || HasUninstallerRunning(processes);
        }
        private static bool HasInstallerOrUpdaterRunning(Process[] processes)
        {
            var processListInstallUpdate = processes
                .Where(proc => proc.ProcessName.ToLower().Contains("nhm_windows"))
                .Where(proc => proc.MainWindowTitle.ToLower().Contains("setup"));
            return processListInstallUpdate.Any();
        }
        private static bool HasUninstallerRunning(Process[] processes)
        {
            var processListUninstall = processes
                .Where(proc => proc.ProcessName.ToLower().Contains("un_a"))
                .Where(proc => proc.MainWindowTitle.ToLower().Contains("uninstall"));
            return processListUninstall.Any();
        }
    }
}
