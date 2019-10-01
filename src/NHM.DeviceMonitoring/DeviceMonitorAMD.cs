using ATI.ADL;
using NHM.Common;
using NHM.DeviceMonitoring.AMD;
using NHM.DeviceMonitoring.TDP;
using System;

namespace NHM.DeviceMonitoring
{
    internal class DeviceMonitorAMD : DeviceMonitor, IFanSpeedRPM, ILoad, IPowerUsage, ITemp, ITDP
    {
        public int BusID { get; private set; }
        private readonly IntPtr _adlContext;
        private bool _powerHasFailed;

        private AmdBusIDInfo[] _adapderIndexInfos;
        private int _adapterIndex {
            get
            {
                foreach (var info in _adapderIndexInfos)
                {
                    return info.Adl1Index;
                }
                return -1;
            }
        } // For ADL
        private int _adapterIndex2
        {
            get
            {
                foreach (var info in _adapderIndexInfos)
                {
                    return info.Adl1Index;
                }
                return -1;
            }
        }// For ADL2

        private static readonly TimeSpan _delayedLogging = TimeSpan.FromMinutes(0.5);

        private string LogTag => $"DeviceMonitorAMD-uuid({UUID})-busid({BusID})";

        internal DeviceMonitorAMD(string uuid, int busID, params AmdBusIDInfo[] infos)
        {
            UUID = uuid;
            BusID = busID;
            _adapderIndexInfos = infos;
            ADL.ADL2_Main_Control_Create.Delegate?.Invoke(ADL.ADL_Main_Memory_Alloc, 0, ref _adlContext);

            try
            {
                // set to high by default
                var defaultLevel = TDPSimpleType.HIGH;
                var success = SetTDPSimple(defaultLevel);
                if (!success)
                {
                    Logger.Info(LogTag, $"Cannot set power target ({defaultLevel.ToString()}) for device with BusID={BusID}");
                }
            }
            catch (Exception e)
            {
                Logger.Error(LogTag, $"Getting power info failed with message \"{e.Message}\", disabling power setting");
            }
        }

        public int FanSpeedRPM
        {
            get
            {
                var adlf = new ADLFanSpeedValue
                {
                    SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_RPM
                };
                var result = ADL.ADL_Overdrive5_FanSpeed_Get.Delegate(_adapterIndex, 0, ref adlf);
                if (result != ADL.ADL_SUCCESS)
                {
                    Logger.InfoDelayed(LogTag, $"ADL fan getting failed with error code {result}", _delayedLogging);
                    return -1;
                }
                return adlf.FanSpeed;
            }
        }

        public float Temp
        {
            get
            {
                var adlt = new ADLTemperature();
                var result = ADL.ADL_Overdrive5_Temperature_Get.Delegate(_adapterIndex, 0, ref adlt);
                if (result != ADL.ADL_SUCCESS)
                {
                    Logger.InfoDelayed(LogTag, $"ADL temp getting failed with error code {result}", _delayedLogging);
                    return -1;
                }
                return adlt.Temperature * 0.001f;
            }
        }

        public float Load
        {
            get
            {
                var adlp = new ADLPMActivity();
                var result = ADL.ADL_Overdrive5_CurrentActivity_Get.Delegate(_adapterIndex, ref adlp);
                if (result != ADL.ADL_SUCCESS)
                {
                    Logger.InfoDelayed(LogTag, $"ADL load getting failed with error code {result}", _delayedLogging);
                    return -1;
                }
                return adlp.ActivityPercent;
            }
        }

        public double PowerUsage
        {
            get
            {
                var power = -1;
                if (!_powerHasFailed && _adlContext != IntPtr.Zero && ADL.ADL2_Overdrive6_CurrentPower_Get.Delegate != null)
                {
                    var result = ADL.ADL2_Overdrive6_CurrentPower_Get.Delegate(_adlContext, _adapterIndex2, 1, ref power);
                    if (result == ADL.ADL_SUCCESS)
                    {
                        return (double)power / (1 << 8);
                    }

                    // Only alert once
                    Logger.InfoDelayed(LogTag, $"ADL power getting failed with code {result} for GPU BusID={BusID}. Turning off power for this GPU.", _delayedLogging);
                    _powerHasFailed = true;
                }

                return power;
            }
        }

        // AMD tdpLimit
        private bool SetTdpADL(bool usePercentage, double rawOrPercValue)
        {
            try
            {
                if (ADL.ADL2_OverdriveN_PowerLimit_Set.Delegate == null)
                {
                    Logger.ErrorDelayed(LogTag, $"SetTdpADL ADL2_OverdriveN_PowerLimit_Set not supported", TimeSpan.FromSeconds(30));
                    return false;
                }
                if (ADL.ADL2_OverdriveN_CapabilitiesX2_Get.Delegate == null)
                {
                    Logger.ErrorDelayed(LogTag, $"SetTdpADL ADL2_OverdriveN_CapabilitiesX2_Get not supported", TimeSpan.FromSeconds(30));
                    return false;
                }
                var ADLODNCapabilitiesX2 = new ADLODNCapabilitiesX2();
                var ret = ADL.ADL2_OverdriveN_CapabilitiesX2_Get.Delegate(_adlContext, _adapterIndex, ref ADLODNCapabilitiesX2);
                if (ret != ADL.ADL_SUCCESS)
                {
                    Logger.Info(LogTag, $"SetTdpADL ADL2_OverdriveN_CapabilitiesX2_Get returned {ret}");
                    return false;
                }
                // We limit 100% to the default as max
                int tdpLimit = 0;
                if (usePercentage)
                {
                    var limit = RangeCalculator.CalculateValue(rawOrPercValue, ADLODNCapabilitiesX2.power.iMin, ADLODNCapabilitiesX2.power.iDefault);
                    tdpLimit = (int)limit;
                }
                else
                {
                    var limit = Math.Max((int)rawOrPercValue, ADLODNCapabilitiesX2.power.iMin);
                    tdpLimit = Math.Min(limit, ADLODNCapabilitiesX2.power.iDefault);
                }

                //set value here
                var lpODPowerLimit = new ADLODNPowerLimitSetting();
                lpODPowerLimit.iMode = (int)ADLODNControlType.ODNControlType_Manual;
                lpODPowerLimit.iTDPLimit = tdpLimit;
                var adlRet = ADL.ADL2_OverdriveN_PowerLimit_Set.Delegate(_adlContext, _adapterIndex, ref lpODPowerLimit);
                if (adlRet != ADL.ADL_SUCCESS)
                {
                    Logger.Error(LogTag, $"ADL2_OverdriveN_PowerLimit_Set failed with code {adlRet} for GPU BusID={BusID}.");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Info(LogTag, $"SetTdpADL {e.Message}");
            }

            return false;
        }

        #region ITDP
        public TDPSettingType SettingType { get; set; } = TDPSettingType.SIMPLE;

        public double TDPRaw
        {
            get
            {
                if (ADL.ADL2_OverdriveN_PowerLimit_Get.Delegate != null)
                {
                    var odNPowerControl = new ADLODNPowerLimitSetting();
                    var adlRet = ADL.ADL2_OverdriveN_PowerLimit_Get.Delegate(_adlContext, _adapterIndex, ref odNPowerControl);
                    if (adlRet != ADL.ADL_SUCCESS)
                    {
                        Logger.Error(LogTag, $"TDPRaw ADL2_OverdriveN_PowerLimit_Get failed with code {adlRet} for GPU BusID={BusID}.");
                        return -1;
                    }
                    return odNPowerControl.iTDPLimit;
                }
                else
                {
                    Logger.InfoDelayed(LogTag, $"TDPRaw ADL2_OverdriveN_PowerLimit_Get not supported", TimeSpan.FromSeconds(30));
                }
                return -1;
            }
        }

        public double TDPPercentage
        {
            get
            {
                if (ADL.ADL2_OverdriveN_PowerLimit_Get.Delegate == null)
                {
                    Logger.ErrorDelayed(LogTag, $"TDPPercentage ADL2_OverdriveN_PowerLimit_Get not supported", TimeSpan.FromSeconds(30));
                    return -1;
                }
                if (ADL.ADL2_OverdriveN_CapabilitiesX2_Get.Delegate == null)
                {
                    Logger.ErrorDelayed(LogTag, $"TDPPercentage ADL2_OverdriveN_CapabilitiesX2_Get not supported", TimeSpan.FromSeconds(30));
                    return -1;
                }

                var odNPowerControl = new ADLODNPowerLimitSetting();
                var adlRet = ADL.ADL2_OverdriveN_PowerLimit_Get.Delegate(_adlContext, _adapterIndex, ref odNPowerControl);
                if (adlRet != ADL.ADL_SUCCESS)
                {
                    Logger.Error(LogTag, $"TDPPercentage ADL2_OverdriveN_PowerLimit_Get failed with code {adlRet} for GPU BusID={BusID}.");
                    return -1;
                }
                var currentTDP = (double)odNPowerControl.iTDPLimit;

                var ADLODNCapabilitiesX2 = new ADLODNCapabilitiesX2();
                var ret = ADL.ADL2_OverdriveN_CapabilitiesX2_Get.Delegate(_adlContext, _adapterIndex, ref ADLODNCapabilitiesX2);
                if (ret != ADL.ADL_SUCCESS)
                {
                    Logger.Info(LogTag, $"TDPPercentage ADL2_OverdriveN_CapabilitiesX2_Get returned {ret}");
                    return -1;
                }

                // We limit 100% to the default as max
                var tdpPerc = RangeCalculator.CalculatePercentage(currentTDP, ADLODNCapabilitiesX2.power.iMin, ADLODNCapabilitiesX2.power.iDefault);
                return tdpPerc; // 0.0d - 1.0d
            }
        }

        public TDPSimpleType TDPSimple { get; private set; } = TDPSimpleType.HIGH;

        public bool SetTDPRaw(double raw)
        {
            if (DeviceMonitorManager.DisableDevicePowerModeSettings)
            {
                Logger.InfoDelayed(LogTag, $"SetTDPRaw Disabled DeviceMonitorManager.DisableDevicePowerModeSettings==true", TimeSpan.FromSeconds(30));
                return false;
            }
            return SetTdpADL(false, raw);
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
            if (percentage > 1.0d)
            {
                Logger.Error(LogTag, $"SetTDPPercentage {percentage} out of bounds. Setting to 1.0d");
                percentage = 1.0d;
            }
            return SetTdpADL(true, percentage);
        }
        private static double? PowerLevelToTDPPercentage(TDPSimpleType level)
        {
            switch (level)
            {
                case TDPSimpleType.LOW: return 0.5d; // 50%
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
            var execRet = SetTdpADL(true, percentage.Value);
            if (execRet) TDPSimple = level;
            Logger.Info(LogTag, $"SetTDPSimple {execRet}.");
            return execRet;
        }
        #endregion ITDP
    }
}
