using NHM.Common;
using NHM.DeviceMonitoring.AMD;
using NHM.DeviceMonitoring.Core_clock;
using NHM.DeviceMonitoring.Core_voltage;
using NHM.DeviceMonitoring.INTEL;
using NHM.DeviceMonitoring.Memory_clock;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.DeviceMonitoring.TDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorINTEL : DeviceMonitor, IFanSpeedRPM, ILoad, IPowerUsage, ITemp, ISpecialTemps, ITDP, ICoreClock, IMemoryClock, ICoreClockSet, ICoreClockRange, ICoreVoltageRange, ICoreVoltage, ICoreVoltageSet, ITDPLimits
    {
        public int BusID { get; private set; }
        private const int RET_OK = 0;

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(0.5);

        private string LogTag => $"DeviceMonitorAMD-uuid({UUID})-busid({BusID})";

        internal DeviceMonitorINTEL(string uuid, int busID)
        {
            UUID = uuid;
            BusID = busID;
        }

        public int FanSpeedRPM
        {
            get
            {
                int rpm = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_fan_speed_rpm(BusID, ref rpm);
                if (ok == 0) return rpm;
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_fan_speed_rpm failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public float Temp
        {
            get
            {
                int temperature = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_temperature(BusID, ref temperature);
                if (ok == 0) return temperature;
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_temperature failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public float Load
        {
            get
            {
                int load_perc = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_load_percentage(BusID, ref load_perc);
                if (ok == 0) return load_perc;
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_load_percentage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public double PowerUsage
        {
            get
            {
                int power_usage = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_power_usage(BusID, ref power_usage);
                if (ok == 0) return power_usage;
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_power_usage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public int VramTemp
        {
            get
            {
                int vramT = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_vram_temperature(BusID, ref vramT);
                if (ok == 0) return vramT;
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_vram_temperature failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public int HotspotTemp
        {
            get
            {
                return -1;
            }
        }

        public int CoreClock
        {
            get
            {
                int coreClock = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_core_clocks(BusID, ref coreClock);
                if (ok == 0) return coreClock;
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_core_clocks failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public int MemoryClock
        {
            get
            {
                int memClock = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_memory_clocks(BusID, ref memClock);
                if (ok == 0) return memClock;
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_memory_clocks failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public int CoreVoltage
        {
            get
            {
                int coreVoltage = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_core_voltage(BusID, ref coreVoltage);
                if (ok == RET_OK) return coreVoltage;
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_core_voltage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public TDPSettingType SettingType { get; set; } = TDPSettingType.SIMPLE;

        public double TDPPercentage
        {
            get
            {
                int tdpRaw = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_power_limit(BusID, ref tdpRaw);
                if (ok != 0)
                {
                    Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_power_limit failed with error code {ok}", _delayedLogging);
                    return -1;
                }
                int min = 0, max = 0, defaultValue = 0;
                int ok2 = INTEL_IGCL.nhm_intel_device_get_power_limit_min_max_default(BusID, ref min, ref max, ref defaultValue);
                if (ok2 != 0)
                {
                    Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_power_limit_min_max_default failed with error code {ok}", _delayedLogging);
                    return -1;
                }
                // We limit 100% to the default as max
                var tdpPerc = RangeCalculator.CalculatePercentage(tdpRaw, min, defaultValue);
                return tdpPerc; // 0.0d - 1.0d
            }
        }

        public TDPSimpleType TDPSimple { get; private set; } = TDPSimpleType.HIGH;

        public (bool ok, int min, int max, int def) CoreClockRange
        {
            get
            {
                int min = 0;
                int max = 0;
                int def = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_core_clocks_min_max_default_delta(BusID, ref min, ref max, ref def);
                if (ok == RET_OK) return (true, min, max, def);
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_core_clocks_min_max_default_delta failed with error code {ok}", _delayedLogging);
                return (false, -1, -1, -1);
            }
        }

        public (bool ok, int min, int max, int def) CoreVoltageRange
        {
            get
            {
                int min = 0;
                int max = 0;
                int def = 0;
                int ok = INTEL_IGCL.nhm_intel_device_get_core_voltage_min_max_default_delta(BusID, ref min, ref max, ref def);
                if (ok == RET_OK) return (true, min, max, def);
                Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_core_voltage_min_max_default_delta failed with error code {ok}", _delayedLogging);
                return (false, -1, -1, -1);
            }
        }

        public (bool ok, int min, int max, int def) GetTDPLimits()
        {
            int min = -1;
            int max = -1;
            int def = -1;
            var ok = INTEL_IGCL.nhm_intel_device_get_power_limit_min_max_default(BusID, ref min, ref max, ref def);
            if (ok == RET_OK) return (true, min, max, def);
            Logger.InfoDelayed(LogTag, $"nhm_intel_device_get_power_limit_min_max_default failed with error code {ok}", _delayedLogging);
            return (false, -1, -1, -1);
        }

        private static double? PowerLevelToTDPPercentage(TDPSimpleType level) =>
            level switch
            {
                TDPSimpleType.LOW => 0.6d, // 60%
                TDPSimpleType.MEDIUM => 0.8d,// 80%
                TDPSimpleType.HIGH => 1.0d, // 100%
                _ => null,
            };

        public bool SetCoreClock(int coreClock)
        {
            var ok = INTEL_IGCL.nhm_intel_device_set_core_clocks_delta(BusID, coreClock);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_intel_device_set_core_clocks_delta failed with error code {ok}", _delayedLogging);
            return false;
        }

        public bool SetCoreVoltage(int coreVoltage)
        {
            var ok = INTEL_IGCL.nhm_intel_device_set_core_voltage_delta(BusID, coreVoltage);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_intel_device_set_core_voltage_delta failed with error code {ok}", _delayedLogging);
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

            var execRet = INTEL_IGCL.nhm_intel_device_set_power_limit(BusID, (int)percentage);
            Logger.Info(LogTag, $"SetTDPPercentage returned {execRet}.");
            return execRet == RET_OK;
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
            var execRet = NVIDIA_MON.nhm_nvidia_device_set_tdp(BusID, (int)(percentage * 100));
            if (execRet == RET_OK) TDPSimple = level;
            Logger.Info(LogTag, $"SetTDPSimple {execRet}.");
            return execRet == RET_OK;
        }

        public bool ResetCoreClock()
        {
            var ok = INTEL_IGCL.nhm_intel_device_reset_core_clocks_delta(BusID);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_intel_device_reset_core_clocks_delta failed with error code {ok}", _delayedLogging);
            return false;
        }

        public bool ResetCoreVoltage()
        {
            var ok = INTEL_IGCL.nhm_intel_device_reset_core_voltage_delta(BusID);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_intel_device_reset_core_voltage_delta failed with error code {ok}", _delayedLogging);
            return false;
        }
    }
}
