using NHM.Common;
using NHM.DeviceMonitoring.AMD;
using NHM.DeviceMonitoring.Core_clock;
using NHM.DeviceMonitoring.Core_voltage;
using NHM.DeviceMonitoring.Memory_clock;
using NHM.DeviceMonitoring.NVIDIA;
using NHM.DeviceMonitoring.TDP;
using System;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorAMD : DeviceMonitor, IFanSpeedRPM, IGetFanSpeedPercentage, ILoad, IPowerUsage, ITemp, ITDP, IMemControllerLoad, ISpecialTemps, ICoreClock, IMemoryClock, ICoreClockSet, IMemoryClockSet, IMemoryClockRange, ICoreClockRange, ISetFanSpeedPercentage, IResetFanSpeed, ICoreVoltageRange, ICoreVoltage, ICoreVoltageSet, ITDPLimits
    {
        public int BusID { get; private set; }
        private const int RET_OK = 0;


        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(0.5);

        private string LogTag => $"DeviceMonitorAMD-uuid({UUID})-busid({BusID})";

        internal DeviceMonitorAMD(string uuid, int busID)
        {
            UUID = uuid;
            BusID = busID;

            try
            {
                // set to high by default
                var defaultLevel = TDPSimpleType.HIGH;
                var success = SetTDPSimple(defaultLevel);
                if (!success)
                {
                    Logger.Info(LogTag, $"Cannot set power target ({defaultLevel}) for device with BusID={BusID}");
                }
            }
            catch (Exception e)
            {
                Logger.Error(LogTag, $"Getting power info failed with message \"{e.Message}\", disabling power setting");
            }
        }

        (bool ok, string driverVer, string catalystVersion, string crimsonVersion, string catalystWebLink) GetAMDVersions()
        {
            AMD_ODN.ADLVersionsInfoX2 versions = new AMD_ODN.ADLVersionsInfoX2(new char[256], new char[256], new char[256], new char[256]);
            int ok = AMD_ODN.nhm_amd_device_get_driver_version(BusID,ref versions);
            if(ok != 0)
            {
                return (false, "", "", "", "");
            }
            return (true, new string(versions.StrDriverVer).Trim('\0'), new string(versions.StrCatalystVersion).Trim('\0'), 
                new string(versions.StrCrimsonVersion).Trim('\0'), new string(versions.StrCatalystWebLink).Trim('\0'));
        }

        (int status, int percentage) IGetFanSpeedPercentage.GetFanSpeedPercentage()
        {
            int percentage = 0;
            int ok = AMD_ODN.nhm_amd_device_get_fan_speed_percentage(BusID, ref percentage);
            if (ok != 0) Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_fan_speed_percentage failed with error code {ok}", _delayedLogging);
            return (ok, percentage);
        }

        public int FanSpeedRPM
        {
            get
            {
                int rpm = 0;
                int ok = AMD_ODN.nhm_amd_device_get_fan_speed_rpm(BusID, ref rpm);
                if (ok == 0) return rpm;
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_fan_speed_rpm failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public float Temp
        {
            get
            {
                int temperature = 0;
                int ok = AMD_ODN.nhm_amd_device_get_temperature(BusID, ref temperature);
                if (ok == 0) return temperature;
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_temperature failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public float Load
        {
            get
            {
                int load_perc = 0;
                int ok = AMD_ODN.nhm_amd_device_get_load_percentage(BusID, ref load_perc);
                if (ok == 0) return load_perc;
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_load_percentage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public double PowerUsage
        {
            get
            {
                int power_usage = 0;
                int ok = AMD_ODN.nhm_amd_device_get_power_usage(BusID, ref power_usage);
                if (ok == 0) return power_usage;
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_power_usage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        // AMD tdpLimit
        private bool SetTdpADL(double percValue)
        {
            int min = 0, max = 0, defaultValue = 0;
            int ok = AMD_ODN.nhm_amd_device_get_tdp_min_max_default(BusID, ref min, ref max, ref defaultValue);
            if (ok != 0)
            {
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_tdp_ranges failed with error code {ok}", _delayedLogging);
                return false;
            }
#if NHMWS4
            int ok2 = AMD_ODN.nhm_amd_device_set_tdp(BusID, (int)percValue);
            if (ok2 != 0)
            {
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_set_tdp failed with error code {ok}", _delayedLogging);
                return false;
            }
            return true;
#else
            // We limit 100% to the default as max
            var limit = 0.0d;
            if (percValue > 1)
            {
                limit = RangeCalculator.CalculateValueAMD(percValue - 1, defaultValue, max);
            }
            else
            {
                limit = RangeCalculator.CalculateValueAMD(percValue, min, defaultValue);
            }

            int ok2 = AMD_ODN.nhm_amd_device_set_tdp(BusID, (int)limit);
            if (ok2 != 0)
            {
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_set_tdp failed with error code {ok2}", _delayedLogging);
                return false;
            }
            return true;
#endif
        }

#region ITDP
        public TDPSettingType SettingType { get; set; } = TDPSettingType.SIMPLE;

        public double TDPPercentage
        {
            get
            {
                int tdpRaw = 0;
                int ok = AMD_ODN.nhm_amd_device_get_tdp(BusID, ref tdpRaw);
                if (ok != 0)
                {
                    Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_tdp failed with error code {ok}", _delayedLogging);
                    return -1;
                }
                int min = 0, max = 0, defaultValue = 0;
                int ok2 = AMD_ODN.nhm_amd_device_get_tdp_min_max_default(BusID, ref min, ref max, ref defaultValue);
                if (ok2 != 0)
                {
                    Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_tdp_ranges failed with error code {ok}", _delayedLogging);
                    return -1;
                }
                // We limit 100% to the default as max
                var tdpPerc = RangeCalculator.CalculatePercentage(tdpRaw, min, defaultValue);
                return tdpPerc; // 0.0d - 1.0d
            }
        }

        public TDPSimpleType TDPSimple { get; private set; } = TDPSimpleType.HIGH;

        public bool SetTDP(double percentage)
        {
            //if (percentage < 0.0d)
            //{
            //    Logger.Error(LogTag, $"SetTDPPercentage {percentage} out of bounds. Setting to 0.0d");
            //    percentage = 0.0d;
            //}
            Logger.Info(LogTag, $"SetTDPPercentage setting to {percentage}.");
            return SetTdpADL(percentage);
        }
        private static double? PowerLevelToTDPPercentage(TDPSimpleType level) =>
            level switch
            {
                TDPSimpleType.LOW => 0.6d, // 60%
                TDPSimpleType.MEDIUM => 0.8d,// 80%
                TDPSimpleType.HIGH => 1.0d, // 100%
                _ => null,
            };

        public bool SetTDPSimple(TDPSimpleType level)
        {
            var percentage = PowerLevelToTDPPercentage(level);
            if (!percentage.HasValue)
            {
                Logger.Error(LogTag, $"SetTDPSimple unkown PowerLevel {level}. Defaulting to {TDPSimpleType.HIGH}");
                level = TDPSimpleType.HIGH;
                percentage = PowerLevelToTDPPercentage(level);
            }
            Logger.Info(LogTag, $"SetTDPSimple setting PowerLevel to {level}.");
            var execRet = SetTdpADL(percentage.Value);
            if (execRet) TDPSimple = level;
            Logger.Info(LogTag, $"SetTDPSimple {execRet}.");
            return execRet;
        }
#endregion ITDP
        private (int vramTemp, int hotspotTemp) GetSpecialTemperatures()
        {
            int vramT = 0;
            int hotspotT = 0;
            int ok = AMD_ODN.nhm_amd_device_get_special_temperatures(BusID, ref hotspotT, ref vramT);
            if (ok == 0) return (vramT, hotspotT);
            Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_special_temperatures failed with error code {ok}", _delayedLogging);
            return (-1, -1);
        }

        public bool SetMemoryClock(int memoryClock)
        {
            var ok = AMD_ODN.nhm_amd_device_set_memory_clocks(BusID, memoryClock);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_set_memory_clocks failed with error code {ok}", _delayedLogging);
            return false;
        }

        public bool SetCoreClock(int coreClock)
        {
            var ok = AMD_ODN.nhm_amd_device_set_core_clocks(BusID, coreClock);
            if(ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_set_core_clocks failed with error code {ok}", _delayedLogging);
            return false;
        }

        public bool SetCoreVoltage(int coreVoltage)
        {
            var ok = AMD_ODN.nhm_amd_device_set_voltage(BusID, coreVoltage);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_set_voltage failed with error code {ok}", _delayedLogging);
            return false;
        }

        public bool SetFanSpeedPercentage(int percentage)
        {
            var ok = percentage <= 0 ? AMD_ODN.nhm_amd_device_reset_fan_speed_percentage(BusID) : AMD_ODN.nhm_amd_device_set_fan_speed_percentage(BusID, percentage);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_set_fan_speed_percentage failed with error code {ok}", _delayedLogging);
            return false;
        }

        public bool ResetFanSpeedPercentage()
        {
            var ok = AMD_ODN.nhm_amd_device_reset_fan_speed_percentage(BusID);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_reset_fan_speed_percentage failed with error code {ok}", _delayedLogging);
            return false;
        }

        public bool ResetCoreClock()
        {
            var ok = AMD_ODN.nhm_amd_device_reset_core_clocks(BusID);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_reset_core_clocks failed with error code {ok}", _delayedLogging);
            return false;
        }

        public bool ResetMemoryClock()
        {
            var ok = AMD_ODN.nhm_amd_device_reset_memory_clocks(BusID);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_reset_memory_clocks failed with error code {ok}", _delayedLogging);
            return false;
        }
        
        public bool ResetCoreVoltage()
        {
            var ok = AMD_ODN.nhm_amd_device_reset_voltage(BusID);
            if (ok == RET_OK) return true;
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_reset_voltage failed with error code {ok}", _delayedLogging);
            return false;
        }

        public (bool ok, int min, int max, int def) GetTDPLimits()
        {
            int min = -1;
            int max = -1;
            int def = -1;
            var ok = AMD_ODN.nhm_amd_device_get_tdp_min_max_default(BusID, ref min, ref max, ref def);
            if (ok == RET_OK) return (true, min, max, def);
            Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_tdp_min_max_default failed with error code {ok}", _delayedLogging);
            return (false, -1, -1, -1);
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
                int ok = AMD_ODN.nhm_amd_device_get_memory_controller_load(BusID, ref memCtrlLoad);
                if (ok == 0) return (memCtrlLoad);
                Logger.InfoDelayed(LogTag, $"nhm_nvidia_device_get_memory_controller_load failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public int MemoryClock
        {
            get
            {
                int memoryClock = 0;
                int ok = AMD_ODN.nhm_amd_device_get_memory_clocks(BusID, ref memoryClock);
                if (ok == RET_OK) return memoryClock;
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_memory_clocks failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public int CoreClock
        {
            get
            {
                int coreClock = 0;
                int ok = AMD_ODN.nhm_amd_device_get_core_clocks(BusID, ref coreClock);
                if (ok == RET_OK) return coreClock;
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_core_clocks failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public int CoreVoltage
        {
            get
            {
                int coreVoltage = 0;
                int ok = AMD_ODN.nhm_amd_device_get_voltage(BusID, ref coreVoltage);
                if (ok == RET_OK) return coreVoltage;
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_voltage failed with error code {ok}", _delayedLogging);
                return -1;
            }
        }

        public (bool ok, int min, int max, int def) MemoryClockRange
        {
            get
            {
                int min = 0;
                int max = 0;
                int def = 0;
                int ok = AMD_ODN.nhm_amd_device_get_memory_clocks_min_max_default(BusID, ref min, ref max, ref def);
                if (ok == RET_OK) return (true, min, max, def);
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_memory_clocks_min_max_default failed with error code {ok}", _delayedLogging);
                return (false, -1, -1, -1);
            }
        }

        public (bool ok, int min, int max, int def) CoreClockRange
        {
            get
            {
                int min = 0;
                int max = 0;
                int def = 0;
                int ok = AMD_ODN.nhm_amd_device_get_core_clocks_min_max_default(BusID, ref min, ref max, ref def);
                if (ok == RET_OK) return (true, min, max, def);
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_core_clocks_min_max_default failed with error code {ok}", _delayedLogging);
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
                int ok = AMD_ODN.nhm_amd_device_get_voltage_min_max_default(BusID, ref min, ref max, ref def);
                if (ok == RET_OK) return (true, min, max, def);
                Logger.InfoDelayed(LogTag, $"nhm_amd_device_get_voltage_min_max_default failed with error code {ok}", _delayedLogging);
                return (false, -1, -1, -1);
            }
        }
    }
}
