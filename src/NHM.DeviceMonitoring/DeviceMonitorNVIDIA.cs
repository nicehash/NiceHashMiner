using NHM.Common;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.DeviceMonitoring.TDP;
using System;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorNVIDIA : DeviceMonitor, IFanSpeedRPM, ILoad, IPowerUsage, ITemp, ITDP
    {
        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(0.5);

        public int BusID { get; private set; }

        private readonly DeviceMonitorWatchdog _deviceMonitorWatchdog;

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
                int loadPerc = 0;
                var returnCode = NVIDIA_MON.nhm_nvidia_device_get_load_perc(BusID, ref loadPerc);
                if (returnCode == 0) return loadPerc;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_load_perc failed with CODE:{returnCode}", _delayedLogging);
                return loadPerc;
            }
        }

        public float Temp
        {
            get
            {
                ulong tempPerc = 0;
                var returnCode = NVIDIA_MON.nhm_nvidia_device_get_temperature(BusID, ref tempPerc);
                if (returnCode == 0) return tempPerc;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_temperature failed with CODE:{returnCode}", _delayedLogging);
                return tempPerc;
            }
        }

        public int FanSpeedRPM
        {
            get
            {
                int fanSpeed = 0;
                var returnCode = NVIDIA_MON.nhm_nvidia_device_get_fan_speed_rpm(BusID, ref fanSpeed);
                if (returnCode == 0) return fanSpeed;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed with CODE:{returnCode}", _delayedLogging);
                return fanSpeed;
            }
        }

        public double PowerUsage
        {
            get
            {
                float powerUsage = 0;
                var returnCode = NVIDIA_MON.nhm_nvidia_device_get_power_usage(UUID, ref powerUsage);
                if (returnCode == 0) return powerUsage;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_power_usage failed with CODE:{returnCode}", _delayedLogging);
                return powerUsage;
            }
        }

        public bool SetFanSpeedPercentage(int percentage)
        {
            var returnCode = NVIDIA_MON.nhm_nvidia_device_set_fan_speed_percentage(BusID, percentage);
            if (returnCode == 0) return true;
            Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_set_fan_speed_percentage failed with CODE:{returnCode}", _delayedLogging);
            return false;
        }

        #region ITDP
        public TDPSettingType SettingType { get; set;  } = TDPSettingType.SIMPLE;

        public TDPSimpleType TDPSimple { get; private set; } = TDPSimpleType.HIGH;

        public double TDPPercentage
        {
            get
            {
                int tdp = 0;
                var returnCode = NVIDIA_MON.nhm_nvidia_device_get_tdp(BusID, ref tdp);
                if (returnCode == 0) return tdp / 100.0;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_tdp failed with CODE:{returnCode}", _delayedLogging);
                return tdp;
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

            var returnCode = NVIDIA_MON.nhm_nvidia_device_set_tdp(UUID, (uint)(percentage * 100));
            if (returnCode == 0) return true;
            Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_set_tdp failed with CODE:{returnCode}", _delayedLogging);
            return false;
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
            var returnCode = NVIDIA_MON.nhm_nvidia_device_set_tdp(UUID, (uint)(percentage * 100));
            if (returnCode == 0) return true;
            Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_set_tdp failed with CODE:{returnCode}", _delayedLogging);
            return false;

        }

        #endregion ITDP
    }
}
