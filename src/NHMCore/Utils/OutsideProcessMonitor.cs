using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using NHM.Common;
using System.Threading;
using System.Diagnostics;
using System.Security.AccessControl;

namespace NHMCore.Utils
{
    public static class OutsideProcessMonitor
    {
        private static int CheckIntervalInSeconds = 2;
        private static string Tag = "OutsideProcessMonitor";
        public static void Init(CancellationToken stop)
        {
            Logger.Info(Tag, "Initializing");
            _ = Task.Run(async () =>
            {
                await ProcessMonitorLoop(stop);
            });
            Logger.Info(Tag, "Closed outside process monitor loop");
        }
        private static async Task ProcessMonitorLoop(CancellationToken stop)
        {
            try
            {
                while (!IsNHMShutdownNeeded()) await Task.Delay(CheckIntervalInSeconds * 1000, stop);
            }
            catch { }
            finally
            {
                if (IsNHMShutdownNeeded()) await GracefulShutdown();
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
                Logger.Error(Tag, $"GracefulShutdown: {ex.Message}");
            }
        }
        private static bool IsNHMShutdownNeeded()
        {
            try
            {
                return Mutex.TryOpenExisting(APP_GUID.GUID, MutexRights.ReadPermissions, out var _);
            }
            catch (Exception ex)
            {
                Logger.Error(Tag, $"IsNHMShutdownNeeded: {ex.Message}");
            }
            return false;
        }
    }
}
