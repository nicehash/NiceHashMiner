using ManagedCuda.Nvml;
using NHM.Common;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.DeviceMonitoring.TDP;
using NVIDIA.NVAPI;
using System;
using System.Linq;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorNVIDIA : DeviceMonitor, IFanSpeedRPM, IGetFanSpeedPercentage, ILoad, IPowerUsage, ITemp, ITDP
    {
        private object _lock = new object();

        public int BusID { get; private set; }

        private readonly DeviceMonitorWatchdog _deviceMonitorWatchdog;

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(0.5);


        private string LogTag => $"DeviceMonitorNVIDIA-uuid({UUID})-busid({BusID})";

        internal DeviceMonitorNVIDIA(string uuid, int busID, TimeSpan firstMaxTimeoutAfterNvmlRestart)
        {
            UUID = uuid;
            BusID = busID;
            _deviceMonitorWatchdog = new DeviceMonitorWatchdog(firstMaxTimeoutAfterNvmlRestart);
            // recovery backoff attempts
            for (int i = 0; i < 20; i++) _deviceMonitorWatchdog.AppendTimeoutTimeSpan(firstMaxTimeoutAfterNvmlRestart);
            for (int i = 0; i < 20; i++) _deviceMonitorWatchdog.AppendTimeoutTimeSpan(TimeSpan.FromMinutes(1)); // attempt on minute
            for (int i = 0; i < 10; i++) _deviceMonitorWatchdog.AppendTimeoutTimeSpan(TimeSpan.FromHours(1)); // attempt on hour
            for (int i = 0; i < 1; i++) _deviceMonitorWatchdog.AppendTimeoutTimeSpan(TimeSpan.FromDays(1)); // attempt after a day and stop after
        }

        public float Load
        {
            get
            {
                int load_perc = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_load_percentage(BusID, ref load_perc);
                if (ok == 0) return load_perc;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_load_percentage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public float Temp
        {
            get
            {
                ulong temperature = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_temperature(BusID, ref temperature);
                if (ok == 0) return temperature;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_temperature failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        (int status, int percentage) IGetFanSpeedPercentage.GetFanSpeedPercentage()
        {
            int percentage = 0;
            int ok = NVIDIA_ODN.nhm_nvidia_device_get_fan_speed_percentage(BusID, ref percentage);
            if (ok != 0) Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed with error code {ok}", _delayedLogging);
            return (ok, percentage);
        }

        public int FanSpeedRPM
        {
            get
            {
                int rpm = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_fan_speed_rpm(BusID, ref rpm);
                if (ok == 0) return rpm;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public double PowerUsage
        {
            get
            {
                int power_usage = 0;
                int ok = NVIDIA_ODN.nhm_nvidia_device_get_power_usage(BusID, ref power_usage);
                if (ok == 0) return power_usage;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_power_usage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }


        public bool SetFanSpeedPercentage(int percentage)
        {
            int ok = NVIDIA_ODN.nhm_nvidia_device_set_fan_speed_percentage(BusID, percentage);
            if (ok != 0)
            {
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_set_fan_speed_rpm failed with error code {ok}", _delayedLogging);
                return false;
            }
            return true;
        }


        #region ITDP
        public TDPSettingType SettingType { get; set; } = TDPSettingType.SIMPLE;

        public TDPSimpleType TDPSimple { get; private set; } = TDPSimpleType.HIGH;

        public double TDPPercentage
        {
            get
            {
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
