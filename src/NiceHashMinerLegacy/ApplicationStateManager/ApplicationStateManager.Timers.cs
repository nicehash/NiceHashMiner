using NHM.DeviceDetection;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NiceHashMinerLegacy.Common;
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
                await MiningManager.MinerStatsCheck();
            };
            _minerStatsCheck.Interval = ConfigManager.GeneralConfig.MinerAPIQueryInterval * 1000;
            _minerStatsCheck.Start();
        }

        private static void StopMinerStatsCheckTimer()
        {
            _minerStatsCheck?.Stop();
            _minerStatsCheck?.Dispose();
            _minerStatsCheck = null;
        }
        #endregion MinerStatsCheck


        #region ComputeDevicesCheck Lost GPU check
        private static SystemTimer _cudaDeviceCheckerTimer;

        private static void StartComputeDevicesCheckTimer()
        {
            if (!ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost)
                return;

            _cudaDeviceCheckerTimer = new SystemTimer();
            _cudaDeviceCheckerTimer.Elapsed += async (object sender, ElapsedEventArgs e) => {
                // this function checks if count of CUDA devices is same as it was on application start, reason for that is
                // because of some reason (especially when algo switching occure) CUDA devices are dissapiring from system
                // creating tons of problems e.g. miners stop mining, lower rig hashrate etc.
                var nvidiaCount = await DeviceDetection.CUDADevicesNumCheck();
                var isDevicesCountMistmatch = nvidiaCount != AvailableDevices.AvailNVGpus;
                if (isDevicesCountMistmatch)
                {
                    try
                    {
                        var onGpusLost = new ProcessStartInfo
                        {
                            FileName = Paths.RootPath("OnGPUsLost.bat"),
                            WindowStyle = ProcessWindowStyle.Minimized
                        };
                        using (var p = Process.Start(onGpusLost))
                        {
                            //p.WaitForExit(10 * 1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("ApplicationStateManager.Timers", $"OnGPUsMismatch.bat error: {ex.Message}");
                    }
                }
            };
            _cudaDeviceCheckerTimer.Interval = 60 * 1000;
            _cudaDeviceCheckerTimer.Start();
        }

        private static void StopComputeDevicesCheckTimer()
        {
            _cudaDeviceCheckerTimer?.Stop();
            _cudaDeviceCheckerTimer?.Dispose();
            _cudaDeviceCheckerTimer = null;
        }
        #endregion ComputeDevicesCheck Lost GPU check

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
            _preventSleepTimer?.Dispose();
            _preventSleepTimer = null;
        }
        #endregion PreventSystemSleepTimer

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
            _refreshDeviceListViewTimer?.Dispose();
            _refreshDeviceListViewTimer = null;
        }
        #endregion RefreshDeviceListView timer

        #region InternetCheck timer
        private static SystemTimer _internetCheckTimer;

        public static event EventHandler<bool> OnInternetCheck;

        public static void StartInternetCheckTimer()
        {
            _internetCheckTimer = new SystemTimer();
            _internetCheckTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                if (ConfigManager.GeneralConfig.IdleWhenNoInternetAccess)
                {
                    OnInternetCheck?.Invoke(null, Helpers.IsConnectedToInternet());
                }
            };
            _internetCheckTimer.Interval = 1000 * 60;
            _internetCheckTimer.Start();
        }

        public static void StopInternetCheckTimer()
        {
            _internetCheckTimer?.Stop();
            _internetCheckTimer?.Dispose();
            _internetCheckTimer = null;
        }

        #endregion InternetCheck timer
    }
}
