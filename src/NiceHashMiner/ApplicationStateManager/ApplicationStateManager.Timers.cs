using NHM.DeviceDetection;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners;
using NHM.Common;
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
        internal class AppTimer
        {
            private readonly object _lock = new object();
            private readonly ElapsedEventHandler _elapsedEventHandler;
            private readonly double _interval;
            private SystemTimer _timer;

            internal AppTimer(ElapsedEventHandler elapsedEventHandler, double interval)
            {
                _elapsedEventHandler = elapsedEventHandler;
                _interval = interval;
            }

            public bool IsActive
            {
                get
                {
                    lock (_lock)
                    {
                        var isActive = _timer != null;
                        return isActive;
                    }
                }
            }

            public void Start()
            {
                lock (_lock)
                {
                    if (_timer != null) return;
                    _timer = new SystemTimer();
                    _timer.Elapsed += _elapsedEventHandler;
                    _timer.Interval = _interval;
                    _timer.Start();
                }
            }

            public void Stop()
            {
                lock (_lock)
                {
                    _timer?.Stop();
                    _timer?.Dispose();
                    _timer = null;
                }
            }
        }

        #region MinerStatsCheck
        private static AppTimer _minerStatsCheck;

        private static void StartMinerStatsCheckTimer()
        {
            if (_minerStatsCheck?.IsActive ?? false) return;
            _minerStatsCheck = new AppTimer(async (object sender, ElapsedEventArgs e) =>
            {
                await MiningManager.MinerStatsCheck();
            },
            ConfigManager.GeneralConfig.MinerAPIQueryInterval * 1000);
            _minerStatsCheck.Start();
        }

        private static void StopMinerStatsCheckTimer()
        {
            _minerStatsCheck?.Stop();
        }
        #endregion MinerStatsCheck


        #region ComputeDevicesCheck Lost GPU check
        private static AppTimer _cudaDeviceCheckerTimer;

        private static void StartComputeDevicesCheckTimer()
        {
            if (_cudaDeviceCheckerTimer?.IsActive ?? false) return;
            _cudaDeviceCheckerTimer = new AppTimer(async (object sender, ElapsedEventArgs e) => {
                if (!ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost)
                    return;
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
            },
            60 * 1000);
            _cudaDeviceCheckerTimer.Start();
        }

        private static void StopComputeDevicesCheckTimer()
        {
            _cudaDeviceCheckerTimer?.Stop();
        }
        #endregion ComputeDevicesCheck Lost GPU check

        #region PreventSystemSleepTimer
        private static AppTimer _preventSleepTimer;

        private static void StartPreventSleepTimer()
        {
            if (_preventSleepTimer?.IsActive ?? false) return;
            // sleep time setting is minimal 1 minute
            _preventSleepTimer = new AppTimer((s, e) => {
                PInvoke.PInvokeHelpers.PreventSleep();
            },
            20 * 1000);// leave this interval, it works
            _preventSleepTimer.Start();
        }

        // restroe/enable sleep
        private static void StopPreventSleepTimer()
        {
            _preventSleepTimer?.Stop();
        }
        #endregion PreventSystemSleepTimer

        #region RefreshDeviceListView timer
        private static AppTimer _refreshDeviceListViewTimer;

        public static void StartRefreshDeviceListViewTimer()
        {
            if (_refreshDeviceListViewTimer?.IsActive ?? false) return;
            _refreshDeviceListViewTimer = new AppTimer((object sender, ElapsedEventArgs e) => {
                RefreshDeviceListView?.Invoke(sender, EventArgs.Empty);
            },
            2000);
            _refreshDeviceListViewTimer.Start();
        }
        
        private static void StopRefreshDeviceListViewTimer()
        {
            _refreshDeviceListViewTimer?.Stop();
        }
        #endregion RefreshDeviceListView timer

        #region InternetCheck timer
        private static AppTimer _internetCheckTimer;

        public static event EventHandler<bool> OnInternetCheck;

        public static void StartInternetCheckTimer()
        {
            if (ConfigManager.GeneralConfig.IdleWhenNoInternetAccess)
            {
                OnInternetCheck?.Invoke(null, Helpers.IsConnectedToInternet());
            }

            if (_internetCheckTimer?.IsActive ?? false) return;
            _internetCheckTimer = new AppTimer((object sender, ElapsedEventArgs e) =>
            {
                if (ConfigManager.GeneralConfig.IdleWhenNoInternetAccess)
                {
                    OnInternetCheck?.Invoke(null, Helpers.IsConnectedToInternet());
                }
            },
            1000 * 60);
            _internetCheckTimer.Start();
        }

        public static void StopInternetCheckTimer()
        {
            _internetCheckTimer?.Stop();
        }

        #endregion InternetCheck timer
    }
}
