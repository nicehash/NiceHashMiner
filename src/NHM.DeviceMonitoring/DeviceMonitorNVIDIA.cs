using NHM.Common;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.DeviceMonitoring.TDP;
using System;
using System.Linq;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorNVIDIA : DeviceMonitor, IFanSpeedRPM, IGetFanSpeedPercentage, ILoad, IPowerUsage, ITemp, ITDP
    {
        private object _lock = new object();

        public int BusID { get; private set; }

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(0.5);
        private static readonly int SecondsUntilDriverRestart = 10;
        private int LastSuccessfullDriverInteraction = 0;

        private string LogTag => $"DeviceMonitorNVIDIA-uuid({UUID})-busid({BusID})";

        internal DeviceMonitorNVIDIA(string uuid, int busID)
        {
            UUID = uuid;
            BusID = busID;
        }

        private bool DriverWorkingOrRestart()
        {
            if (LastSuccessfullDriverInteraction == 0) return false;
            var now = (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var timeToLastSuccess = now - LastSuccessfullDriverInteraction;
            if(timeToLastSuccess > SecondsUntilDriverRestart)
            {
                NVIDIA_ODN.nhm_nvidia_restart_driver();
                return true;
            }
            return false;
        }

        private void LogSuccessfullInteraction()
        {
            LastSuccessfullDriverInteraction = (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public float Load
        {
            get
            {
                var crashed = DriverWorkingOrRestart();
                int load_perc = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_load_percentage(BusID, ref load_perc);
                if (ok == 0 && !crashed)
                {
                    LogSuccessfullInteraction();
                    return load_perc;
                }
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_load_percentage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public float Temp
        {
            get
            {
                var crashed = DriverWorkingOrRestart();
                ulong temperature = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_temperature(BusID, ref temperature);
                if (ok == 0 && !crashed)
                {
                    LogSuccessfullInteraction();
                    return temperature;
                }
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_temperature failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        (int status, int percentage) IGetFanSpeedPercentage.GetFanSpeedPercentage()
        {
            var crashed = DriverWorkingOrRestart();
            int percentage = 0;
            int ok = NVIDIA_ODN.nhm_nvidia_device_get_fan_speed_percentage(BusID, ref percentage);
            if (ok != 0 && crashed)
            {
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed with error code {ok}", _delayedLogging);
            }
            else LogSuccessfullInteraction();
            return (ok, percentage);
        }

        public int FanSpeedRPM
        {
            get
            {
                var crashed = DriverWorkingOrRestart();
                int rpm = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_fan_speed_rpm(BusID, ref rpm);
                if (ok == 0 && !crashed)
                {
                    LogSuccessfullInteraction();
                    return rpm;
                }
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public double PowerUsage
        {
            get
            {
                var crashed = DriverWorkingOrRestart();
                int power_usage = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_power_usage(BusID, ref power_usage);
                if (ok == 0 && !crashed)
                {
                    LogSuccessfullInteraction();
                    return power_usage;
                }
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_power_usage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }


        public bool SetFanSpeedPercentage(int percentage)
        {
            var crashed = DriverWorkingOrRestart();
            int ok = NVIDIA_ODN.nhm_nvidia_device_set_fan_speed_percentage(BusID, percentage);
            if (ok != 0 && crashed)
            {
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_set_fan_speed_rpm failed with error code {ok}", _delayedLogging);
                return false;
            }
            LogSuccessfullInteraction();
            return true;
        }


        #region ITDP
        public TDPSettingType SettingType { get; set; } = TDPSettingType.SIMPLE;

        public TDPSimpleType TDPSimple { get; private set; } = TDPSimpleType.HIGH;

        public double TDPPercentage
        {
            get
            {
                var crashed = DriverWorkingOrRestart();
                int tdpRaw = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_tdp(BusID, ref tdpRaw);
                if (ok != 0)
                {
                    Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_tdp failed with error code {ok}", _delayedLogging);
                    return -1;
                }
                uint min = 0, max = 0, defaultValue = 0;
                int ok2 = NVIDIA_ODN.nhm_nvidia_device_get_tdp_min_max_default(BusID, ref min, ref max, ref defaultValue);
                if (ok2 != 0)
                {
                    Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_tdp_ranges failed with error code {ok}", _delayedLogging);
                    return -1;
                }
                // We limit 100% to the default as max
                var tdpPerc = RangeCalculator.CalculatePercentage(tdpRaw, min, defaultValue);
                LogSuccessfullInteraction();
                return tdpPerc; // 0.0d - 1.0d
            }
        }

        private static double? PowerLevelToTDPPercentage(TDPSimpleType level)
        {
            switch (level)
            {
                case TDPSimpleType.LOW: return 0.6d; // 60%
                case TDPSimpleType.MEDIUM: return 0.8d; // 80%
                case TDPSimpleType.HIGH: return 1.0d; // 100%
            }
            return null;
        }
        public bool SetTDPSimple(TDPSimpleType level)
        {
            if (DeviceMonitorManager.DisableDevicePowerModeSettings)
            {
                Logger.InfoDelayed(LogTag, $"SetTDPSimple Disabled DeviceMonitorManager.DisableDevicePowerModeSettings==true", TimeSpan.FromSeconds(30));
                return false;
            }
            var percentage = PowerLevelToTDPPercentage(level);
            if (!percentage.HasValue)
            {
                Logger.Error(LogTag, $"SetTDPSimple unkown PowerLevel {level}. Defaulting to {TDPSimpleType.HIGH}");
                level = TDPSimpleType.HIGH;
                percentage = PowerLevelToTDPPercentage(level);
            }
            Logger.Info(LogTag, $"SetTDPSimple setting PowerLevel to {level}.");
            var execRet = NVIDIA_ODN.nhm_nvidia_device_set_tdp(BusID, (int)percentage);
            if (execRet < 0) TDPSimple = level;
            Logger.Info(LogTag, $"SetTDPSimple {execRet}.");
            return execRet < 0;
        }

        public bool SetTDPPercentage(double percentage)
        {
            if (DeviceMonitorManager.DisableDevicePowerModeSettings)
            {
                Logger.InfoDelayed(LogTag, $"SetTDPPercentage Disabled DeviceMonitorManager.DisableDevicePowerModeSettings==true", TimeSpan.FromSeconds(30));
                return false;
            }
            if (percentage < 0.0d)
            {
                Logger.Error(LogTag, $"SetTDPPercentage {percentage} out of bounds. Setting to 0.0d");
                percentage = 0.0d;
            }

            Logger.Info(LogTag, $"SetTDPPercentage setting to {percentage}.");
            var execRet = NVIDIA_ODN.nhm_nvidia_device_set_tdp(BusID, (int)percentage);
            Logger.Info(LogTag, $"SetTDPPercentage {execRet}.");
            return execRet < 0;
        }

        #endregion ITDP
    }
}
