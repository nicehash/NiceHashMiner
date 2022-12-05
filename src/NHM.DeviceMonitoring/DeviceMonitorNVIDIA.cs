﻿using NHM.Common;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.DeviceMonitoring.TDP;
using System;
using System.Threading;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorNVIDIA : DeviceMonitor, IFanSpeedRPM, IGetFanSpeedPercentage, ILoad, IPowerUsage, ITemp, ITDP, IMemoryTimings, IMemControllerLoad, ISpecialTemps
    {
        private const int RET_OK = 0;
        public static object _lock = new object();

        public int BusID { get; private set; }

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(0.5);
        private string LogTag => $"DeviceMonitorNVIDIA-uuid({UUID})-busid({BusID})";

        private static Timer DriverAliveCheckTimer;
        private static int FailCounter = 0;
        private static int CurrentTimeout = 10000;

        internal DeviceMonitorNVIDIA(string uuid, int busID)
        {
            UUID = uuid;
            BusID = busID;
        }

        public static void Init()
        {
            using var tryLock = new TryLock(_lock);
            DriverAliveCheckTimer = new Timer(CheckDriverLife, null, 10000, 10000);
        }

        private static void RestartDrivers()
        {
            NVIDIA_MON.nhm_nvidia_deinit();
            NVIDIA_MON.nhm_nvidia_init();
        }

        private static void CheckDriverLife(object objectInfo)
        {
            using var tryLock = new TryLock(_lock);
            if (!NVIDIA_MON.nhm_nvidia_is_nvapi_alive() || !NVIDIA_MON.nhm_nvidia_is_nvml_alive())
            {
                FailCounter++;
                RestartDrivers();
                if (FailCounter == 20)
                {
                    DriverAliveCheckTimer.Change(0, 60000);
                    CurrentTimeout = 60000;
                }
                else if (FailCounter == 30)
                {
                    DriverAliveCheckTimer.Change(0, 3600000);
                    CurrentTimeout = 3600000;
                }
            }
            else
            {
                FailCounter = 0;
                if (CurrentTimeout != 10000)
                {
                    DriverAliveCheckTimer.Change(0, 10000);
                }
            }
        }

        public float Load
        {
            get
            {
                int load_perc = 0;
                int ok = NVIDIA_MON.nhm_nvidia_device_get_load_percentage(BusID, ref load_perc);
                if (ok == RET_OK) return load_perc;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_load_percentage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public float Temp
        {
            get
            {
                ulong temperature = 0;
                int ok = NVIDIA_MON.nhm_nvidia_device_get_temperature(BusID, ref temperature);
                if (ok == RET_OK) return temperature;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_temperature failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        (int status, int percentage) IGetFanSpeedPercentage.GetFanSpeedPercentage()
        {
            int percentage = 0;
            int ok = NVIDIA_MON.nhm_nvidia_device_get_fan_speed_percentage(BusID, ref percentage);
            if (ok != RET_OK) Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed with error code {ok}", _delayedLogging);
            return (ok, percentage);
        }

        public int FanSpeedRPM
        {
            get
            {
                int rpm = 0;
                int ok = NVIDIA_MON.nhm_nvidia_device_get_fan_speed_rpm(BusID, ref rpm);
                if (ok == RET_OK) return rpm;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_fan_speed_rpm failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public double PowerUsage
        {
            get
            {
                int power_usage = 0;
                int ok = NVIDIA_MON.nhm_nvidia_device_get_power_usage(BusID, ref power_usage);
                if (ok == RET_OK) return power_usage;
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_power_usage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }


        public bool SetFanSpeedPercentage(int percentage)
        {
            int ok = NVIDIA_MON.nhm_nvidia_device_set_fan_speed_percentage(BusID, percentage);
            if (ok != RET_OK)
            {
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_set_fan_speed_percentage failed with error code {ok}", _delayedLogging);
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
                int ok = NVIDIA_MON.nhm_nvidia_device_get_tdp(BusID, ref tdpRaw);
                if (ok != RET_OK)
                {
                    Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_tdp failed with error code {ok}", _delayedLogging);
                    return -1;
                }
                var tdpPerc = (double)tdpRaw / 100;
                return tdpPerc; // 0.0d - 1.0d
            }
        }

        private static double? PowerLevelToTDPPercentage(TDPSimpleType level) =>
            level switch
            {
                TDPSimpleType.LOW => 0.6d, // 60%
                TDPSimpleType.MEDIUM => 0.8d, // 80%
                TDPSimpleType.HIGH => 1.0d, // 100%
                _ => null,
            };
        
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
            var execRet = NVIDIA_MON.nhm_nvidia_device_set_tdp(BusID, (int)(percentage*100));
            if (execRet == RET_OK) TDPSimple = level;
            Logger.Info(LogTag, $"SetTDPSimple {execRet}.");
            return execRet == RET_OK;
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
            var execRet = NVIDIA_MON.nhm_nvidia_device_set_tdp(BusID, (int)percentage*100);
            Logger.Info(LogTag, $"SetTDPPercentage {execRet}.");
            return execRet == RET_OK;
        }

        #endregion ITDP
        public int SetMemoryTimings(string mt)
        {
            return NVIDIA_MON.nhm_nvidia_device_set_memory_timings(BusID, mt);
        }
        public int ResetMemoryTimings()
        {
            return NVIDIA_MON.nhm_nvidia_device_reset_memory_timings(BusID);
        }
        private (int vramTemp, int hotspotTemp) GetSpecialTemperatures()
        {
            int vramT = 0;
            int hotspotT = 0;
            int ok = NVIDIA_MON.nhm_nvidia_device_get_special_temperatures(BusID, ref hotspotT, ref vramT);
            if (ok == RET_OK) return (vramT, hotspotT);
            Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_special_temperatures failed with error code {ok}", _delayedLogging);
            return (-1, -1);
        }
        public int HotspotTemp
        {
            get
            {
                return GetSpecialTemperatures().hotspotTemp;
            }
        }
        public int VramTemp
        {
            get
            {
                return GetSpecialTemperatures().vramTemp;
            }
        }
        public int MemoryControllerLoad
        {
            get
            {
                int memCtrlLoad = 0;
                int ok = NVIDIA_MON.nhm_nvidia_device_get_memory_controller_load(BusID, ref memCtrlLoad);
                if (ok == RET_OK) return (memCtrlLoad);
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_memory_controller_load failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }
        public void PrintMemoryTimings()
        {
            NVIDIA_MON.nhm_nvidia_device_print_memory_timings(BusID);
        }
    }
}
