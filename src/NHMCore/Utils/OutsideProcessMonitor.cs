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
    public class OutsideProcessMonitor : IDisposable
    {
        private Timer MonitorTimer;
        private bool _isDisposed = false;

        public OutsideProcessMonitor()
        {
            var startTimeSpan = TimeSpan.FromSeconds(1);
            var periodTimeSpan = TimeSpan.FromSeconds(5);
            MonitorTimer = new System.Threading.Timer((e) =>
            {
                MonitorIteration();
            }, null, startTimeSpan, periodTimeSpan);
        }
        private Task MonitorIteration()
        {
            var processlist = Process.GetProcesses()
                .Where(proc => proc.ProcessName.ToLower().Contains("nhm_windows")//installer
                    || proc.ProcessName.ToLower().Contains("un_a"))//uninstaller
                .Where(proc => (proc.MainWindowTitle.ToLower().Contains("setup") 
                    || proc.MainWindowTitle.ToLower().Contains("uninstall")));
            if (processlist.Any())
            {
                try
                {
                    Logger.Info("OutsideProcessMonitor", "Installer/uninstaller is running, attempting NHM shutdown");
                    _ = ApplicationStateManager.BeforeExit();
                    ApplicationStateManager.ExecuteApplicationExit();
                    Logger.Info("OutsideProcessMonitor", "NHM closed successfully due to installer/uninstaller.");
                }
                catch (Exception ex)
                {
                    Logger.Error("OutsideProcessMonitor", ex.Message);
                }
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                MonitorTimer.Dispose();
            }
        }
    }
}
