using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SystemTimer = System.Timers.Timer;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        #region MinerStatsCheck
        private static SystemTimer _minerStatsCheck;

        private static void StartMinerStatsCheckTimer()
        {
            _minerStatsCheck = new SystemTimer();
            _minerStatsCheck.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                await MinersManager.MinerStatsCheck();
            };
            _minerStatsCheck.Interval = ConfigManager.GeneralConfig.MinerAPIQueryInterval * 1000;
            _minerStatsCheck.Start();
        }

        private static void StopMinerStatsCheckTimer()
        {
            _minerStatsCheck?.Stop();
            _minerStatsCheck = null;
        }
        #endregion

        // TODO This is duplicated with CudaDeviceChecker
        #region ComputeDevicesCheck Lost GPU check
        private static CudaDeviceChecker _cudaDeviceChecker;

        private static void StartComputeDevicesCheckTimer()
        {
            if (_cudaDeviceChecker == null)
            {
                _cudaDeviceChecker = new CudaDeviceChecker();
            }
            _cudaDeviceChecker.Start();
        }

        private static void StopComputeDevicesCheckTimer()
        {
            _cudaDeviceChecker?.Stop();
        }
        #endregion

        #region PreventSystemSleepTimer
        private static SystemTimer _preventSleepTimer;

        private static void StartPreventSleepTimer()
        {
            _preventSleepTimer = new SystemTimer();
            _preventSleepTimer.Elapsed += (object sender, ElapsedEventArgs e) => {
                PInvoke.PInvokeHelpers.PreventSleep();
            };
            // sleep time setting is minimal 1 minute
            _preventSleepTimer.Interval = 20 * 1000; // leave this interval, it works
            _preventSleepTimer.Start();
        }

        // restroe/enable sleep
        private static void StopPreventSleepTimer()
        {
            _preventSleepTimer?.Stop();
            _preventSleepTimer = null;
        }
        #endregion

        #region RefreshDeviceListView timer
        private static SystemTimer _refreshDeviceListViewTimer;

        public static void StartRefreshDeviceListViewTimer()
        {
            _refreshDeviceListViewTimer = new SystemTimer();
            _refreshDeviceListViewTimer.Elapsed += (object sender, ElapsedEventArgs e) => {
                RefreshDeviceListView?.Invoke(sender, EventArgs.Empty);
            };
            _refreshDeviceListViewTimer.Interval = 2000;
            _refreshDeviceListViewTimer.Start();
        }
        
        private static void StopRefreshDeviceListViewTimer()
        {
            _refreshDeviceListViewTimer?.Stop();
            _refreshDeviceListViewTimer = null;
        }
#endregion
    }
}
