using NHM.Common;
using NHM.DeviceDetection;
using NHMCore.Configs;
using NHMCore.Notifications;
using NHMCore.Utils;
using System;
using System.Diagnostics;
using System.Timers;

namespace NHMCore
{
    public static partial class ApplicationStateManager
    {
        internal class AppTimer
        {
            private readonly object _lock = new object();
            private readonly ElapsedEventHandler _elapsedEventHandler;
            private readonly double _interval;
            private Timer _timer;

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
                    _timer = new Timer();
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


        #region ComputeDevicesCheck Lost GPU check
        private static AppTimer _cudaDeviceCheckerTimer;

        private static void StartComputeDevicesCheckTimer()
        {
            if (_cudaDeviceCheckerTimer?.IsActive ?? false) return;
            _cudaDeviceCheckerTimer = new AppTimer(async (object sender, ElapsedEventArgs e) =>
            {
                if (!GlobalDeviceSettings.Instance.CheckForMissingGPUs)
                    return;
                // this function checks if count of CUDA devices is same as it was on application start, reason for that is
                // because of some reason (especially when algo switching occure) CUDA devices are dissapiring from system
                // creating tons of problems e.g. miners stop mining, lower rig hashrate etc.
                var hasMissingGPUs = await DeviceDetection.CheckIfMissingGPUs();
                if (!hasMissingGPUs) return;
                if (GlobalDeviceSettings.Instance.RestartMachineOnLostGPU)
                {
                    Logger.Info("ApplicationStateManager.Timers", $"Detected missing GPUs will execute 'OnGPUsLost.bat'");
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
                else
                {
                    Logger.Info("ApplicationStateManager.Timers", $"Detected missing GPUs");
                    AvailableNotifications.CreateMissingGPUsInfo();
                }
            },
            5 * 60 * 1000); // check every 5 minutes
            _cudaDeviceCheckerTimer.Start();
        }

        private static void StopComputeDevicesCheckTimer()
        {
            _cudaDeviceCheckerTimer?.Stop();
        }
        #endregion ComputeDevicesCheck Lost GPU check

        #region InternetCheck timer
        private static AppTimer _internetCheckTimer;

        public static event EventHandler<bool> OnInternetCheck;

        public static void StartInternetCheckTimer()
        {
            if (IdleMiningSettings.Instance.IdleWhenNoInternetAccess)
            {
                OnInternetCheck?.Invoke(null, Helpers.IsConnectedToInternet());
            }

            if (_internetCheckTimer?.IsActive ?? false) return;
            _internetCheckTimer = new AppTimer((object sender, ElapsedEventArgs e) =>
            {
                if (IdleMiningSettings.Instance.IdleWhenNoInternetAccess)
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
