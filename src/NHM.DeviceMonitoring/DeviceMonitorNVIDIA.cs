using NHM.Common;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.DeviceMonitoring.TDP;
using System;
using System.Linq;

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
                var load_perc = NVIDIA_MON.nhm_nvidia_device_get_load_perc(BusID);
                if (load_perc >= 0) return load_perc;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_load_perc failed", _delayedLogging);
                return -1;
            }
        }

        public float Temp
        {
            get
            {    
                var temp = NVIDIA_MON.nhm_nvidia_device_get_temperature(BusID);
                if (temp < uint.MaxValue) return temp;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_temperature failed", _delayedLogging);
                return -1;
            }
        }

        public int FanSpeedRPM
        {
            get
            {
                //var fan_speed = NVIDIA_MON.nhm_nvidia_device_get_fan_speed_rpm(BusID);
                var fan_speed = NVIDIA_MON.nhm_nvidia_device_get_fan_speed_percentage(UUID);
                if (fan_speed < 0) Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed", _delayedLogging);
                return fan_speed;
            }
        }

        public double PowerUsage
        {
            get
            {
                var power_usage = NVIDIA_MON.nhm_nvidia_device_get_power_usage(UUID);
                if (power_usage >= 0) return power_usage;

                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_power_usage failed", _delayedLogging);
                return -1;
            }
        }

        private bool NvmlSetTDP(double percentage)
        {
            bool ret = NVIDIA_MON.nhm_nvidia_device_set_tdp(UUID, (uint)(percentage * 100));
            return ret;
        }

        #region ITDP
        public TDPSettingType SettingType { get; set;  } = TDPSettingType.SIMPLE;

        public TDPSimpleType TDPSimple { get; private set; } = TDPSimpleType.HIGH;

        public double TDPPercentage
        {
            get
            {
                int tdpRet = NVIDIA_MON.nhm_nvidia_device_get_tdp(BusID);
                if (tdpRet < 0) return -1.0;

                return tdpRet / 100.0;
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
            var execRet = NvmlSetTDP(percentage.Value);
            if (execRet) TDPSimple = level;
            Logger.Info(LogTag, $"SetTDPSimple {execRet}.");
            return execRet;
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
            var execRet = NvmlSetTDP(percentage);
            Logger.Info(LogTag, $"SetTDPPercentage {execRet}.");
            return execRet;
        }

        #endregion ITDP
    }
}
